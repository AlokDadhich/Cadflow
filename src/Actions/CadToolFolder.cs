namespace Loupedeck.CadFlow
{
    using System.Collections.Generic;

    public struct CtxBtn
    {
        public readonly string Label;
        public readonly string Cmd;
        public readonly bool   IsToggle;

        public CtxBtn(string label, string cmd, bool isToggle = false)
        {
            Label    = label;
            Cmd      = cmd;
            IsToggle = isToggle;
        }
    }

    public abstract class CadToolFolder : PluginDynamicFolder
    {
        protected abstract string   ToolName    { get; }
        protected abstract string   ToolCmd     { get; }
        protected abstract string   FolderGroup { get; }
        protected abstract CtxBtn[] ContextBtns { get; }

        private static readonly string[] CtxParams =
            { "btn0","btn1","btn2","btn3","btn4","btn5","btn6","btn7" };

        protected void Init()
        {
            this.DisplayName = ToolName;
            this.GroupName   = FolderGroup;
        }

        public override bool Load()   => true;
        public override bool Unload() => true;

        public override bool Activate()
        {
            ToolContext.Activate(this);
            AcadSend.Send(ToolCmd);
            this.ButtonActionNamesChanged();
            RefreshAll();
            return true;
        }

        public override bool Deactivate()
        {
            ToolContext.Deactivate(this);
            AcadSend.SendEscape();
            this.Close();
            return true;
        }

        public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
        {
            return new[]
            {
                this.CreateCommandName(CtxParams[0]),
                this.CreateCommandName(CtxParams[1]),
                this.CreateCommandName(CtxParams[2]),
                this.CreateCommandName(CtxParams[3]),
                this.CreateCommandName(CtxParams[4]),
                this.CreateCommandName(CtxParams[5]),
                this.CreateCommandName(CtxParams[6]),
                this.CreateCommandName(CtxParams[7]),
            };
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType deviceType)
            => PluginDynamicFolderNavigation.None;

        public override void RunCommand(string param)
        {
            if (param == null) return;
            int idx = SlotIndex(param);
            if (idx < 0) return;

            if (idx == 7)
            {
                AcadSend.SendEnter();
            }
            else if (ContextBtns[idx].IsToggle)
            {
                AcadSend.SendToggle(ContextBtns[idx].Cmd);
                RefreshAll();
            }
            else if (ContextBtns[idx].Cmd == "TAB")
            {
                AcadSend.SendTab();
            }
            else if (!string.IsNullOrEmpty(ContextBtns[idx].Cmd))
            {
                AcadSend.Send(ContextBtns[idx].Cmd);
            }
        }

        public override BitmapImage GetCommandImage(string param, PluginImageSize size)
        {
            if (param == null) return null;
            int idx = SlotIndex(param);
            if (idx < 0) return Blank(size);
            return DrawTile(size, idx, ContextBtns[idx]);
        }

        public override string GetCommandDisplayName(string param, PluginImageSize size)
        {
            if (param == null) return "";
            int idx = SlotIndex(param);
            return idx >= 0 ? ContextBtns[idx].Label : "";
        }

        private void RefreshAll()
        {
            foreach (var p in CtxParams)
                this.CommandImageChanged(this.CreateCommandName(p));
        }

        private BitmapImage DrawTile(PluginImageSize size, int idx, CtxBtn btn)
        {
            BitmapColor fg, accent, dimFg;

            if (idx == 7)
            {
                fg     = new BitmapColor(70, 210, 70);
                accent = new BitmapColor(40, 160, 40);
                dimFg  = new BitmapColor(40, 120, 40);
            }
            else if (idx == 6)
            {
                fg     = new BitmapColor(80, 140, 255);
                accent = new BitmapColor(50, 90, 200);
                dimFg  = new BitmapColor(40, 70, 140);
            }
            else if (btn.Cmd == "TAB")
            {
                fg     = new BitmapColor(255, 155, 50);
                accent = new BitmapColor(180, 85, 15);
                dimFg  = new BitmapColor(120, 55, 10);
            }
            else if (btn.IsToggle)
            {
                fg     = new BitmapColor(110, 150, 255);
                accent = new BitmapColor(55, 75, 180);
                dimFg  = new BitmapColor(45, 55, 110);
            }
            else
            {
                fg     = new BitmapColor(200, 200, 210);
                accent = new BitmapColor(55, 55, 70);
                dimFg  = new BitmapColor(70, 70, 80);
            }

            var black = new BitmapColor(0, 0, 0);
            var b = new BitmapBuilder(size);
            int W = b.Width;
            int H = b.Height;

            b.FillRectangle(0, 0, W, H, black);
            DrawIcon(b, btn.Label, idx, W, H, fg, accent, dimFg, black);
            return b.ToImage();
        }

        // Virtual canvas: 60 units — icons centred around cy, min 2px strokes
        private static void DrawIcon(BitmapBuilder b, string label, int idx, int W, int H,
                                     BitmapColor fg, BitmapColor accent, BitmapColor dim,
                                     BitmapColor bg)
        {
            int cx = W / 2;
            int cy = H / 2;
            double sy = H / 60.0;
            double sx = W / 60.0;
            int SY(int h) => Math.Max(2, (int)(h * sy));
            int SX(int w) => Math.Max(2, (int)(w * sx));

            // ── Special slots ────────────────────────────────────────────────

            if (idx == 7) // Confirm — bold ✓
            {
                b.FillRectangle(cx - 12, cy + SY(2),  SX(5), SY(5), fg);
                b.FillRectangle(cx -  7, cy + SY(6),  SX(5), SY(5), fg);
                b.FillRectangle(cx -  2, cy + SY(2),  SX(5), SY(5), fg);
                b.FillRectangle(cx +  3, cy - SY(3),  SX(5), SY(5), fg);
                b.FillRectangle(cx +  8, cy - SY(8),  SX(5), SY(5), fg);
                return;
            }

            if (idx == 6) // Back arrow ←
            {
                b.FillRectangle(cx - 10, cy - SY(3), SX(18), SY(6), fg);
                b.FillRectangle(cx - 16, cy - SY(3), SX(6),  SY(6), fg);
                b.FillRectangle(cx - 12, cy - SY(9), SX(6),  SY(6), fg);
                b.FillRectangle(cx - 12, cy + SY(3), SX(6),  SY(6), fg);
                return;
            }

            // ── Label-based icons ─────────────────────────────────────────────
            switch (label)
            {
                case "Ortho":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 14,    SX(6),  SY(28), fg);
                    b.FillRectangle(cx -  3, cy - SY(3), SX(6),  SY(6), accent);
                    break;

                case "Polar":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 14,    SX(6),  SY(28), fg);
                    b.FillRectangle(cx +  5, cy - 14,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx +  9, cy - 18,    SX(5),  SY(5), accent);
                    break;

                case "OSnap":
                    b.FillRectangle(cx -  9, cy - 12,    SX(18), SY(5), fg);
                    b.FillRectangle(cx -  9, cy +  7,    SX(18), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  8, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx - 18, cy - SY(2), SX(5),  SY(4), dim);
                    b.FillRectangle(cx + 13, cy - SY(2), SX(5),  SY(4), dim);
                    b.FillRectangle(cx -  2, cy - 18,    SX(4),  SY(5), dim);
                    b.FillRectangle(cx -  2, cy + 13,    SX(4),  SY(5), dim);
                    break;

                case "OTrack":
                    b.FillRectangle(cx - 14, cy - SY(2), SX(6),  SY(4), fg);
                    b.FillRectangle(cx -  4, cy - SY(2), SX(6),  SY(4), fg);
                    b.FillRectangle(cx +  6, cy - SY(2), SX(6),  SY(4), fg);
                    b.FillRectangle(cx - 16, cy -  8,    SX(4),  SY(16), accent);
                    break;

                case "Snap":
                    for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        b.FillRectangle(cx - 10 + col * 10, cy - 10 + row * 10, SX(5), SY(5), fg);
                    break;

                case "UndoSeg":
                    b.FillRectangle(cx - 12, cy - SY(3), SX(20), SY(6), fg);
                    b.FillRectangle(cx - 12, cy - SY(9), SX(6),  SY(6), fg);
                    b.FillRectangle(cx - 12, cy + SY(3), SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  6, cy - 10,    SX(5),  SY(20), dim);
                    break;

                case "Tab":
                    b.FillRectangle(cx - 13, cy - SY(3), SX(18), SY(6), fg);
                    b.FillRectangle(cx - 13, cy - 10,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx - 13, cy + SY(3), SX(5),  SY(7), fg);
                    b.FillRectangle(cx +  5, cy - 14,    SX(5),  SY(28), fg);
                    break;

                case "Close":
                    b.FillRectangle(cx - 12, cy + SY(2), SX(18), SY(6), fg);
                    b.FillRectangle(cx - 12, cy - 14,    SX(5),  SY(16), fg);
                    b.FillRectangle(cx +  3, cy - 14,    SX(14), SY(5), fg);
                    b.FillRectangle(cx - 15, cy + SY(2), SX(5),  SY(6), accent);
                    b.FillRectangle(cx - 17, cy + SY(5), SX(7),  SY(5), accent);
                    break;

                case "ArcMode":
                    b.FillRectangle(cx - 10, cy - 12,    SX(20), SY(5), fg);
                    b.FillRectangle(cx - 14, cy -  7,    SX(5),  SY(10), fg);
                    b.FillRectangle(cx +  9, cy -  7,    SX(5),  SY(10), fg);
                    b.FillRectangle(cx -  8, cy +  3,    SX(5),  SY(6), fg);
                    b.FillRectangle(cx +  3, cy +  3,    SX(5),  SY(6), fg);
                    break;

                case "Width":
                    b.FillRectangle(cx - 13, cy - 12,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy +  8,    SX(26), SY(4), fg);
                    b.FillRectangle(cx -  3, cy -  8,    SX(6),  SY(16), accent);
                    b.FillRectangle(cx -  5, cy -  8,    SX(10), SY(4), accent);
                    b.FillRectangle(cx -  5, cy +  4,    SX(10), SY(4), accent);
                    break;

                case "Join":
                    b.FillRectangle(cx - 13, cy - 12,    SX(5),  SY(18), fg);
                    b.FillRectangle(cx +  8, cy - 12,    SX(5),  SY(18), fg);
                    b.FillRectangle(cx -  8, cy - SY(3), SX(8),  SY(6), fg);
                    b.FillRectangle(cx,      cy - SY(3), SX(8),  SY(6), fg);
                    b.FillRectangle(cx -  2, cy + SY(3), SX(5),  SY(10), fg);
                    break;

                case "Center":
                    b.FillRectangle(cx - 10, cy - 12,    SX(20), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  8, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx - 10, cy +  7,    SX(20), SY(5), fg);
                    b.FillRectangle(cx -  3, cy -  4,    SX(6),  SY(8), accent);
                    break;

                case "2Point":
                    b.FillRectangle(cx - 15, cy -  4,    SX(6),  SY(8), fg);
                    b.FillRectangle(cx +  9, cy -  4,    SX(6),  SY(8), fg);
                    b.FillRectangle(cx - 10, cy - SY(2), SX(20), SY(4), dim);
                    break;

                case "3Point":
                    b.FillRectangle(cx -  3, cy - 15,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx - 15, cy +  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  9, cy +  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx - 12, cy -  9,    SX(12), SY(3), dim);
                    b.FillRectangle(cx,      cy -  9,    SX(12), SY(3), dim);
                    b.FillRectangle(cx - 12, cy + 12,    SX(24), SY(3), dim);
                    break;

                case "Radius":
                    b.FillRectangle(cx - 10, cy - 12,    SX(20), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  8, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx - 10, cy +  7,    SX(20), SY(5), fg);
                    b.FillRectangle(cx -  2, cy -  3,    SX(4),  SY(6), accent);
                    b.FillRectangle(cx -  2, cy -  3,    SX(14), SY(4), accent);
                    break;

                case "Diameter":
                    b.FillRectangle(cx - 10, cy - 12,    SX(20), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  8, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx - 10, cy +  7,    SX(20), SY(5), fg);
                    b.FillRectangle(cx - 13, cy - SY(2), SX(26), SY(4), accent);
                    break;

                case "Sides":
                case "Inscribed":
                case "Circum":
                case "Edge":
                    b.FillRectangle(cx -  6, cy - 14,    SX(12), SY(5), fg);
                    b.FillRectangle(cx - 11, cy -  9,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx +  6, cy -  9,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx - 13, cy -  2,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx +  8, cy -  2,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx - 11, cy +  5,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx +  6, cy +  5,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx -  6, cy + 10,    SX(12), SY(5), fg);
                    break;

                case "Chamfer":
                case "Fillet":
                case "Rotate":
                    b.FillRectangle(cx - 13, cy - 14,    SX(5),  SY(22), fg);
                    b.FillRectangle(cx - 13, cy +  8,    SX(26), SY(5), fg);
                    b.FillRectangle(cx -  8, cy +  3,    SX(6),  SY(5), accent);
                    b.FillRectangle(cx -  2, cy -  2,    SX(6),  SY(5), accent);
                    break;

                case "Rotation":
                case "AngleLk":
                    b.FillRectangle(cx - 13, cy +  8,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy - 10,    SX(4),  SY(18), fg);
                    b.FillRectangle(cx -  9, cy - 12,    SX(8),  SY(4), accent);
                    b.FillRectangle(cx -  1, cy - 10,    SX(5),  SY(4), accent);
                    b.FillRectangle(cx +  4, cy -  6,    SX(5),  SY(4), accent);
                    b.FillRectangle(cx +  6, cy -  2,    SX(5),  SY(4), accent);
                    break;

                case "Area":
                case "Uniform":
                    b.FillRectangle(cx - 13, cy - 14,    SX(8),  SY(4), fg);
                    b.FillRectangle(cx +  5, cy - 14,    SX(8),  SY(4), fg);
                    b.FillRectangle(cx - 13, cy + 10,    SX(8),  SY(4), fg);
                    b.FillRectangle(cx +  5, cy + 10,    SX(8),  SY(4), fg);
                    b.FillRectangle(cx - 14, cy -  9,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx - 14, cy +  2,    SX(4),  SY(7), fg);
                    b.FillRectangle(cx + 10, cy -  9,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx + 10, cy +  2,    SX(4),  SY(7), fg);
                    break;

                case "LastVal":
                    b.FillRectangle(cx -  6, cy - 10,    SX(12), SY(4), fg);
                    b.FillRectangle(cx -  9, cy -  6,    SX(5),  SY(12), fg);
                    b.FillRectangle(cx +  4, cy -  6,    SX(5),  SY(8), fg);
                    b.FillRectangle(cx -  6, cy +  6,    SX(12), SY(4), fg);
                    b.FillRectangle(cx -  2, cy -  2,    SX(5),  SY(8), dim);
                    b.FillRectangle(cx +  4, cy +  2,    SX(6),  SY(5), fg);
                    break;

                case "BasePt":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 16,    SX(6),  SY(28), fg);
                    b.FillRectangle(cx -  5, cy -  5,    SX(10), SY(10), accent);
                    break;

                case "Displace":
                    b.FillRectangle(cx - 13, cy - SY(3), SX(22), SY(6), fg);
                    b.FillRectangle(cx +  6, cy -  8,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  6, cy +  3,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 15, cy - 10,    SX(5),  SY(20), dim);
                    break;

                case "CopyMode":
                case "Multiple":
                    b.FillRectangle(cx - 13, cy - 12,    SX(14), SY(4), fg);
                    b.FillRectangle(cx - 13, cy + SY(3), SX(14), SY(4), fg);
                    b.FillRectangle(cx - 15, cy -  8,    SX(4),  SY(11), fg);
                    b.FillRectangle(cx -  1, cy -  8,    SX(4),  SY(11), fg);
                    b.FillRectangle(cx -  7, cy -  8,    SX(16), SY(4), accent);
                    b.FillRectangle(cx -  7, cy + SY(7), SX(16), SY(4), accent);
                    b.FillRectangle(cx +  7, cy -  4,    SX(4),  SY(14), accent);
                    break;

                case "AxisLock":
                case "Axis":
                    b.FillRectangle(cx -  8, cy - 12,    SX(16), SY(4), fg);
                    b.FillRectangle(cx - 10, cy -  8,    SX(4),  SY(10), fg);
                    b.FillRectangle(cx +  6, cy -  8,    SX(4),  SY(10), fg);
                    b.FillRectangle(cx - 12, cy +  2,    SX(24), SY(12), fg);
                    b.FillRectangle(cx -  3, cy +  4,    SX(6),  SY(8), bg);
                    break;

                case "RotateCp":
                    b.FillRectangle(cx - 10, cy - 12,    SX(20), SY(5), fg);
                    b.FillRectangle(cx +  6, cy -  7,    SX(5),  SY(12), fg);
                    b.FillRectangle(cx - 10, cy +  5,    SX(14), SY(5), fg);
                    b.FillRectangle(cx - 14, cy -  1,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx - 14, cy +  4,    SX(5),  SY(5), accent);
                    break;

                case "MirrorCp":
                    b.FillRectangle(cx - 13, cy - 13,    SX(11), SY(18), fg);
                    b.FillRectangle(cx +  2, cy - 13,    SX(11), SY(18), accent);
                    b.FillRectangle(cx -  2, cy - 16,    SX(4),  SY(24), dim);
                    break;

                case "ArrayCp":
                case "Rect":
                    b.FillRectangle(cx - 14, cy - 14,    SX(11), SY(11), fg);
                    b.FillRectangle(cx +  3, cy - 14,    SX(11), SY(11), fg);
                    b.FillRectangle(cx - 14, cy +  3,    SX(11), SY(11), fg);
                    b.FillRectangle(cx +  3, cy +  3,    SX(11), SY(11), accent);
                    break;

                case "Distance":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx - 14, cy -  9,    SX(4),  SY(12), fg);
                    b.FillRectangle(cx -  7, cy -  7,    SX(4),  SY(8), dim);
                    b.FillRectangle(cx -  1, cy -  9,    SX(4),  SY(12), dim);
                    b.FillRectangle(cx +  5, cy -  7,    SX(4),  SY(8), dim);
                    b.FillRectangle(cx + 10, cy -  9,    SX(4),  SY(12), fg);
                    break;

                case "Through":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 10,    SX(6),  SY(20), accent);
                    b.FillRectangle(cx -  5, cy -  7,    SX(10), SY(14), bg);
                    b.FillRectangle(cx -  3, cy -  5,    SX(6),  SY(10), accent);
                    break;

                case "Both":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx - 14, cy -  9,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 14, cy + SY(3), SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  9, cy -  9,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  9, cy + SY(3), SX(5),  SY(5), fg);
                    break;

                case "EraseSrc":
                    b.FillRectangle(cx - 12, cy - 12,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  6, cy -  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx,      cy,         SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  6, cy +  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx - 12, cy +  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  6, cy,         SX(6),  SY(6), fg);
                    b.FillRectangle(cx,      cy -  6,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  6, cy - 12,    SX(6),  SY(6), fg);
                    break;

                case "Layer":
                    b.FillRectangle(cx - 13, cy - 12,    SX(26), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  2,    SX(26), SY(5), fg);
                    b.FillRectangle(cx - 13, cy +  8,    SX(26), SY(5), dim);
                    break;

                case "Fence":
                    b.FillRectangle(cx - 14, cy +  8,    SX(7),  SY(5), fg);
                    b.FillRectangle(cx -  9, cy +  2,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  4, cy -  4,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  1, cy - 10,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  5, cy -  4,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  9, cy +  2,    SX(6),  SY(6), fg);
                    break;

                case "Crossing":
                    b.FillRectangle(cx - 13, cy - 13,    SX(7),  SY(4), fg);
                    b.FillRectangle(cx +  6, cy - 13,    SX(7),  SY(4), fg);
                    b.FillRectangle(cx - 13, cy +  9,    SX(7),  SY(4), fg);
                    b.FillRectangle(cx +  6, cy +  9,    SX(7),  SY(4), fg);
                    b.FillRectangle(cx - 15, cy -  9,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx - 15, cy +  0,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx + 11, cy -  9,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx + 11, cy +  0,    SX(4),  SY(9), fg);
                    b.FillRectangle(cx -  6, cy -  6,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx +  1, cy +  1,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx +  1, cy -  6,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx -  6, cy +  1,    SX(5),  SY(5), accent);
                    break;

                case "UndoTrim":
                case "UndoExt":
                    b.FillRectangle(cx - 10, cy -  3,    SX(18), SY(6), fg);
                    b.FillRectangle(cx - 10, cy -  9,    SX(5),  SY(6), fg);
                    b.FillRectangle(cx - 10, cy + SY(3), SX(5),  SY(6), fg);
                    b.FillRectangle(cx +  5, cy - 12,    SX(5),  SY(24), dim);
                    b.FillRectangle(cx +  5, cy - 14,    SX(9),  SY(4), dim);
                    break;

                case "Extend":
                    b.FillRectangle(cx +  8, cy - 14,    SX(5),  SY(24), fg);
                    b.FillRectangle(cx - 13, cy - SY(3), SX(22), SY(6), fg);
                    b.FillRectangle(cx +  2, cy - 10,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  2, cy +  5,    SX(5),  SY(5), fg);
                    break;

                case "Trim":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx +  4, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 14,    SX(6),  SY(22), accent);
                    break;

                case "All":
                    b.FillRectangle(cx - 13, cy - 14,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy + 12,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 15, cy - 10,    SX(4),  SY(22), fg);
                    b.FillRectangle(cx + 11, cy - 10,    SX(4),  SY(22), fg);
                    b.FillRectangle(cx -  8, cy +  0,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  2, cy +  4,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  3, cy -  2,    SX(5),  SY(8), fg);
                    break;

                case "LastSel":
                    b.FillRectangle(cx -  9, cy - 12,    SX(18), SY(5), fg);
                    b.FillRectangle(cx - 11, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  6, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx -  9, cy +  7,    SX(18), SY(5), fg);
                    b.FillRectangle(cx -  2, cy -  7,    SX(5),  SY(12), accent);
                    b.FillRectangle(cx -  2, cy -  1,    SX(8),  SY(5), accent);
                    break;

                case "Ref":
                    b.FillRectangle(cx - 13, cy + 10,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy - 14,    SX(4),  SY(24), fg);
                    b.FillRectangle(cx -  7, cy +  3,    SX(6),  SY(4), accent);
                    b.FillRectangle(cx -  1, cy -  3,    SX(6),  SY(4), accent);
                    b.FillRectangle(cx +  5, cy -  9,    SX(6),  SY(4), accent);
                    break;

                case "Copy":
                    b.FillRectangle(cx - 11, cy - 14,    SX(14), SY(20), fg);
                    b.FillRectangle(cx -  5, cy -  8,    SX(14), SY(14), accent);
                    break;

                case "Xonly":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx +  8, cy -  9,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  8, cy + SY(3), SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 14, cy - 16,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx -  9, cy - 11,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx -  4, cy -  6,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx -  4, cy - 16,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx - 14, cy -  6,    SX(5),  SY(5), accent);
                    break;

                case "Yonly":
                    b.FillRectangle(cx -  2, cy - SY(3), SX(4),  SY(28), fg);
                    b.FillRectangle(cx -  7, cy - 10,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  3, cy - 10,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  7, cy - 16,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx + 11, cy - 11,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx + 10, cy -  6,    SX(4),  SY(7), accent);
                    b.FillRectangle(cx +  7, cy -  6,    SX(5),  SY(5), accent);
                    b.FillRectangle(cx + 13, cy - 11,    SX(5),  SY(5), accent);
                    break;

                case "AxisPick":
                    b.FillRectangle(cx -  2, cy - 16,    SX(4),  SY(6), fg);
                    b.FillRectangle(cx -  2, cy -  8,    SX(4),  SY(6), fg);
                    b.FillRectangle(cx -  2, cy,         SX(4),  SY(6), fg);
                    b.FillRectangle(cx -  4, cy - 20,    SX(8),  SY(4), accent);
                    b.FillRectangle(cx -  7, cy - 22,    SX(14), SY(4), accent);
                    break;

                case "Horiz":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx - 14, cy -  9,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 14, cy + SY(3), SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  9, cy -  9,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  9, cy + SY(3), SX(5),  SY(5), fg);
                    b.FillRectangle(cx -  2, cy - 16,    SX(4),  SY(26), dim);
                    break;

                case "Vert":
                    b.FillRectangle(cx -  2, cy - 16,    SX(4),  SY(32), fg);
                    b.FillRectangle(cx -  6, cy - 16,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  1, cy - 16,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx -  6, cy + 11,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx +  1, cy + 11,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 13, cy - SY(2), SX(26), SY(4), dim);
                    break;

                case "Rot180":
                    b.FillRectangle(cx - 11, cy - 12,    SX(22), SY(5), fg);
                    b.FillRectangle(cx - 14, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx +  9, cy -  7,    SX(5),  SY(14), fg);
                    b.FillRectangle(cx - 11, cy +  7,    SX(22), SY(5), fg);
                    b.FillRectangle(cx -  3, cy -  4,    SX(6),  SY(8), accent);
                    break;

                case "Polyline":
                    b.FillRectangle(cx - 14, cy +  6,    SX(8),  SY(5), fg);
                    b.FillRectangle(cx -  8, cy -  1,    SX(5),  SY(7), fg);
                    b.FillRectangle(cx -  8, cy -  1,    SX(12), SY(5), fg);
                    b.FillRectangle(cx +  1, cy - 14,    SX(5),  SY(20), fg);
                    b.FillRectangle(cx +  1, cy +  6,    SX(12), SY(5), fg);
                    b.FillRectangle(cx + 10, cy - 16,    SX(5),  SY(22), fg);
                    break;

                case "Chain":
                    b.FillRectangle(cx - 14, cy -  4,    SX(11), SY(10), fg);
                    b.FillRectangle(cx - 12, cy -  2,    SX(6),  SY(6), bg);
                    b.FillRectangle(cx -  2, cy -  4,    SX(4),  SY(10), fg);
                    b.FillRectangle(cx +  1, cy -  4,    SX(11), SY(10), fg);
                    b.FillRectangle(cx +  3, cy -  2,    SX(6),  SY(6), bg);
                    break;

                case "PolarArr":
                    b.FillRectangle(cx -  3, cy - 16,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  9, cy -  3,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  3, cy +  9,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx - 15, cy -  3,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  3, cy -  3,    SX(6),  SY(6), accent);
                    break;

                case "Path":
                    b.FillRectangle(cx - 14, cy + 10,    SX(8),  SY(5), fg);
                    b.FillRectangle(cx -  8, cy +  4,    SX(5),  SY(6), fg);
                    b.FillRectangle(cx -  5, cy -  2,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx,      cy -  6,    SX(7),  SY(5), fg);
                    b.FillRectangle(cx +  5, cy - 12,    SX(8),  SY(5), fg);
                    b.FillRectangle(cx + 11, cy - 14,    SX(5),  SY(10), fg);
                    break;

                case "Rows":
                    b.FillRectangle(cx - 13, cy - 12,    SX(26), SY(5), fg);
                    b.FillRectangle(cx - 13, cy -  2,    SX(26), SY(5), fg);
                    b.FillRectangle(cx - 13, cy +  8,    SX(26), SY(5), dim);
                    break;

                case "Cols":
                    b.FillRectangle(cx - 13, cy - 12,    SX(5),  SY(24), fg);
                    b.FillRectangle(cx -  2, cy - 12,    SX(5),  SY(24), fg);
                    b.FillRectangle(cx +  9, cy - 12,    SX(5),  SY(24), dim);
                    break;

                case "Space":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(28), SY(6), fg);
                    b.FillRectangle(cx - 14, cy - 14,    SX(4),  SY(18), fg);
                    b.FillRectangle(cx + 10, cy - 14,    SX(4),  SY(18), fg);
                    b.FillRectangle(cx -  4, cy - 10,    SX(8),  SY(4), accent);
                    b.FillRectangle(cx -  4, cy +  6,    SX(8),  SY(4), accent);
                    break;

                case "Linear":
                    b.FillRectangle(cx - 14, cy + SY(2), SX(28), SY(5), fg);
                    b.FillRectangle(cx - 14, cy -  8,    SX(4),  SY(10), fg);
                    b.FillRectangle(cx + 10, cy -  8,    SX(4),  SY(10), fg);
                    b.FillRectangle(cx -  4, cy -  6,    SX(8),  SY(4), accent);
                    break;

                case "Aligned":
                    b.FillRectangle(cx - 14, cy + 10,    SX(5),  SY(5), fg);
                    b.FillRectangle(cx - 10, cy +  4,    SX(5),  SY(6), fg);
                    b.FillRectangle(cx -  6, cy -  2,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx -  1, cy -  8,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  4, cy - 12,    SX(5),  SY(6), fg);
                    b.FillRectangle(cx +  8, cy - 12,    SX(5),  SY(24), fg);
                    break;

                case "Continue":
                    b.FillRectangle(cx - 14, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx +  4, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx - 14, cy - 10,    SX(4),  SY(14), fg);
                    b.FillRectangle(cx -  2, cy - 10,    SX(4),  SY(14), fg);
                    b.FillRectangle(cx,      cy - 10,    SX(4),  SY(14), fg);
                    b.FillRectangle(cx + 12, cy - 10,    SX(4),  SY(14), fg);
                    break;

                case "Scale":
                    b.FillRectangle(cx -  3, cy - SY(3), SX(6),  SY(6), accent);
                    b.FillRectangle(cx - 14, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx +  4, cy - SY(3), SX(10), SY(6), fg);
                    b.FillRectangle(cx -  3, cy - 14,    SX(6),  SY(10), fg);
                    b.FillRectangle(cx -  3, cy +  4,    SX(6),  SY(10), fg);
                    break;

                case "Flip":
                    b.FillRectangle(cx - 14, cy +  4,    SX(13), SY(5), fg);
                    b.FillRectangle(cx - 11, cy -  2,    SX(10), SY(6), fg);
                    b.FillRectangle(cx -  7, cy -  8,    SX(6),  SY(6), fg);
                    b.FillRectangle(cx +  1, cy -  2,    SX(10), SY(6), accent);
                    b.FillRectangle(cx +  1, cy +  4,    SX(13), SY(5), accent);
                    b.FillRectangle(cx +  1, cy -  8,    SX(6),  SY(6), accent);
                    b.FillRectangle(cx -  2, cy - 12,    SX(4),  SY(26), dim);
                    break;

                case "Single":
                    b.FillRectangle(cx -  4, cy - 14,    SX(4),  SY(24), fg);
                    b.FillRectangle(cx +  0, cy - 14,    SX(4),  SY(24), fg);
                    b.FillRectangle(cx -  2, cy -  2,    SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 13, cy + 10,    SX(26), SY(4), dim);
                    break;

                case "Multi":
                    b.FillRectangle(cx - 13, cy - 14,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy -  6,    SX(20), SY(4), fg);
                    b.FillRectangle(cx - 13, cy +  2,    SX(22), SY(4), fg);
                    b.FillRectangle(cx - 13, cy + 10,    SX(16), SY(4), dim);
                    break;

                case "Height":
                    b.FillRectangle(cx -  3, cy - 14,    SX(6),  SY(28), fg);
                    b.FillRectangle(cx -  6, cy - 14,    SX(12), SY(5), fg);
                    b.FillRectangle(cx -  6, cy + 10,    SX(12), SY(5), fg);
                    b.FillRectangle(cx - 10, cy -  2,    SX(9),  SY(5), dim);
                    break;

                case "Style":
                    b.FillRectangle(cx -  8, cy - 12,    SX(16), SY(5), fg);
                    b.FillRectangle(cx - 10, cy -  7,    SX(5),  SY(8), fg);
                    b.FillRectangle(cx -  8, cy +  0,    SX(16), SY(5), fg);
                    b.FillRectangle(cx +  5, cy +  4,    SX(5),  SY(8), fg);
                    b.FillRectangle(cx -  8, cy + 11,    SX(16), SY(5), fg);
                    break;

                case "Align":
                    b.FillRectangle(cx - 13, cy - 10,    SX(26), SY(4), fg);
                    b.FillRectangle(cx - 13, cy -  2,    SX(18), SY(4), fg);
                    b.FillRectangle(cx - 13, cy +  6,    SX(22), SY(4), fg);
                    b.FillRectangle(cx - 16, cy - 12,    SX(4),  SY(20), accent);
                    break;

                default:
                    // Generic diamond fallback — bold
                    b.FillRectangle(cx -  2, cy - 14,    SX(4),  SY(5), fg);
                    b.FillRectangle(cx -  7, cy -  9,    SX(14), SY(5), fg);
                    b.FillRectangle(cx - 10, cy -  4,    SX(20), SY(5), fg);
                    b.FillRectangle(cx -  7, cy +  1,    SX(14), SY(5), fg);
                    b.FillRectangle(cx -  2, cy +  6,    SX(4),  SY(5), fg);
                    break;
            }
        }

        private static BitmapImage Blank(PluginImageSize size)
        {
            var b = new BitmapBuilder(size);
            b.FillRectangle(0, 0, b.Width, b.Height, new BitmapColor(0, 0, 0));
            return b.ToImage();
        }

        private static int SlotIndex(string param)
        {
            if (param != null && param.StartsWith("btn") && param.Length == 4 && char.IsDigit(param[3]))
            {
                int i = param[3] - '0';
                if (i >= 0 && i <= 7) return i;
            }
            return -1;
        }
    }

    public abstract class DrawToolFolder      : CadToolFolder { protected override string FolderGroup => "CAD Draw"; }
    public abstract class ModifyToolFolder    : CadToolFolder { protected override string FolderGroup => "CAD Modify"; }
    public abstract class PrecisionToolFolder : CadToolFolder { protected override string FolderGroup => "CAD Precision"; }
    public abstract class AdvancedToolFolder  : CadToolFolder { protected override string FolderGroup => "CAD Advanced"; }
}