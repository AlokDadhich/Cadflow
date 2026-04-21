# CADFlow — User Guide

A practical manual for drafters, architects, and students using **CADFlow** on AutoCAD 2027 for Mac with a **Loupedeck CT** or **Logitech MX Creative Console**.

If you are a developer looking to extend the plugin, jump to the [Developer Guide](developer-guide.md).

---

## 1. Before you start

You need:

- AutoCAD 2027 for Mac (installed and launched at least once so macOS learned its bundle ID).
- Logi Options+ or the Loupedeck software installed — the host service is called **Logi Plugin Service**.
- A CADFlow `.lplug4` package installed (either from a release build or your own `dotnet build` → `logiplugintool install`).
- macOS permissions granted:
  - *System Settings → Privacy & Security → Accessibility* → allow `LogiPluginService`.
  - *System Settings → Privacy & Security → Automation* → allow `LogiPluginService` to control AutoCAD 2027 and System Events.

### Smoke test

1. Launch AutoCAD 2027 and open any drawing.
2. Bring AutoCAD to the front.
3. Glance at your console — the tiles should switch to the CADFlow layout (coloured top buttons, 8 centre slots).
4. Press the top-left button. The AutoCAD drawing should undo your last action.

If step 3 doesn't happen, CADFlow is installed but auto-activation is not taking over. Either drag the CADFlow profile onto the device manually via the Logi app, or check that AutoCAD's bundle ID really is `com.autodesk.autocad2027` with:

```bash
osascript -e 'id of app "AutoCAD 2027"'
```

---

## 2. The default layout

The shipped profile (`DefaultProfile20.lp5`) binds:

```
┌─────────────┬───────────────────────────────────────────────┬─────────────┐
│  ⬅  Undo    │                                               │  Redo  ➡    │ ← top-row commands
├─────────────┴───────────────────────────────────────────────┴─────────────┤
│                                                                           │
│             8 tool-folder slots (Line, Circle, Move, Copy, …)             │ ← assignable
│                                                                           │
├──────────────┬────────────────────────────────────────────────────────────┤
│  Quick Ring  │                                                            │
│  folder ⤵   │                  roller ↔  /  scroller ↕                   │ ← pan wheels
└──────────────┴────────────────────────────────────────────────────────────┘
```

| Control | Action |
| --- | --- |
| Top-left | Undo (⌘Z) |
| Top-right | Redo (⇧⌘Z) |
| Centre buttons | Whatever tool folders you assign (defaults: Line, Circle, Move, Copy, Offset, Trim, Dimension, Array …) |
| Bottom-left | Opens the **Quick Ring** |
| Roller | Pan X (horizontal, transparent, during any command) |
| Scroller | Pan Y (vertical, transparent) |

You can rearrange any centre button in Logi Options+: drag a different `CAD Draw` / `CAD Modify` / `CAD Precision` / `CAD Advanced` folder onto the slot.

---

## 3. How a tool folder works

Tap a tool button (say **Line**). Three things happen at once:

1. The console opens the **Line** folder on the 8 centre tiles.
2. `ToolContext` marks `Line` as the active tool.
3. AutoCAD is sent the `line` command, so it is already waiting for a first point.

The 8 tiles inside any tool folder follow a fixed convention:

```
┌────────┬────────┬────────┬────────┐
│  btn0  │  btn1  │  btn2  │  btn3  │   ← tool-specific options
├────────┼────────┼────────┼────────┤
│  btn4  │  btn5  │  btn6  │  btn7  │
└────────┴────────┴────────┴────────┘
                      ▲        ▲
                   Close    Confirm (Enter)
```

- Slots **0–5** are options that make sense *inside* that command (Close, Width, Radius, Copy, Ref, Snap, …).
- Slot **6** is always a **back-arrow / close**.
- Slot **7** is always a green **confirm** tile — it sends `Enter` to AutoCAD.

Navigate back (slot 6 or the hardware home button) and CADFlow:

- Removes the tool from `ToolContext.Current`.
- Sends `Escape` to AutoCAD to cancel any pending prompt.
- Returns you to the home layout.

---

## 4. Quick Ring — parameter shortcuts

The Quick Ring is a second-level folder for small, frequent parameter hits:

| Slot | Label | What it does |
| --- | --- | --- |
| 0 | Zoom | Issues `ZOOM` with Window / Previous variants — combine with Scroller to zoom, Roller to pan. |
| 1 | LW+ | Opens `LWEIGHT` so you can pick a line weight on the fly. |
| 2 | Layer | Opens `CLAYER` to cycle active layers. |
| 3 | Angle | Starts `ROTATE` from the current selection. |
| 4 | Scale | Starts `SCALE`. |
| 5 | Hatch | Opens `HPSCALE` — adjust hatch density while a hatch is active. |
| 6 | TxtH | Opens `TEXTSIZE`. |
| 7 | Confirm | Green Enter button. |

The Quick Ring is non-modal — unlike a tool folder, it does not pre-enter any command at activation. Each tile issues its command only when you press it.

---

## 5. Roller & Scroller panning

The wheels pan the drawing **transparently**: you can spin them during any command without ending it.

- A gentle spin gives pixel-level pan (`FineScale = 0.6`).
- A fast spin crosses a large drawing quickly (`FineScale + TurboCoeff · |Δ|^1.65`, clamped at ±12 ticks per callback).
- X is Roller, Y is Scroller.
- The display does not show a numeric value — the wheels are pure nudges.

If panning feels too slow or too twitchy, open `src/Actions/NudgeEngine.cs`, tune `FineScale`, `TurboCoeff`, or `Exponent`, and rebuild. See the [Developer Guide](developer-guide.md#6-tile-rendering--surviving-bitmapbuilder).

---

## 6. Real-world workflows

### 6.1 Draw a rectilinear room outline

1. **Line** folder.
2. Click first corner in AutoCAD.
3. **btn0 Ortho** — locks to 90° without cancelling.
4. Click remaining corners.
5. **btn6 Close** — AutoCAD types `c⏎` and the last segment snaps back to the start.

No keyboard input; your drawing hand stays on the mouse.

### 6.2 Offset a run of wall-centreline segments

1. **Offset** folder.
2. **btn0 Distance** → type the distance once.
3. Pick the source line, then the side.
4. **btn6 LastVal** → reuses `@` for the next offset.
5. Repeat for each line. **Confirm** (btn7) when done.

### 6.3 Dimension chain

1. **Dimension** folder (opens `DIMLINEAR`).
2. Pick the extension points for the first dimension.
3. **btn5 Continue** → chains `DIMCONTINUE`, pick next extension point.
4. Keep picking extension points; each one extends the chain.
5. **Confirm** (btn7) to finish.

### 6.4 Array around a hub

1. **Array** folder.
2. Select objects in AutoCAD.
3. **btn1 Polar** → `ARRAYPOLAR`.
4. Pick centre point; AutoCAD prompts for count.
5. Enter the count, **Confirm**.

### 6.5 Scan a wide drawing without losing the command

1. Start any command — **Move**, for instance, mid-selection.
2. Spin Roller and Scroller to pan around the drawing and find the target.
3. Finish selecting.
4. Spin wheels again to pan to the destination.
5. Click to place. **Confirm**.

Because pan is transparent, steps 2/4 never abort the Move.

---

## 7. Tips & tricks

- **Undo never cancels a running command.** `⌘Z` is safe during a LINE — it undoes the previous segment the same way typing `U` would.
- **Slot 7 is always Enter.** If a command is asking "press Enter to finish", just smack slot 7; don't go to the keyboard.
- **Mid-command toggles work even in nested sub-commands.** Ortho/Polar/OSnap/OTrack route through a `⌘`-shortcut, which AutoCAD treats as a system-var flip, not as a command.
- **The back-arrow (slot 6) sends Escape**. If you find yourself in a weird AutoCAD prompt you don't recognise, that's usually the fastest way out.
- **Quick Ring + Tool folder don't compose.** You can only be in one folder at a time. Exit Quick Ring before entering a tool.
- **You can edit the `DefaultProfile20.lp5`** in Logi Options+ and drop new assignments in place. The file that ships is the suggested layout, not a lock-in.

---

## 8. Troubleshooting

### 8.1 Buttons don't do anything in AutoCAD

**Cause 1** — Permissions. macOS blocks AppleScript from typing into another app until you allow it.

Go to *System Settings → Privacy & Security → Accessibility*; tick `LogiPluginService`. Then *Automation* → under `LogiPluginService` → tick `System Events` and `AutoCAD 2027`.

**Cause 2** — AutoCAD isn't the frontmost app. `AcadSend` calls `tell application "AutoCAD 2027" to activate` first, but if macOS denies `activate` (Stage Manager quirk, multiple AutoCAD instances) the keystrokes go to whatever window *is* front. Click into AutoCAD's viewport, then try again.

### 8.2 The profile doesn't auto-switch

Open a Terminal and run:

```bash
osascript -e 'id of app "AutoCAD 2027"'
```

If the result is **not** `com.autodesk.autocad2027`, Autodesk has shipped a differently-IDed build. Work around by either:

- Manually dragging the CADFlow profile onto the device in Logi Options+, or
- Editing `src/CadFlowApplication.cs` to return the bundle you actually got, and rebuilding.

### 8.3 Pan is too jittery / too slow

Tune `NudgeEngine.cs`:

| Feel | Change |
| --- | --- |
| Too slow for fine detail | Raise `FineScale` (e.g. `0.9`). |
| Jittery — single clicks over-pan | Lower `FineScale` (e.g. `0.4`). |
| Turbo doesn't kick in | Raise `TurboCoeff` (e.g. `2.2`) or `Exponent` (e.g. `1.85`). |
| Pan runs away on a hard spin | Lower `MaxDelta` (e.g. `8`). |

Rebuild (`dotnet build -c Release`) — the `.link` file auto-hot-reloads the plugin.

### 8.4 I get weird characters typed into the drawing

Most likely: the AppleScript ran but AutoCAD wasn't focused. Make sure no system dialog (Finder, Notification Center, a Save sheet) grabbed focus before the press. Close any AutoCAD modal first.

### 8.5 The tile images are blurry or distorted

CADFlow draws tiles procedurally against a 45-pixel virtual canvas. On very small screens (e.g. dial adjustment tiles) aliasing is expected. Any label not known to the base class's `DrawIcon` falls back to a diamond glyph — if you see a diamond, the tool has a custom label that the renderer hasn't been taught yet (harmless; see the [Developer Guide](developer-guide.md) to add a case).

### 8.6 Nothing activates after a clean install

Open a shell and run:

```bash
ls "$HOME/Library/Application Support/Logi/LogiPluginService/Plugins/"
```

If no `CadFlow.link` is present, the post-build step failed. Rebuild with `dotnet build -c Debug src/CadFlow.csproj` and confirm the build output doesn't show MSBuild errors on the `PostBuild` target.

### 8.7 Where do I see log output?

```bash
log stream --predicate 'process == "LogiPluginService"' --info
```

Every folder activation logs `[ToolContext] Activated: <name>`. If you don't see those, the button press never reached the plugin — investigate the device mapping in Logi Options+.

---

## 9. FAQ

**Q: Does CADFlow work on AutoCAD for Windows?**
A: Not out of the box. Everything is routed through `osascript`, which is macOS-only. See the [Roadmap](../README.md#roadmap--future-improvements).

**Q: Does CADFlow work on AutoCAD LT?**
A: If the macOS build's bundle ID is recognised and it supports the subset of commands CADFlow sends, most tools should work. Advanced (Array, Dimension chains) may not.

**Q: Can I customise the 8 buttons in a tool folder?**
A: Yes, but only at build time today — each tool's `ContextBtns` array is a static property. A configuration file at runtime would be a welcome contribution.

**Q: Does CADFlow store any data?**
A: No. It keeps a single in-memory `ToolContext.Current` reference. It writes no settings files. All state lives in the Logi profile (`DefaultProfile20.lp5`).

**Q: Is CADFlow safe for production drawings?**
A: CADFlow only issues the same commands you'd issue at the AutoCAD command line. Undo and Redo work normally. That said, because it automates keystrokes, always keep an eye on what's being typed if an unexpected dialog or modal steals focus.

**Q: How do I uninstall?**
A: In VSCode, run the `Uninstall Plugin` task — it invokes `logiplugintool uninstall My`. Or remove `~/Library/Application Support/Logi/LogiPluginService/Plugins/CadFlow.link` and restart the Logi Plugin Service.
