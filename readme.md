# CadFlow

> **Hardware-driven AutoCAD 2027 workflow for macOS** — a Loupedeck plugin that replaces keyboard-heavy command entry with tactile buttons, velocity-scaled rotary panning, and a structured 60+ command hierarchy.

<br>

```
Loupedeck Hardware  →  CadFlowPlugin  →  CadToolFolder  →  AcadSend  →  osascript  →  AutoCAD 2027
     Button/Encoder      Entry & Lifecycle    Action System     IPC Bridge    execution
```

<br>

## What Is This?

CadFlow is a `.NET 8 / C#` middleware plugin that bridges a **Loupedeck console** with **AutoCAD 2027 on macOS (Apple Silicon)**. Instead of typing commands or hunting for toolbar icons, every frequently used CAD operation lives one button-press away on physical hardware you can feel without looking.

The plugin communicates with AutoCAD entirely through **AppleScript** injected via `/usr/bin/osascript` — no AutoCAD API, no COM interop, just reliable keystroke injection that works transparently inside active commands.

<br>

## Features

| Capability | Detail |
|---|---|
| **60+ CAD operations** | Organised into Draw, Modify, Precision, and Advanced tool folders |
| **8-slot contextual grid** | Slots 0–5 are sub-commands; Slot 6 = Escape; Slot 7 = Enter |
| **Velocity-scaled pan** | Non-linear rotary encoder → `_-pan` (transparent, works mid-command) |
| **Delta accumulation** | Encoder events queue while osascript is busy — nothing is dropped |
| **Auto-profile switching** | Plugin activates automatically when AutoCAD gains focus |
| **Zero image assets** | Every on-device icon rendered at runtime via `BitmapBuilder.FillRectangle()` |
| **Mode toggles** | ORTHOMODE, POLARMODE, OSMODE, AUTOSNAP — all work inside active commands |
| **Global Undo / Redo** | `⌘Z` / `⌘⇧Z` always available regardless of active tool |

<br>

## System Requirements

- macOS (Apple Silicon — M1/M2/M3)
- AutoCAD 2027 macOS (`com.autodesk.autocad2027`)
- Loupedeck console with SDK support
- .NET 8 SDK

<br>

## Architecture

CadFlow has three distinct layers:

### Layer 1 — Plugin Entry & Lifecycle
`CadFlowPlugin` (inherits Loupedeck `Plugin`) bootstraps logging and embedded resources. `CadFlowApplication` (inherits `ClientApplication`) watches for AutoCAD's bundle ID and triggers automatic profile activation.

### Layer 2 — Action & Adjustment Handlers
`CadToolFolder` groups expose an 8-button contextual grid per tool. `ToolContext` is a static state machine that tracks which folder is open, letting the Confirm/Cancel slots always know their context.

### Layer 3 — IPC Bridge (`AcadSend`)
Static utility that builds AppleScript strings and executes them through `/usr/bin/osascript`. Three execution modes:

| Method | Behaviour | Used for |
|---|---|---|
| `Send(cmd)` | Blocking, 3000ms timeout | All tool folder button presses |
| `SendAsync(cmd)` | Fire-and-forget | NudgeEngine rotary encoder input |
| `SendEnter/Escape/Tab()` | Raw key codes (36 / 53 / 48) | Slot 7 Confirm, Slot 6 Cancel |
| `SendShortcut(key, mods)` | Modifier key combos | Undo, Redo, and shortcuts |
| `SendToggle(sysVar)` | CAD system variable toggles | Mode buttons |

All commands pass through `Sanitise()` before injection — lowercased, backslashes and quotes escaped, newlines stripped.

<br>

## NudgeEngine — Rotary Pan

High-frequency encoder input is handled by a singleton `NudgeEngine` that accumulates fractional deltas to prevent missed events when osascript is in-flight.

**Velocity formula:**
```
velocity = sign(raw) × (FineScale + TurboCoeff × |raw|^Exponent)
```

| Constant | Value | Purpose |
|---|---|---|
| `FineScale` | 0.6 | Base movement per encoder tick |
| `TurboCoeff` | 1.5 | Fast-spin multiplier |
| `Exponent` | 1.65 | Non-linear acceleration curve |
| `MaxDelta` | 12 | Hard clamp on raw input |
| `IdleResetMs` | 60 ms | Gap that wipes stale accumulators |

**Pan command sent to AutoCAD:**
```
'_-pan 0,0 {dx},{dy} \n
```

The leading `'` makes `_-pan` a *transparent* command — it works inside any other active AutoCAD command without cancelling it.

<br>

## Tool Groups

### CAD Draw — 6 tools
`Line` · `Polyline` · `Circle` · `Arc` · `Rectangle` · `Polygon`

### CAD Modify — 9 tools
`Move` · `Copy` · `Rotate` · `Scale` · `Trim` · `Extend` · `Fillet` · `Mirror` · `Offset`

### CAD Precision — 4 tools
`DimLinear` · `DimAligned` · `DimAngular` · `Align`

### CAD Advanced — 4 tools
`Array (Rect · Polar · Path)` · `Text` · `MText`

<br>

## Global Actions

Always available, regardless of which tool folder is active:

**QuickRingFolder** — rapid viewport ops
- Zoom Extents (`_zoom e`), Zoom Window (`_zoom w`), Zoom Previous (`_zoom p`)
- Layer Manager (`_layer`), Scale Objects (`_scale`), Regen (`_regen`)

**Undo / Redo** — `⌘Z` / `⌘⇧Z`

**Mode Toggles** — via `SendToggle()`, work mid-command:
- ORTHOMODE → `⌘L`
- POLARMODE → `⌘U`
- OSMODE → `F3`
- AUTOSNAP → `⌘⇧T`

<br>

## Project Structure

```
MyPlugin/
├── MyPlugin.sln
├── README.md
└── src/
    ├── CadFlow.csproj
    ├── CadFlowPlugin.cs          # Plugin entry point & lifecycle
    ├── CadFlowApplication.cs     # AutoCAD bundle ID detection
    │
    ├── Actions/                  # CadToolFolder subclasses (Draw, Modify, Precision, Advanced)
    │   ├── Draw/
    │   ├── Modify/
    │   ├── Precision/
    │   ├── Advanced/
    │   └── Global/               # QuickRingFolder, Undo, Redo, NudgeX/Y
    │
    ├── Context/
    │   └── ToolContext.cs        # Static state machine — tracks active folder
    │
    ├── Helpers/
    │   ├── AcadSend.cs           # AppleScript IPC bridge
    │   ├── NudgeEngine.cs        # Velocity-scaled encoder accumulator
    │   ├── PluginLog.cs          # SDK logging wrapper
    │   └── PluginResources.cs    # Embedded resource loader
    │
    └── package/
        └── metadata/
            └── LoupedeckPackage.yaml
```

<br>

## Build & Install

### 1. Set environment variables (Apple Silicon)

```bash
export DOTNET_ROOT=$(brew --prefix dotnet@8)/libexec
export DOTNET_ROOT_ARM64=$DOTNET_ROOT
export PATH=$DOTNET_ROOT:$PATH
```

### 2. Build

```bash
cd src
dotnet build
```

Output: `bin/Debug/CadFlow.dll`

### 3. Package & install

```bash
# Package: zip metadata + DLL + icons + profiles
# Install via the Loupedeck CLI:
logiplugintool install ./CadFlow.lplug4
```

### 4. Enable in Loupedeck

Loupedeck app → **Marketplace** → **Local plugins** → Enable **CadFlow**

### 5. Launch AutoCAD

The CadFlow profile activates automatically when AutoCAD 2027 becomes the frontmost window.

<br>

## Package Manifest

```yaml
# src/package/metadata/LoupedeckPackage.yaml
PackageName:         Loupedeck.MyPlugin
DisplayName:         CadFlow
Version:             1.0.0
ApplicationTarget:   com.autodesk.autocad2027
DllName:             CadFlow.dll
```

<br>

## Key Classes

| Class | File | Role |
|---|---|---|
| `CadFlowPlugin` | `CadFlowPlugin.cs` | Entry point, inherits `Plugin` |
| `CadFlowApplication` | `CadFlowApplication.cs` | Bundle ID watcher, inherits `ClientApplication` |
| `ToolContext` | `Context/ToolContext.cs` | Static state machine for active folder |
| `AcadSend` | `Helpers/AcadSend.cs` | AppleScript builder & executor |
| `NudgeEngine` | `Helpers/NudgeEngine.cs` | Singleton encoder accumulator |
| `PluginLog` | `Helpers/PluginLog.cs` | Logging (Info / Warn / Error) |
| `PluginResources` | `Helpers/PluginResources.cs` | Embedded SVG icon loader |

**`CtxBtn`** — lightweight struct held by each folder slot: `Label`, `Cmd` (AutoCAD string or special tag), `IsToggle`.

<br>

## Extending CadFlow

**Add a new tool group** — create a new `CadToolFolder` subclass in `src/Actions/`, define your `CtxBtn[]` array (slots 0–5), and register it in `CadFlowPlugin.cs`.

**Add a global command** — subclass `PluginDynamicCommand` in `src/Actions/Global/` and call the appropriate `AcadSend` method.

**Add a new encoder axis** — subclass `PluginDynamicAdjustment` and forward diffs to `NudgeEngine.Instance.FeedX()` or `FeedY()`.

<br>

## Glossary

| Term | Meaning |
|---|---|
| `PluginDynamicFolder` | Loupedeck SDK base for folders that populate a button grid at runtime |
| `PluginDynamicAdjustment` | SDK base for dial/encoder adjustments |
| `ClientApplication` | SDK class that links a plugin to a macOS bundle ID |
| `CtxBtn` | Struct: Label + Cmd + IsToggle per grid slot |
| `ToolContext` | Static tracker for the currently open `CadToolFolder` |
| `AcadSend` | Generates and executes AppleScript keystroke injection |
| `NudgeEngine` | Velocity-scaled encoder singleton with async dispatch |
| `osascript` | macOS CLI for AppleScript execution (`System.Diagnostics.Process`) |
| `_-pan` | AutoCAD transparent pan command (apostrophe prefix = works mid-command) |
| `BitmapBuilder` | Loupedeck SDK class for on-device pixel-art tile rendering |
| `logiplugintool` | Loupedeck CLI for packaging, installing, and debugging plugins |
| `DefaultProfile20.lp5` | Pre-configured profile bundled for first-install key mapping |

<br>

---

**CadFlow** · Loupedeck × AutoCAD 2027 · macOS · .NET 8 · C#
[github.com/AlokDadhich/Cadflow](https://github.com/AlokDadhich/Cadflow)