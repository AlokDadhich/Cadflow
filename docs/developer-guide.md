# CADFlow — Developer Guide

Everything you need to hack on CADFlow: how to set up, how the code is organised, how to add new tools / buttons / toggles, and how to debug the moving parts.

---

## 1. Local setup

### Prerequisites

- macOS 13+ (Apple Silicon or Intel)
- .NET 8 SDK
- AutoCAD 2027 for Mac (for end-to-end testing)
- Logi Plugin Service (`/Applications/Utilities/LogiPluginService.app`) — installed with Logi Options+
- A Loupedeck CT or Logitech MX Creative Console
- (Optional) the `logiplugintool` CLI, used by the VSCode tasks

```bash
# Install .NET 8 through Homebrew
brew install --cask dotnet-sdk@8

# Point MSBuild at the Homebrew SDK (add to your shell profile)
export DOTNET_ROOT="$(brew --prefix dotnet@8)/libexec"
export DOTNET_ROOT_ARM64="$DOTNET_ROOT"
export PATH="$DOTNET_ROOT:$PATH"

# Clone & build
git clone https://github.com/AlokDadhich/Cadflow.git
cd Cadflow
dotnet build src/CadFlow.csproj -c Debug
```

After the first successful build, the project will have:

1. Produced `bin/Debug/bin/CadFlow.dll` at the solution root (note: `Directory.Build.props` redirects output *up one level* from `src`).
2. Copied `src/package/**` next to the DLL.
3. Written `~/Library/Application\ Support/Logi/LogiPluginService/Plugins/CadFlow.link` pointing back at `bin/Debug/`.
4. Fired `open loupedeck:plugin/CadFlow/reload` to live-reload the Logi Plugin Service.

### Grant macOS permissions

The Logi Plugin Service needs **Accessibility** and **Automation (AutoCAD 2027)** granted in *System Settings → Privacy & Security*. Without those, `osascript` calls silently fail and buttons appear dead.

Quick way to check:

```bash
osascript -e 'tell application "System Events" to tell process "AutoCAD 2027" to keystroke "z" using {command down}'
```

If that does nothing, fix permissions before debugging CADFlow itself.

---

## 2. Project layout — what to touch for what

| If you want to… | Edit this |
| --- | --- |
| Add a new draft primitive (e.g. `Spline`) | A subclass of `DrawToolFolder` in `src/Actions/Draw/DrawTools.cs` |
| Add a new edit primitive (e.g. `Stretch`) | A subclass of `ModifyToolFolder` in `src/Actions/Modify/ModifyTools.cs` |
| Add a new dimension or align command | `PrecisionToolFolder` subclass in `src/Actions/Precision/PrecisionTools.cs` |
| Add a heavyweight tool (array, text, hatch) | `AdvancedToolFolder` subclass in `src/Actions/Advanced/AdvancedTools.cs` |
| Change the Quick Ring's slots | `Slots` array in `src/Actions/QuickRingFolder.cs` |
| Tune pan feel | Tuning constants at the top of `src/Actions/NudgeEngine.cs` |
| Add a top-row shortcut (e.g. `SaveAction`) | A new `PluginDynamicCommand` in `src/Actions/` |
| Support a new mid-command toggle | Extend the `switch` in `AcadSend.SendToggle` |
| Target a different AutoCAD build | `AcadSend.AcadApp`, `NudgeEngine.AcadApp`, `CadFlowApplication.GetBundleName` |
| Change the default profile | `src/package/profiles/DefaultProfile20.lp5` (binary — edit via Logi Options+ and copy out) |
| Add fancy SVG tiles | Drop files into `src/package/actionicons/` and load with `PluginResources.ReadImage` |

---

## 3. Coding conventions

The code base is small but consistent. Keep the following when contributing:

### 3.1 Style

- **Language version**: `latest` (C# 12). `Directory.Build.props` sets this globally.
- **Nullable**: currently `disable` — do **not** introduce `#nullable enable` piecemeal; change at the csproj level if we go that route.
- **Braces**: Allman style, single-line `if`/`else` permitted for trivial returns.
- **Using placement**: inside the namespace, as in `AcadSend.cs`, to keep each file self-contained.
- **Comments**: use the big `══════` block comments to announce sections of functionality. Tables, not prose, for per-slot documentation.
- **Naming**:

  | Kind | Pattern | Example |
  | --- | --- | --- |
  | Class | `PascalCase`, ends with domain role | `DrawToolFolder`, `UndoAction`, `NudgeEngine` |
  | Method | `PascalCase` | `SendToggle` |
  | Private field | `_camelCase` | `_accumX`, `_current` |
  | Constant | `PascalCase` (not SCREAMING) | `FineScale`, `AcadApp` |
  | Slot parameter | `btn{0..7}` | `"btn0"` |

### 3.2 Dependencies

- Only one external binary reference: `PluginApi.dll` from Logi Plugin Service. Do **not** pull in NuGet packages without discussion — the Logi host prefers a small surface.
- Treat `AcadSend` and `NudgeEngine` as the **only** places that talk to `osascript`. Any new code that wants to interact with AutoCAD should call through these.

### 3.3 Error handling

- Never let an exception escape a callback that the Logi SDK invokes. The pattern used throughout `AcadSend` is `try { … } catch { }`.
- For diagnostics, emit through `PluginLog.Info/Warning/Error`, never `Console.WriteLine`.
- If you want richer context, catch with `(Exception ex)` and log `PluginLog.Error(ex, "context string")`.

---

## 4. Adding a new tool folder (step-by-step)

Suppose you want to add a **Hatch** tool to the *Advanced* group.

### 4.1 Pick the group subclass

```csharp
// src/Actions/Advanced/AdvancedTools.cs
public class HatchTool : AdvancedToolFolder
{
    public HatchTool() { Init(); }
    protected override string ToolName => "Hatch";
    protected override string ToolCmd  => "hatch";

    protected override CtxBtn[] ContextBtns => new[]
    {
        new CtxBtn("Pick",    ""),          // slot 0 — click a region
        new CtxBtn("Select",  "s"),         // slot 1 — pick objects
        new CtxBtn("Pattern", "p"),         // slot 2 — choose pattern
        new CtxBtn("Scale",   "hpscale"),   // slot 3 — set hatch scale
        new CtxBtn("Angle",   "hpang"),     // slot 4 — set hatch angle
        new CtxBtn("Origin",  "o"),         // slot 5 — reset origin
        new CtxBtn("LastVal", "@"),         // slot 6 — reserved Close
        new CtxBtn("Confirm", ""),          // slot 7 — reserved Confirm
    };
}
```

**Rules:**

- `ContextBtns` must be length 8.
- Slot 6 is drawn by the base class as a Close / back arrow regardless of label. Many existing tools put a functional command there (`LastVal`) anyway; decide which you want, but don't try to change slot 6's visual.
- Slot 7 always fires `Enter` via `AcadSend.SendEnter()`; its label is purely decorative.

### 4.2 Pick the right command name

AutoCAD commands should be **lower-case**, **non-localized** (no leading underscore — `AcadSend.Sanitise` would lowercase the letter anyway). If the command has a dash variant you want (e.g. `-hatch` to force the command-line flow), put the dash in the string: `"-hatch"`.

### 4.3 Decide toggle vs. action

`IsToggle = true` means the `Cmd` is fed into `AcadSend.SendToggle`. Only the four known sysvars are handled: `ORTHOMODE`, `POLARMODE`, `OSMODE`, `AUTOSNAP`. If you need a new toggle, extend the `switch` in `AcadSend.SendToggle` first, then set `IsToggle: true` in your `CtxBtn`.

### 4.4 Build & register

No central registry is needed — the Loupedeck SDK auto-discovers `PluginDynamicFolder` subclasses. `dotnet build` and the new tool shows up in Logi Options+ under the "CAD Advanced" group.

### 4.5 Optional: give it a real icon

If the default label-dispatched glyph isn't what you want, add a `case "Pick": …` branch to `CadToolFolder.DrawIcon` or (preferred) drop an SVG into `src/package/actionicons/Loupedeck.MyPlugin.CadHatch.svg` and, in your tool, override `GetCommandImage` to read it via `PluginResources.ReadImage`.

---

## 5. Adding a new top-level command

For something that acts on a single press (Save, PurgeAll, Layer Previous, etc.):

```csharp
// src/Actions/SaveAction.cs
namespace Loupedeck.CadFlow
{
    public class SaveAction : PluginDynamicCommand
    {
        public SaveAction()
            : base("Save", "Save the drawing (⌘S)", "CadFlow") { }

        protected override void RunCommand(string actionParameter)
            => AcadSend.SendShortcut("s", "command down");

        protected override BitmapImage GetCommandImage(
            string actionParameter, PluginImageSize imageSize)
            => /* build a BitmapBuilder tile — see UndoRedoActions.cs */;
    }
}
```

Conventions:

- Group name is `"CadFlow"` for every top-level action — keeps the UI list tidy.
- Draw the tile with `BitmapBuilder` and a 60-pixel virtual canvas (match `UndoAction`).
- Use `AcadSend.Send` / `SendShortcut` — never invent a new AppleScript spawner.

---

## 6. Tile rendering — surviving `BitmapBuilder`

The Loupedeck SDK gives you a pixel-level API. Conventions used throughout the code base:

```csharp
var b   = new BitmapBuilder(size);
int W   = b.Width;
int H   = b.Height;
int cx  = W / 2;
double sy = H / 45.0;                        // 45-pixel virtual canvas
double sx = W / 45.0;
int Y(int y)  => (int)(y * sy);              // Y-coordinate scaler
int SY(int h) => Math.Max(1, (int)(h * sy)); // height scaler, min 1 px
int SX(int w) => Math.Max(1, (int)(w * sx)); // width scaler, min 1 px
```

Then lay rectangles out from the 45-pixel design: `cx - 8, Y(20), SX(16), SY(3)`. The `Math.Max(1, …)` ensures tiles still show at the tiniest device screens (e.g. adjustment tiles on a Loupedeck CT).

Colour semantics (stick to them for visual consistency):

| Purpose | RGB |
| --- | --- |
| Confirm / Enter | `(70, 210, 70)` |
| Close / Escape / nav-back | `(80, 140, 255)` |
| Tab / special | `(255, 155, 50)` |
| Toggle | `(110, 150, 255)` |
| Default foreground | `(200, 200, 210)` |
| Background | black `(0, 0, 0)` |

---

## 7. Testing

There is currently **no automated test project**. High-leverage additions you might make:

1. Unit-test the `CtxBtn[]` shape for every concrete tool — fail CI if any tool has != 8 slots.
2. Mock `AcadSend` (behind an `IAcadSend` interface) and verify each slot in each tool produces the expected AppleScript snippet.
3. Unit-test `NudgeEngine.Velocity` at the boundaries (`0`, `±1`, `±MaxDelta`, `±100`) and verify monotonic growth.
4. Integration-test via a headless AppleScript sink: replace `osascript` with a shell shim that writes its args to a file, then assert the file contents.

Until those exist, manual testing runs through the following:

```
1. Open AutoCAD 2027.
2. Watch the Logi Plugin Service log: tail -f ~/Library/Logs/Logi/LogiPluginService.log
3. Touch each button and verify the right command appears in AutoCAD's command line.
4. Spin the Roller / Scroller — view should pan smoothly in-command and idle.
5. Toggle Ortho / Polar mid-LINE — the status line should flip without exiting LINE.
```

---

## 8. Debugging

| Symptom | Likely cause | Where to look |
| --- | --- | --- |
| Button does nothing | `osascript` can't reach AutoCAD (permissions) | System Settings → Privacy & Security → Accessibility / Automation |
| Button types but doesn't Enter | `Sanitise` stripped the `\n` correctly (it should), but AutoCAD is not the frontmost window | `AcadSend.KeystrokeScript` includes `activate` — confirm AutoCAD isn't covered by a modal |
| Pan is stepped / jumpy | Velocity curve exponent too high | Tune `Exponent` and `TurboCoeff` in `NudgeEngine.cs` |
| Tile renders blank | `SlotIndex` returned `-1` | Verify `param` is in the form `btn{0..7}` |
| Wrong icon | Label doesn't match any `case` in `DrawIcon` | Add a new `case` or rely on the diamond fallback |
| Toggle doesn't work | Sysvar not in `AcadSend.SendToggle` switch | Add a new branch mapping to the right `⌘`-shortcut |
| Plugin doesn't reload after build | `.link` file wasn't written or `open loupedeck:` scheme failed | Check Logi Plugin Service is running; re-register by restarting the service |
| "PluginApi not found" build error | `PluginApiDir` MSBuild property points nowhere | Check `/Applications/Utilities/LogiPluginService.app` exists, or override `<PluginApiDir>` in `src/CadFlow.csproj` |

To tail live plugin logs:

```bash
log stream --predicate 'process == "LogiPluginService"' --info
```

`PluginLog.Info` output appears here — every `ToolContext.Activate` / `Deactivate` is logged.

---

## 9. Common pitfalls

1. **Slot 7 is NOT free.** The base class always sends `Enter`. Do not put a toggle or tab in slot 7 — it will be shadowed by the hard-coded `SendEnter()`.
2. **Forgetting `Init()` in a new tool folder.** The ctor must call `Init()` so the abstract base can set `DisplayName` and `GroupName`. Without it the folder appears unnamed in Logi Options+.
3. **Using uppercase in `Cmd`.** `Sanitise` lower-cases the command, so `"LINE"` becomes `"line"` — works, but strictly wrong. Only use uppercase for sysvar toggles (`"ORTHOMODE"`), which go through a different path.
4. **Adding `\r` or other whitespace.** Only `\n` is understood by the sanitiser; anything else becomes literal text. Use `\n` to inject an explicit Enter inside a multi-part command (e.g. `"zoom\nw\n"`).
5. **Forgetting the AutoCAD bundle ID when you bump versions.** Three places hold the string: `AcadSend.AcadApp`, `NudgeEngine.AcadApp`, and `CadFlowApplication.GetBundleName`. Change all three.
6. **Mutating `Slots` / `ContextBtns` at runtime.** Both are `static readonly` or derived per-activation by design; rebuilding them on every press is expensive and breaks tile caching.
7. **Emitting more than one `osascript` per encoder tick.** The `NudgeEngine` accumulator exists to prevent fork-bombing. Don't "fix" perceived lag by removing it.

---

## 10. Release checklist

Before tagging a new version:

- [ ] `dotnet build -c Release` — no warnings.
- [ ] Bump `version:` in `src/package/metadata/LoupedeckPackage.yaml`.
- [ ] Bump the PR / README changelog section.
- [ ] Run the VSCode **Package Plugin** task: `logiplugintool pack ./bin/Release ./CadFlow.lplug4`.
- [ ] Install the `.lplug4` on a clean machine and walk the [User Guide](user-guide.md) example workflows.
- [ ] Commit, tag `vX.Y.Z`, push, create a GitHub Release with the `.lplug4` attached.

---

## 11. Further reading

- [Loupedeck Plugin API documentation](https://developer.loupedeck.com/) — `PluginDynamicCommand`, `PluginDynamicFolder`, `PluginDynamicAdjustment` base classes.
- [Logi Options+ SDK](https://www.logitech.com/en-us/software/logi-options-plus.html) — installation and host behaviour.
- [AutoCAD Commands List](https://help.autodesk.com/view/ACD/2024/ENU/?guid=GUID-2DE4BE53-4B2C-49EC-8A4F-4E14EEF2B3FE) — canonical command names you'll use in `CtxBtn.Cmd`.
- [AppleScript System Events reference](https://developer.apple.com/documentation/coreservices/applescript) — especially `keystroke`, `key code`, and `using`.
