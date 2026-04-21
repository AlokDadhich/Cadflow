Here’s your **fully rewritten, clean, professional README.md**, with explicit mention of **AutoCAD 2027 plugin for macOS** and proper structure.

Just copy this into your `README.md`:

---

```markdown
# CADFlow – AutoCAD 2027 Plugin for macOS (Loupedeck Integration)

CADFlow is a custom plugin developed for AutoCAD 2027 on macOS, designed to integrate with the Loupedeck console for high-efficiency, hardware-driven CAD workflows. It provides structured tool access, encoder-based controls, and context-aware actions to reduce reliance on traditional keyboard input.

---

## Overview

CADFlow enhances AutoCAD interaction by mapping commonly used tools and operations to a physical control interface. The system is designed to improve speed, consistency, and usability for repetitive CAD tasks.

---

## Key Features

- Structured tool folders with multi-layer navigation  
- Encoder (dial) support for real-time parameter control  
- Context-aware button layouts for different tools  
- Direct AutoCAD command execution  
- Modular and extensible architecture  

---

## Target Environment

- AutoCAD 2027 (macOS version)  
- macOS (Apple Silicon recommended)  
- Loupedeck Console with SDK support  
- .NET 8 (installed via Homebrew or manual setup)  

---

## Project Structure

```

MyPlugin/
│
├── MyPlugin.sln
├── README.md
├── src/
│   ├── CadFlow.csproj
│   ├── CadFlowPlugin.cs
│   ├── CadFlowApplication.cs
│   │
│   ├── Actions/
│   ├── Context/
│   ├── Helpers/
│   ├── package/
│   │
│   └── Directory.Build.props
│
├── bin/
├── obj/
└── .vscode/

````

---

## Build Instructions

### macOS (.NET via Homebrew)

Set environment variables (required for some Apple Silicon setups):

```bash
export DOTNET_ROOT=$(brew --prefix dotnet@8)/libexec
export DOTNET_ROOT_ARM64=$DOTNET_ROOT
export PATH=$DOTNET_ROOT:$PATH
````

### Build the Project

```bash
cd src
dotnet build
```

---

## Installation

1. Build the project
2. Locate the compiled plugin in:

   ```
   bin/Debug/
   ```
3. Copy the plugin to the Loupedeck plugin directory
4. Launch Loupedeck software
5. Open AutoCAD 2027 (macOS)
6. Verify that CADFlow controls are active

---

## Usage

* Navigate tools using Loupedeck buttons
* Use encoders for continuous input (zoom, adjustments, etc.)
* Trigger AutoCAD commands directly from hardware controls
* Access sub-tools through structured folder layers

---

## Design Principles

* Efficiency: Reduce time spent on repetitive CAD operations
* Consistency: Fixed layouts for improved muscle memory
* Clarity: Logical grouping of tools and actions
* Extensibility: Easy addition of new tools and features

---

## Customization

* Add new tools in `src/Actions/`
* Modify layouts in `src/Context/`
* Extend shared logic via `src/Helpers/`

---

## Known Limitations

* SVG icon scaling and dynamic loading require improvement
* Some tool confirmation behaviors may be inconsistent
* Certain AutoCAD command mappings may require refinement

---

## Notes

* This plugin is specifically built for AutoCAD 2027 on macOS
* Optimized for Loupedeck hardware interaction
* Not intended as a general-purpose CAD extension framework

