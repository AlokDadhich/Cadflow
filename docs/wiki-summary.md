# CADFlow — Wiki Summary

This file is intentionally short-but-comprehensive: a DeepWiki-style orientation for new contributors. If you are about to read the source for the first time, read this file first.

---

## What is CADFlow, in one paragraph?

CADFlow is a **Loupedeck / Logitech MX Creative Console** plugin for **AutoCAD 2027 on macOS**. It is a single `CadFlow.dll` (`.NET 8`, Loupedeck Plugin API v4) hosted by the Logi Plugin Service. Every button press, folder activation, or encoder tick on the device translates into a short **AppleScript** that `osascript` delivers to AutoCAD via **System Events**. There is no AutoCAD SDK involvement; the plugin drives AutoCAD purely through synthetic keystrokes.

## How the code is wired (30-second tour)

```
CadFlowPlugin.cs            ← plugin entry, starts PluginLog / PluginResources
CadFlowApplication.cs       ← bundle-ID hook that auto-activates CADFlow when AutoCAD is front

Actions/
  AcadSend.cs               ← static: all osascript output (Send, SendShortcut, SendToggle, SendEnter…)
  NudgeEngine.cs            ← singleton: Roller/Scroller → transparent '_-pan'
  NudgeXAdjustment.cs       ← PluginDynamicAdjustment → NudgeEngine.FeedX
  NudgeYAdjustment.cs       ← PluginDynamicAdjustment → NudgeEngine.FeedY
  UndoRedoActions.cs        ← ⌘Z / ⇧⌘Z dynamic commands, self-drawn tiles
  QuickRingFolder.cs        ← 8-slot parameter folder (Zoom/LW/Layer/Angle/Scale/Hatch/TxtH/Confirm)
  CadToolFolder.cs          ← abstract base for every per-tool folder + tile renderer
  Draw/DrawTools.cs         ← Line, Polyline, Circle, Arc, Polygon, Rectangle
  Modify/ModifyTools.cs     ← Move, Copy, Offset, Trim, Extend, Rotate, Scale, Mirror, Fillet, Chamfer
  Precision/PrecisionTools.cs ← Dimension, Align
  Advanced/AdvancedTools.cs ← Array, Text

Context/
  ToolContext.cs            ← static registry of the currently active CadToolFolder

Helpers/
  PluginLog.cs              ← facade over PluginLogFile
  PluginResources.cs        ← facade over embedded-assembly helpers

package/
  metadata/LoupedeckPackage.yaml ← plugin manifest (name, version, devices)
  metadata/Icon256x256.png       ← plugin icon
  actionicons/*.svg               ← optional per-tool SVGs (not loaded today)
  profiles/DefaultProfile20.lp5   ← binary profile shipped out-of-the-box
```

## One-line purpose for every file

| File | One-line purpose |
| --- | --- |
| `src/CadFlowPlugin.cs` | Plugin entry — initialises shared helpers. |
| `src/CadFlowApplication.cs` | Declares that CADFlow targets AutoCAD 2027's macOS bundle. |
| `src/CadFlow.csproj` | net8.0 build, references `PluginApi.dll`, post-build copies package and writes `.link`. |
| `src/Directory.Build.props` | Sets `LangVersion=latest` and redirects build output above `src/`. |
| `src/Actions/AcadSend.cs` | Single static class that owns every AppleScript emission. |
| `src/Actions/NudgeEngine.cs` | Singleton that smooths encoder deltas into transparent `'_-pan` commands. |
| `src/Actions/NudgeXAdjustment.cs` | Maps Roller → `NudgeEngine.FeedX`. |
| `src/Actions/NudgeYAdjustment.cs` | Maps Scroller → `NudgeEngine.FeedY`. |
| `src/Actions/UndoRedoActions.cs` | Two top-row dynamic commands — `⌘Z` / `⇧⌘Z`. |
| `src/Actions/QuickRingFolder.cs` | Second-level parameter folder with 8 labelled slots. |
| `src/Actions/CadToolFolder.cs` | Abstract base + `CtxBtn` struct + `DrawTile`/`DrawIcon` renderer + 4 group subclasses. |
| `src/Actions/Draw/DrawTools.cs` | Concrete Draw tools (Line, Polyline, Circle, Arc, Polygon, Rectangle). |
| `src/Actions/Modify/ModifyTools.cs` | Concrete Modify tools (Move…Chamfer). |
| `src/Actions/Precision/PrecisionTools.cs` | Concrete Precision tools (Dimension, Align). |
| `src/Actions/Advanced/AdvancedTools.cs` | Concrete Advanced tools (Array, Text). |
| `src/Context/ToolContext.cs` | Static reference to the active `CadToolFolder`. |
| `src/Helpers/PluginLog.cs` | Static log facade used throughout the plugin. |
| `src/Helpers/PluginResources.cs` | Static resource-lookup facade. |
| `src/package/metadata/LoupedeckPackage.yaml` | Plugin manifest — name, version, supported devices, capabilities. |
| `src/package/metadata/Icon256x256.png` | Plugin icon displayed in Logi Options+. |
| `src/package/actionicons/*.svg` | Optional per-tool SVG icons (not loaded at runtime yet). |
| `src/package/profiles/DefaultProfile20.lp5` | Out-of-the-box device profile (binary). |
| `MyPlugin.sln` | Minimal single-project solution. |
| `.vscode/tasks.json` | Build / package / install / uninstall tasks. |
| `.vscode/launch.json` | VSCode debug launch config. |
| `.gitignore` | Standard .NET ignore list. |
| `README.md` | High-level project overview. |
| `docs/*.md` | The documentation suite you are currently reading. |

## How it all connects (reading order)

1. **`CadFlowPlugin`** is loaded by the Logi Plugin Service. Its ctor calls `PluginLog.Init` and `PluginResources.Init`.
2. The service inspects **`CadFlowApplication`** → sees bundle `com.autodesk.autocad2027` and decides this plugin is "interested" in AutoCAD.
3. When AutoCAD becomes frontmost, the Logi host swaps the device to the CADFlow profile (`DefaultProfile20.lp5`).
4. The profile binds physical controls to **`UndoAction`**, **`RedoAction`**, **`QuickRingFolder`**, **`NudgeXAdjustment`**, **`NudgeYAdjustment`**, and a selection of concrete `CadToolFolder` subclasses.
5. When the user presses a tool button, its folder `Activate()`s: `ToolContext.Activate(this)` → `AcadSend.Send(ToolCmd)` → AutoCAD starts that command.
6. When the user presses any of the folder's 8 context buttons, `RunCommand("btn{i}")` routes through `AcadSend` — either a plain keystroke, a modifier shortcut, a sysvar toggle, `Enter`, or `Tab`.
7. When the user spins Roller/Scroller, `NudgeXAdjustment`/`NudgeYAdjustment` forwards `diff` into `NudgeEngine`, which fires a transparent `'_-pan` AppleScript.
8. When the user navigates back, the folder `Deactivate()`s: `ToolContext.Deactivate(this)` → `AcadSend.SendEscape()`.

## Mental model for new developers

- Think of CADFlow as a **typewriter controller**. Its only capability is "activate AutoCAD and type". Everything clever in the UI is just pre-canned sequences of letters, numbers, and modifier keys.
- Any feature that requires *reading back* AutoCAD state (selection, current layer, sysvar values) is out of scope with today's architecture — AppleScript on AutoCAD Mac is one-way.
- Tool folders are **opinionated** about slots 6 and 7. They are drawn by the base class and slot 7 always dispatches `Enter`. Don't fight that.
- The plugin keeps **essentially no state**: `ToolContext.Current` plus the two `NudgeEngine` accumulators. That makes the code very easy to reason about — any bug is in how input is mapped to AppleScript, not in hidden mutable state.
- All heavy UI work (tile rendering) is done procedurally inside `BitmapBuilder`. There is no runtime asset pipeline you need to learn.

## If you're here to…

| Task | Start at |
| --- | --- |
| Understand the architecture | [`architecture.md`](architecture.md) |
| Look up what a class does | [`api.md`](api.md) |
| Build or extend the plugin | [`developer-guide.md`](developer-guide.md) |
| Use the plugin day-to-day | [`user-guide.md`](user-guide.md) |
| Find every file's one-line purpose | This file, [§ One-line purpose for every file](#one-line-purpose-for-every-file) |

## Glossary

| Term | Meaning |
| --- | --- |
| **Loupedeck CT / MX Creative Console** | The supported physical devices (`LoupedeckCtFamily`). Loupedeck was acquired by Logitech; both devices share the same plugin API. |
| **Logi Plugin Service** | The macOS background app (`LogiPluginService.app`) that hosts managed plugins and talks to the device. |
| **PluginApi.dll** | The SDK assembly CADFlow references. Bundled with the Plugin Service. |
| **`PluginDynamicCommand`** | A single-press action on the device. |
| **`PluginDynamicFolder`** | A sub-layout on the device that hosts its own buttons. |
| **`PluginDynamicAdjustment`** | An encoder / dial with incremental delta callbacks. |
| **`ClientApplication`** | SDK hook that maps a plugin to a specific desktop application. |
| **Transparent command** | AutoCAD command invoked with a leading apostrophe, runs *inside* a currently active command without cancelling it. CADFlow uses `'_-pan` for Roller/Scroller. |
| **Sysvar toggle** | An AutoCAD system variable flipped via a keyboard shortcut (`⌘L` for `ORTHOMODE`, `⌘U` for `POLARMODE`, `F3` for `OSMODE`, `⇧⌘T` for `AUTOSNAP`). |
| **`CtxBtn`** | CADFlow's 3-tuple `(Label, Cmd, IsToggle)` for one slot in a tool folder. |
| **Slot** | One of the 8 tiles in a dynamic folder, addressed as `btn0` … `btn7`. |
| **Quick Ring** | The 8-slot parameter-helper folder in `QuickRingFolder.cs`. |
| **NudgeEngine** | The singleton that turns wheel deltas into accumulated transparent-pan commands. |
