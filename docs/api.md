# CADFlow — API Reference

This document lists every public / internal type in `CadFlow.dll` and documents the contract of each method: purpose, inputs, outputs, and side effects. Types are grouped by module.

> All types live in the `Loupedeck.CadFlow` namespace. "Side effects" refers to anything observable outside the method: filesystem, child process, UI, static state.

---

## 1. Plugin entry — `src/CadFlowPlugin.cs`, `src/CadFlowApplication.cs`

### `class CadFlowPlugin : Plugin`

The Loupedeck plugin entry-point. Instantiated by the Logi Plugin Service.

| Member | Kind | Purpose | Inputs | Outputs | Side effects |
| --- | --- | --- | --- | --- | --- |
| `UsesApplicationApiOnly` | property | Tells the SDK this plugin uses plugin-side API in addition to the application-API bridge. | — | `false` | — |
| `HasNoApplication` | property | Declares that a `ClientApplication` is supplied. | — | `false` | — |
| `CadFlowPlugin()` | ctor | Wires up shared helpers. | — | — | `PluginLog.Init(this.Log)` • `PluginResources.Init(this.Assembly)` |
| `Load()` | override | Plugin is being loaded by the host. | — | `void` | Currently no-op. |
| `Unload()` | override | Plugin is being unloaded. | — | `void` | Currently no-op. |

### `class CadFlowApplication : ClientApplication`

Declares the target app that CADFlow binds to.

| Member | Purpose | Returns |
| --- | --- | --- |
| `GetBundleName()` | macOS bundle identifier of the app that triggers auto-profile-switch. | `"com.autodesk.autocad2027"` |
| `GetProcessName()` | Windows process name. Empty because this plugin is macOS-only. | `""` |
| `GetApplicationStatus()` | Reports whether the app is running. Currently returns `Unknown`; the Logi Plugin Service falls back to bundle-based detection. | `ClientApplicationStatus.Unknown` |

---

## 2. Output layer — `src/Actions/AcadSend.cs`

### `internal static class AcadSend`

Every keystroke CADFlow emits goes through `AcadSend`. It is the single choke point for `osascript`.

> **Constants**
>
> - `AcadApp = "AutoCAD 2027"` — the name AppleScript uses in `tell application "…"`. Change this in one place to target a different AutoCAD build.

#### Public methods

| Method | Signature | Purpose | Inputs | Output | Side effects |
| --- | --- | --- | --- | --- | --- |
| `Send` | `void Send(string command)` | Blocking type-and-enter. Used by buttons. | `command` — text to type into AutoCAD; `\n` chars are stripped, backslashes and quotes escaped, lower-cased. | `void` | Writes a temp `.scpt`, runs `osascript`, waits up to 3 s, deletes the file. |
| `SendAsync` | `void SendAsync(string command)` | Non-blocking type-and-enter. Reserved for high-frequency input (unused today — kept for future wheel-into-command features). | Same as `Send`. | `void` | Writes a temp `.scpt`, runs `osascript` fire-and-forget. **Temp file is not cleaned up.** |
| `SendEnter` | `void SendEnter()` | Presses Return (key code 36) inside AutoCAD. | — | `void` | Temp script run blocking. |
| `SendEscape` | `void SendEscape()` | Presses Escape (key code 53). Used by `Deactivate` to cancel any in-progress command. | — | `void` | Blocking run. |
| `SendTab` | `void SendTab()` | Presses Tab (key code 48). Toggles between angle and length prompt in LINE / PLINE. | — | `void` | Blocking run. |
| `SendShortcut` | `void SendShortcut(string key, string modifiers)` | Generic modifier-key shortcut. | `key` — literal character to send; `modifiers` — AppleScript modifier list, e.g. `"command down"` or `"command down, shift down"`. | `void` | Blocking run. |
| `SendToggle` | `void SendToggle(string sysVar)` | AutoCAD Mac-specific sysvar toggles that work **mid-command**. Supported values: `"ORTHOMODE"` → ⌘L, `"POLARMODE"` → ⌘U, `"OSMODE"` → F3, `"AUTOSNAP"` → ⇧⌘T. Unknown sysvars are silently ignored. | `sysVar` — case-insensitive. | `void` | Blocking run. |

All public methods swallow exceptions from `osascript` so a drop-in AppleScript failure cannot crash the Logi Plugin Service.

#### Private helpers

| Method | Purpose |
| --- | --- |
| `Sanitise(cmd)` | Escapes backslashes and quotes, strips newlines, lower-cases the input so that AutoCAD's lexer sees a safe token. |
| `KeystrokeScript(safe)` | Returns an AppleScript that activates AutoCAD, waits 150 ms, and types the characters followed by Return. |
| `KeyCodeScript(code)` | AppleScript that presses a single key code. Uses `key code 36 / 53 / 48 / 99` for Enter / Esc / Tab / F3. |
| `ShortcutScript(key, modifiers)` | AppleScript for keystroke-with-modifiers. |
| `RunScript(script, blocking)` | Writes the script to a temp `.scpt` (for key / shortcut variants) or spawns with `-e` directly, runs `osascript`, optionally waits up to 3 s, and deletes the file in the blocking path. |

---

## 3. Panning engine — `src/Actions/NudgeEngine.cs`

### `internal sealed class NudgeEngine`

Singleton. Converts raw encoder deltas into transparent `'_-pan` AppleScript events.

> **Tuning constants**
>
> | Constant | Value | Effect |
> | --- | --- | --- |
> | `FineScale` | `0.6` | Base gain for tiny spins — gives pixel-level precision. |
> | `TurboCoeff` | `1.5` | Multiplier on the non-linear term. |
> | `Exponent` | `1.65` | Exponent applied to `|diff|`. Higher = more aggressive turbo. |
> | `MaxDelta` | `12` | Clamps raw `diff` to avoid runaway pans. |
> | `AcadApp` | `"AutoCAD 2027"` | Same constant as `AcadSend`. |

| Member | Signature | Purpose | Side effects |
| --- | --- | --- | --- |
| `Instance` | `static readonly NudgeEngine` | Global accessor. Single-instance. | — |
| `FeedX` | `void FeedX(int diff)` | Feeds a horizontal encoder step. | Mutates `_accumX`. When the accumulator crosses a whole pixel, emits a pan AppleScript with `dx = -(int)accumX` (negative to keep the drawing's perceived drag direction). |
| `FeedY` | `void FeedY(int diff)` | Feeds a vertical encoder step. | Mutates `_accumY`. Emits `dy = (int)accumY`. |
| `Velocity(raw)` | `static double` | Maps raw integer deltas into fractional pixel velocity: `sign · (FineScale + TurboCoeff · |raw|^Exponent)`, with `raw` clamped to ±`MaxDelta`. | — |
| `SendPan(dx, dy)` | `static void` | Fires a single compact inline AppleScript `keystroke "'_-pan"⏎"0,0"⏎"dx,dy"⏎` via `osascript -e`. | Starts a process, **does not wait** or clean up. |

Characteristics:

- Thread-safety is inherited from "only one encoder callback at a time" per Loupedeck SDK guarantees — fields are not locked.
- Because `SendPan` does not block, the overhead per wheel tick is a single `fork + exec`. Fine for typical wheel rates.
- The accumulators carry fractional state across calls, so at the slowest spin the user still eventually gets a single-pixel pan.

---

## 4. Adjustments — `NudgeXAdjustment.cs`, `NudgeYAdjustment.cs`

Both are tiny `PluginDynamicAdjustment` subclasses.

### `class NudgeXAdjustment : PluginDynamicAdjustment`

| Member | Purpose |
| --- | --- |
| `ctor` | Registers as "Pan X" in group "CadFlow", `hasReset = false`. |
| `ApplyAdjustment(string actionParameter, int diff)` | Forwards to `NudgeEngine.Instance.FeedX(diff)`. |
| `GetAdjustmentValue(string actionParameter)` | Returns empty string — no numeric display on the device. |

### `class NudgeYAdjustment : PluginDynamicAdjustment`

Mirror of the above, calling `FeedY`. Display name `"Pan Y"`.

---

## 5. Top-row commands — `src/Actions/UndoRedoActions.cs`

### `class UndoAction : PluginDynamicCommand`

| Member | Purpose |
| --- | --- |
| `ctor` | Registers "Undo" in group "CadFlow". |
| `RunCommand(string actionParameter)` | Calls `AcadSend.SendShortcut("z", "command down")`. |
| `GetCommandImage(string, PluginImageSize)` | Returns a procedurally drawn blue counter-clockwise arrow tile via `BitmapBuilder`. |

### `class RedoAction : PluginDynamicCommand`

Same shape; fires `AcadSend.SendShortcut("Z", "command down, shift down")` (AutoCAD Mac uses ⇧⌘Z) and draws a green clockwise arrow.

---

## 6. Quick Ring — `src/Actions/QuickRingFolder.cs`

### `class QuickRingFolder : PluginDynamicFolder`

An 8-slot parameter folder. Each slot is a `RingSlot`:

```csharp
private struct RingSlot
{
    public readonly string Label;
    public readonly string CmdUp;    // text sent on press (the "up" command historically, used as single-press)
    public readonly string CmdDown;  // reserved — would drive encoder-down events
    public readonly byte   AccentR, AccentG, AccentB;
}
```

The built-in `Slots` array is:

| Slot | Label | Command | Accent (R, G, B) | Notes |
| --- | --- | --- | --- | --- |
| 0 | `Zoom` | `zoom\nw\n` / `zoom\np\n` | 90, 180, 255 | Zoom Window / Previous pair. |
| 1 | `LW+` | `lweight\n` | 255, 200, 60 | Opens `LWEIGHT`. |
| 2 | `Layer` | `clayer\n` | 160, 120, 255 | Current layer cycle. |
| 3 | `Angle` | `rotate\n` | 255, 140, 60 | Default-rotate prompt. |
| 4 | `Scale` | `scale\n` | 80, 220, 160 | Default-scale prompt. |
| 5 | `Hatch` | `hpscale\n` | 220, 80, 160 | Hatch pattern scale. |
| 6 | `TxtH` | `textsize\n` | 255, 255, 100 | Text size sysvar. |
| 7 | `Confirm` | *(empty)* | 60, 200, 80 | Slot 7 hard-codes `AcadSend.SendEnter()`. |

Public members:

| Member | Purpose |
| --- | --- |
| `Load() / Unload() / Activate() / Deactivate()` | Standard `PluginDynamicFolder` lifecycle. Activation re-publishes button action names. |
| `GetButtonPressActionNames(DeviceType)` | Emits 8 action names (`btn0`…`btn7`). |
| `GetNavigationArea(DeviceType)` | Returns `None` — the folder uses its entire screen for the slots. |
| `RunCommand(string param)` | Parses `btn{i}` into a slot index, fires `AcadSend.SendEnter()` for slot 7, otherwise `AcadSend.Send(Slots[i].CmdUp)` if the command is non-empty. |
| `GetCommandImage(string, PluginImageSize)` | Returns a procedurally drawn tile for the slot. |
| `GetCommandDisplayName(string, PluginImageSize)` | Returns the slot's `Label`. |
| `DrawTile(size, idx)` | Private — renders the 45-pixel virtual-canvas icon with per-slot glyphs (magnifier, line weights, layer stack, protractor, etc.). |
| `Blank(size)` | Private — black tile used as a fallback. |
| `SlotIndex(string param)` | Private — parses `"btn0"` → `0` through `"btn7"` → `7`, returns `-1` otherwise. |

---

## 7. Tool folders — `src/Actions/CadToolFolder.cs`

### `public struct CtxBtn`

A single context-button declaration.

| Field | Type | Meaning |
| --- | --- | --- |
| `Label` | `string` | Displayed under the icon and used by the tile renderer to pick a glyph (`"Ortho"`, `"OSnap"`, …). |
| `Cmd` | `string` | Text sent to AutoCAD when the button is pressed. Special values: `""` (no-op except slot 7), `"TAB"` (emits Tab), `"ORTHOMODE" / "POLARMODE" / "OSMODE" / "AUTOSNAP"` when combined with `IsToggle=true`. |
| `IsToggle` | `bool` | If `true`, `Cmd` is a sysvar passed to `AcadSend.SendToggle`. If `false`, it is sent as plain text. |

### `public abstract class CadToolFolder : PluginDynamicFolder`

The shared parent of every tool folder. Subclasses override four abstract members:

| Member | Purpose |
| --- | --- |
| `ToolName` | Display name, e.g. `"Line"`. |
| `ToolCmd` | AutoCAD command to start the tool on folder activation (`"line"`, `"circle"`, …). |
| `FolderGroup` | Group label in the Logi Options+ UI. Supplied by the four subclass families (see below). |
| `ContextBtns` | A `CtxBtn[]` of length 8 that defines the 8 slots. Slot 6 is a Close back-button by convention (drawn automatically), slot 7 is Confirm. |

Lifecycle methods (inherited / overridden):

| Method | Effect |
| --- | --- |
| `Load()` / `Unload()` | Return `true`. No resources. |
| `Activate()` | `ToolContext.Activate(this)` → `AcadSend.Send(ToolCmd)` → `ButtonActionNamesChanged()` → refreshes all 8 tiles. |
| `Deactivate()` | `ToolContext.Deactivate(this)` → `AcadSend.SendEscape()` → `this.Close()`. |
| `GetButtonPressActionNames` | Publishes `btn0`…`btn7`. |
| `GetNavigationArea` | `None`. |
| `RunCommand(param)` | Slot-7 fires `SendEnter()`; toggles route to `SendToggle`; `"TAB"` fires `SendTab()`; any other non-empty `Cmd` is typed via `AcadSend.Send`. |
| `GetCommandImage / GetCommandDisplayName` | Delegate to `DrawTile` / `ContextBtns[idx].Label`. |

Rendering internals:

| Method | Purpose |
| --- | --- |
| `DrawTile(size, idx, btn)` | Chooses a colour palette (confirm-green, close-blue, tab-orange, toggle-blue, default-grey) and draws the icon on a black background. |
| `DrawIcon(...)` | Label-dispatched icon renderer: dozens of `case "Ortho": … case "Polar": …` branches draw glyphs on a 45-pixel virtual canvas. Falls back to a generic diamond for unknown labels. |
| `Blank(size)` | Static — solid black tile. |
| `SlotIndex(param)` | Same parsing rule as `QuickRingFolder`. |
| `RefreshAll()` | Iterates `btn0`…`btn7` and calls `CommandImageChanged` to force redraw. |

### Four concrete group subclasses

Each adds only a `FolderGroup`:

```csharp
public abstract class DrawToolFolder      : CadToolFolder { protected override string FolderGroup => "CAD Draw"; }
public abstract class ModifyToolFolder    : CadToolFolder { protected override string FolderGroup => "CAD Modify"; }
public abstract class PrecisionToolFolder : CadToolFolder { protected override string FolderGroup => "CAD Precision"; }
public abstract class AdvancedToolFolder  : CadToolFolder { protected override string FolderGroup => "CAD Advanced"; }
```

---

## 8. Concrete tools

Every concrete tool declares its `ToolName`, `ToolCmd`, and `CtxBtn[]`. The tables below list every `CtxBtn` with the literal string that reaches AutoCAD.

### 8.1 Draw — `src/Actions/Draw/DrawTools.cs`

#### `LineTool` (`line`)

| Slot | Label | Cmd | Kind |
| --- | --- | --- | --- |
| 0 | Ortho | `ORTHOMODE` | toggle |
| 1 | Polar | `POLARMODE` | toggle |
| 2 | OSnap | `OSMODE` | toggle |
| 3 | OTrack | `AUTOSNAP` | toggle |
| 4 | UndoSeg | `u` | action |
| 5 | Tab | `TAB` | tab |
| 6 | Close | `c` | action (close polyline back to start) |
| 7 | Confirm | *(empty)* | enter |

#### `PolylineTool` (`pline`)

| 0 Close `c` • 1 ArcMode `a` • 2 Width `w` • 3 UndoSeg `u` • 4 Snap `snapmode` • 5 Join `j` • 6 Tab `TAB` • 7 Confirm |
| --- |

#### `CircleTool` (`circle`)

| 0 Center — • 1 2Point `2p` • 2 3Point `3p` • 3 Radius `r` • 4 Diameter `d` • 5 Snap `snapmode` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `ArcTool` (`arc`)

| 0 3Point — • 1 StartCtr `c` • 2 StartEnd `e` • 3 Direction `d` • 4 Radius `r` • 5 Snap `snapmode` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `PolygonTool` (`polygon`)

| 0 Sides — • 1 Inscribed `i` • 2 Circum `c` • 3 Edge `e` • 4 Snap `snapmode` • 5 Rotate `r` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `RectangleTool` (`rectang`)

| 0 Chamfer `c` • 1 Fillet `f` • 2 Width `w` • 3 Rotation `r` • 4 Snap `snapmode` • 5 Area `a` • 6 LastVal `@` • 7 Confirm |
| --- |

### 8.2 Modify — `src/Actions/Modify/ModifyTools.cs`

#### `MoveTool` (`move`)

| 0 BasePt — • 1 Displace `d` • 2 Snap `snapmode` • 3 Ortho `orthomode` • 4 AxisLock `_axislock` • 5 CopyMode `c` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `CopyTool` (`copy`)

| 0 BasePt — • 1 Multiple `m` • 2 Snap `snapmode` • 3 RotateCp `r` • 4 MirrorCp `mirror` • 5 ArrayCp `a` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `OffsetTool` (`offset`)

| 0 Distance — • 1 Through `t` • 2 Both `b` • 3 EraseSrc `e` • 4 Layer `l` • 5 Snap `snapmode` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `TrimTool` (`trim`)

| 0 Fence `f` • 1 Crossing `c` • 2 Edge `e` • 3 UndoTrim `u` • 4 Extend `ex` • 5 All `all` • 6 LastSel `p` • 7 Confirm |
| --- |

#### `ExtendTool` (`extend`)

| 0 Fence `f` • 1 Crossing `c` • 2 Edge `e` • 3 UndoExt `u` • 4 Trim `t` • 5 All `all` • 6 LastSel `p` • 7 Confirm |
| --- |

#### `RotateTool` (`rotate`)

| 0 BasePt — • 1 Copy `c` • 2 Ref `r` • 3 AngleLk `_angbase` • 4 Snap `snapmode` • 5 Axis `_axislock` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `ScaleTool` (`scale`)

| 0 BasePt — • 1 Ref `r` • 2 Copy `c` • 3 Uniform — • 4 Xonly `x` • 5 Yonly `y` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `MirrorTool` (`mirror`)

| 0 AxisPick — • 1 Copy `c` • 2 Snap `snapmode` • 3 Horiz `h` • 4 Vert `v` • 5 Rot180 `r` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `FilletTool` (`fillet`)

| 0 Radius `r` • 1 Trim `t` • 2 Multiple `m` • 3 Polyline `p` • 4 Chain `c` • 5 Snap `snapmode` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `ChamferTool` (`chamfer`)

| 0 Dist1 `d` • 1 Dist2 `d` • 2 Angle `a` • 3 Trim `t` • 4 Multiple `m` • 5 Polyline `p` • 6 LastVal `@` • 7 Confirm |
| --- |

### 8.3 Precision — `src/Actions/Precision/PrecisionTools.cs`

#### `DimensionTool` (`dimlinear`)

| 0 Linear `dimlinear` • 1 Aligned `dimaligned` • 2 Radius `dimradius` • 3 Diameter `dimdiameter` • 4 Angle `dimangular` • 5 Continue `dimcontinue` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `AlignTool` (`align`)

| 0 2Point — • 1 3Point — • 2 Scale `s` • 3 Rotate `r` • 4 Snap `snapmode` • 5 Flip `f` • 6 LastVal `@` • 7 Confirm |
| --- |

### 8.4 Advanced — `src/Actions/Advanced/AdvancedTools.cs`

#### `ArrayTool` (`array`)

| 0 Rect `r` • 1 Polar `po` • 2 Path `pa` • 3 Rows `r` • 4 Cols `c` • 5 Space `s` • 6 LastVal `@` • 7 Confirm |
| --- |

#### `TextTool` (`text`)

| 0 Single `text` • 1 Multi `mtext` • 2 Height `h` • 3 Rotate `r` • 4 Style `style` • 5 Align `j` • 6 LastVal `@` • 7 Confirm |
| --- |

---

## 9. Runtime state — `src/Context/ToolContext.cs`

### `internal static class ToolContext`

Tracks the currently-open tool folder so future global commands (a universal Confirm, Exit, or "what tool am I in?" indicator) can key off a single state.

| Member | Purpose |
| --- | --- |
| `Current` | `CadToolFolder?` — the active folder, or `null`. |
| `IsActive` | `true` iff `Current != null`. |
| `Activate(CadToolFolder folder)` | Sets `Current = folder` and logs at Info level. |
| `Deactivate(CadToolFolder folder)` | Clears `Current` if it matches; logs. No-op otherwise (a nested activation cannot race-clear a different folder). |

---

## 10. Helpers

### `internal static class PluginLog` — `src/Helpers/PluginLog.cs`

Thin wrapper around Loupedeck's `PluginLogFile`. Must be initialised with `PluginLog.Init(plugin.Log)` once at startup (done in `CadFlowPlugin` ctor).

| Method | Purpose |
| --- | --- |
| `Init(PluginLogFile)` | Stores the log sink. Throws via `CheckNullArgument` if `null`. |
| `Verbose(text)` / `Verbose(ex, text)` | Verbose entries, `null` sink safe. |
| `Info(text)` / `Info(ex, text)` | Info entries. |
| `Warning(text)` / `Warning(ex, text)` | Warnings. |
| `Error(text)` / `Error(ex, text)` | Errors. |

### `internal static class PluginResources` — `src/Helpers/PluginResources.cs`

Thin facade over Loupedeck's `Assembly` extension methods for embedded resource discovery. Must be initialised with `PluginResources.Init(assembly)` (done in `CadFlowPlugin` ctor).

| Method | Purpose |
| --- | --- |
| `GetFilesInFolder(folderName)` | Returns all embedded resource names under a virtual folder. |
| `FindFile(fileName)` | Returns the full resource name for a short file name, or throws. |
| `FindFiles(regexPattern)` | Returns resource names matching a regex. |
| `GetStream(resourceName)` | Opens a resource as a stream. |
| `ReadTextFile(resourceName)` | Reads a resource as UTF-8 text. |
| `ReadBinaryFile(resourceName)` | Reads a resource as a byte array. |
| `ReadImage(resourceName)` | Reads a resource as a `BitmapImage`. |
| `ExtractFile(resourceName, filePathName)` | Writes a resource to disk. |

---

## 11. Package metadata — `src/package/metadata/LoupedeckPackage.yaml`

Fields the Logi Plugin Service reads when surfacing CADFlow to the user:

| Field | Value |
| --- | --- |
| `type` | `plugin4` |
| `name` / `displayName` | `CadFlow` / `CADFlow` |
| `description` | AutoCAD middleware for MX Creative Console. |
| `pluginFileName` | `CadFlow.dll` |
| `version` | `2.0` |
| `author` | `alokd` |
| `pluginFolderMac` | `bin` |
| `supportedDevices` | `LoupedeckCtFamily` |
| `pluginCapabilities` | `HasApplication`, `ActivatesApplication` |
| `minimumLoupedeckVersion` | `6.0` |
| `license` / `licenseUrl` | `MIT` / https://opensource.org/licenses/MIT |

---

## 12. Command-param string grammar

All dynamic commands and folders accept a `string actionParameter` when triggered. CADFlow uses a single grammar:

```
param  ::= "btn" DIGIT
DIGIT  ::= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7"
```

The parsing helper `SlotIndex` is duplicated in `QuickRingFolder` and `CadToolFolder` because each class keeps its own 8-slot contract. Anything outside `btn0`…`btn7` returns `-1` and is silently dropped.

---

## 13. Logging & error semantics

- `AcadSend.*` and `NudgeEngine.SendPan` wrap their AppleScript invocations in `try { … } catch { }`. A failed `osascript` never propagates to the Logi Plugin Service.
- `PluginLog` is the single logging channel. There is no `Console.WriteLine` anywhere in the codebase.
- Folder lifecycle events log at Info: `"[ToolContext] Activated: Line"` / `"Deactivated: Line"`. Useful for diagnosing "why does my button do nothing?" — check whether the expected folder is `Current`.
