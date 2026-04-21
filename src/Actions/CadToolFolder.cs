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

            // Transparent/black background — no FillRectangle for bg
            var black = new BitmapColor(0, 0, 0);
            var b = new BitmapBuilder(size);
            int W = b.Width;
            int H = b.Height;

            b.FillRectangle(0, 0, W, H, black);

            DrawIcon(b, btn.Label, idx, W, H, fg, accent, dimFg, black);

            return b.ToImage();
        }

        // Icons scaled to ~45px virtual canvas for larger rendering
        private static void DrawIcon(BitmapBuilder b, string label, int idx, int W, int H,
                                     BitmapColor fg, BitmapColor accent, BitmapColor dim,
                                     BitmapColor bg)
        {
            int cx = W / 2;
            int cy = H / 2;
            // Increased scale: virtual canvas 45px instead of 60px → ~33% larger icons
            double sy = H / 45.0;
            double sx = W / 45.0;
            int Y(int y) => (int)(y * sy);
            int SY(int h) => Math.Max(1, (int)(h * sy));
            int SX(int w) => Math.Max(1, (int)(w * sx));

            // ── Special slots ───────────────────────────────────────────────
            if (idx == 7) // Confirm — checkmark
            {
                b.FillRectangle(cx - 10, Y(28), SX(4), SY(8), fg);
                b.FillRectangle(cx - 7,  Y(33), SX(4), SY(4), fg);
                b.FillRectangle(cx - 4,  Y(26), SX(4), SY(4), fg);
                b.FillRectangle(cx,      Y(22), SX(4), SY(4), fg);
                b.FillRectangle(cx + 4,  Y(18), SX(4), SY(4), fg);
                return;
            }
            if (idx == 6) // Close / back-arrow
            {
                b.FillRectangle(cx - 10, Y(26), SX(10), SY(4), fg);
                b.FillRectangle(cx - 10, Y(22), SX(4),  SY(4), fg);
                b.FillRectangle(cx - 10, Y(30), SX(4),  SY(4), fg);
                b.FillRectangle(cx - 10, Y(18), SX(4),  SY(4), fg);
                b.FillRectangle(cx - 10, Y(34), SX(4),  SY(4), fg);
                return;
            }

            // ── Label-based icons ───────────────────────────────────────────
            switch (label)
            {
                case "Ortho":
                    b.FillRectangle(cx - 12, Y(27), SX(24), SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(18), SX(3),  SY(21), fg);
                    b.FillRectangle(cx - 3,  Y(27), SX(7),  SY(3), accent);
                    break;

                case "Polar":
                    b.FillRectangle(cx - 1,  Y(18), SX(3),  SY(20), fg);
                    b.FillRectangle(cx - 11, Y(27), SX(22), SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(22), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 7,  Y(19), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 10, Y(16), SX(3),  SY(3), accent);
                    break;

                case "OSnap":
                    b.FillRectangle(cx - 7,  Y(18), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 9,  Y(21), SX(3),  SY(10), fg);
                    b.FillRectangle(cx + 6,  Y(21), SX(3),  SY(10), fg);
                    b.FillRectangle(cx - 7,  Y(31), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 13, Y(26), SX(4),  SY(3), dim);
                    b.FillRectangle(cx + 9,  Y(26), SX(4),  SY(3), dim);
                    b.FillRectangle(cx - 1,  Y(16), SX(3),  SY(4), dim);
                    b.FillRectangle(cx - 1,  Y(34), SX(3),  SY(4), dim);
                    break;

                case "OTrack":
                    b.FillRectangle(cx - 9,  Y(26), SX(4),  SY(3), fg);
                    b.FillRectangle(cx - 3,  Y(26), SX(4),  SY(3), fg);
                    b.FillRectangle(cx + 3,  Y(26), SX(4),  SY(3), fg);
                    b.FillRectangle(cx + 9,  Y(26), SX(4),  SY(3), fg);
                    b.FillRectangle(cx - 14, Y(24), SX(4),  SY(3), accent);
                    b.FillRectangle(cx - 13, Y(22), SX(2),  SY(7), accent);
                    break;

                case "Snap":
                    for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        b.FillRectangle(cx - 8 + col * 8, Y(19) + row * SY(8), SX(3), SY(3), fg);
                    break;

                case "UndoSeg":
                    b.FillRectangle(cx - 10, Y(24), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(21), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 10, Y(27), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 5,  Y(21), SX(3),  SY(10), dim);
                    break;

                case "Tab":
                    b.FillRectangle(cx - 11, Y(26), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 11, Y(29), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(21), SX(3),  SY(13), fg);
                    break;

                case "Close":
                    b.FillRectangle(cx - 10, Y(29), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(18), SX(3),  SY(14), fg);
                    b.FillRectangle(cx + 2,  Y(18), SX(12), SY(3), fg);
                    b.FillRectangle(cx - 13, Y(29), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 13, Y(32), SX(6),  SY(3), accent);
                    break;

                case "ArcMode":
                    b.FillRectangle(cx - 9,  Y(19), SX(18), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(7), fg);
                    b.FillRectangle(cx + 8,  Y(22), SX(3),  SY(7), fg);
                    b.FillRectangle(cx - 7,  Y(29), SX(3),  SY(4), fg);
                    b.FillRectangle(cx + 4,  Y(29), SX(3),  SY(4), fg);
                    break;

                case "Width":
                    b.FillRectangle(cx - 10, Y(19), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(32), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 2,  Y(22), SX(4),  SY(9), accent);
                    b.FillRectangle(cx - 4,  Y(22), SX(8),  SY(2), accent);
                    b.FillRectangle(cx - 4,  Y(29), SX(8),  SY(2), accent);
                    break;

                case "Join":
                    b.FillRectangle(cx - 10, Y(19), SX(3),  SY(10), fg);
                    b.FillRectangle(cx + 7,  Y(19), SX(3),  SY(10), fg);
                    b.FillRectangle(cx - 7,  Y(26), SX(6),  SY(3), fg);
                    b.FillRectangle(cx + 1,  Y(26), SX(6),  SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(29), SX(3),  SY(8), fg);
                    break;

                case "Center":
                    b.FillRectangle(cx - 8,  Y(19), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 7,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 8,  Y(31), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 2,  Y(24), SX(4),  SY(5), accent);
                    break;

                case "2Point":
                    b.FillRectangle(cx - 12, Y(24), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 8,  Y(24), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 8,  Y(26), SX(16), SY(2), dim);
                    break;

                case "3Point":
                    b.FillRectangle(cx - 2,  Y(18), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 12, Y(30), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 8,  Y(30), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 10, Y(22), SX(10), SY(2), dim);
                    b.FillRectangle(cx,      Y(22), SX(10), SY(2), dim);
                    b.FillRectangle(cx - 10, Y(32), SX(20), SY(2), dim);
                    break;

                case "Radius":
                    b.FillRectangle(cx - 8,  Y(19), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 7,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 8,  Y(31), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(25), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 1,  Y(25), SX(10), SY(2), accent);
                    break;

                case "Diameter":
                    b.FillRectangle(cx - 8,  Y(19), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 7,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 8,  Y(31), SX(16), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(26), SX(20), SY(2), accent);
                    break;

                case "Sides":
                case "Inscribed":
                case "Circum":
                case "Edge":
                    b.FillRectangle(cx - 5,  Y(18), SX(10), SY(3), fg);
                    b.FillRectangle(cx - 9,  Y(21), SX(3),  SY(4), fg);
                    b.FillRectangle(cx + 6,  Y(21), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 11, Y(25), SX(3),  SY(4), fg);
                    b.FillRectangle(cx + 8,  Y(25), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 9,  Y(29), SX(3),  SY(4), fg);
                    b.FillRectangle(cx + 6,  Y(29), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 5,  Y(33), SX(10), SY(3), fg);
                    break;

                case "Chamfer":
                case "Fillet":
                case "Rotate":
                    b.FillRectangle(cx - 10, Y(19), SX(3),  SY(15), fg);
                    b.FillRectangle(cx - 10, Y(34), SX(20), SY(3), fg);
                    b.FillRectangle(cx - 7,  Y(31), SX(4),  SY(3), accent);
                    b.FillRectangle(cx - 4,  Y(28), SX(4),  SY(3), accent);
                    break;

                case "Rotation":
                case "AngleLk":
                    b.FillRectangle(cx - 10, Y(34), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(22), SX(2),  SY(12), fg);
                    b.FillRectangle(cx - 8,  Y(20), SX(6),  SY(2), accent);
                    b.FillRectangle(cx - 2,  Y(21), SX(3),  SY(2), accent);
                    b.FillRectangle(cx + 1,  Y(23), SX(3),  SY(2), accent);
                    b.FillRectangle(cx + 3,  Y(26), SX(3),  SY(2), accent);
                    break;

                case "Area":
                case "Uniform":
                    b.FillRectangle(cx - 10, Y(19), SX(6),  SY(2), fg);
                    b.FillRectangle(cx - 1,  Y(19), SX(6),  SY(2), fg);
                    b.FillRectangle(cx + 5,  Y(19), SX(6),  SY(2), fg);
                    b.FillRectangle(cx - 10, Y(34), SX(6),  SY(2), fg);
                    b.FillRectangle(cx - 1,  Y(34), SX(6),  SY(2), fg);
                    b.FillRectangle(cx + 5,  Y(34), SX(6),  SY(2), fg);
                    b.FillRectangle(cx - 11, Y(21), SX(2),  SY(6), fg);
                    b.FillRectangle(cx - 11, Y(30), SX(2),  SY(4), fg);
                    b.FillRectangle(cx + 9,  Y(21), SX(2),  SY(6), fg);
                    b.FillRectangle(cx + 9,  Y(30), SX(2),  SY(4), fg);
                    break;

                case "LastVal":
                    b.FillRectangle(cx - 5,  Y(21), SX(10), SY(2), fg);
                    b.FillRectangle(cx - 7,  Y(23), SX(3),  SY(8), fg);
                    b.FillRectangle(cx + 4,  Y(23), SX(3),  SY(5), fg);
                    b.FillRectangle(cx - 5,  Y(31), SX(10), SY(2), fg);
                    b.FillRectangle(cx - 1,  Y(25), SX(4),  SY(5), dim);
                    b.FillRectangle(cx + 4,  Y(28), SX(4),  SY(3), fg);
                    break;

                case "BasePt":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(17), SX(3),  SY(21), fg);
                    b.FillRectangle(cx - 3,  Y(24), SX(7),  SY(7), accent);
                    break;

                case "Displace":
                    b.FillRectangle(cx - 10, Y(26), SX(18), SY(3), fg);
                    b.FillRectangle(cx + 5,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 5,  Y(29), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 12, Y(22), SX(3),  SY(10), dim);
                    break;

                case "CopyMode":
                case "Multiple":
                    b.FillRectangle(cx - 10, Y(19), SX(11), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(31), SX(11), SY(2), fg);
                    b.FillRectangle(cx - 12, Y(21), SX(2),  SY(10), fg);
                    b.FillRectangle(cx - 1,  Y(21), SX(2),  SY(10), fg);
                    b.FillRectangle(cx - 5,  Y(22), SX(12), SY(2), accent);
                    b.FillRectangle(cx - 5,  Y(33), SX(12), SY(2), accent);
                    b.FillRectangle(cx + 5,  Y(24), SX(2),  SY(9), accent);
                    break;

                case "AxisLock":
                case "Axis":
                    b.FillRectangle(cx - 6,  Y(19), SX(12), SY(2), fg);
                    b.FillRectangle(cx - 8,  Y(21), SX(2),  SY(6), fg);
                    b.FillRectangle(cx + 6,  Y(21), SX(2),  SY(6), fg);
                    b.FillRectangle(cx - 9,  Y(27), SX(18), SY(9), fg);
                    b.FillRectangle(cx - 2,  Y(29), SX(4),  SY(5), bg);
                    break;

                case "RotateCp":
                    b.FillRectangle(cx - 8,  Y(19), SX(16), SY(3), fg);
                    b.FillRectangle(cx + 5,  Y(22), SX(3),  SY(8), fg);
                    b.FillRectangle(cx - 8,  Y(30), SX(10), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(27), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 11, Y(30), SX(3),  SY(3), accent);
                    break;

                case "MirrorCp":
                    b.FillRectangle(cx - 10, Y(19), SX(8),  SY(12), fg);
                    b.FillRectangle(cx + 2,  Y(19), SX(8),  SY(12), accent);
                    b.FillRectangle(cx - 1,  Y(17), SX(2),  SY(18), dim);
                    break;

                case "ArrayCp":
                case "Rect":
                    b.FillRectangle(cx - 11, Y(18), SX(8),  SY(8), fg);
                    b.FillRectangle(cx + 3,  Y(18), SX(8),  SY(8), fg);
                    b.FillRectangle(cx - 11, Y(28), SX(8),  SY(8), fg);
                    b.FillRectangle(cx + 3,  Y(28), SX(8),  SY(8), accent);
                    break;

                case "Distance":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(2),  SY(7), fg);
                    b.FillRectangle(cx - 6,  Y(23), SX(2),  SY(5), dim);
                    b.FillRectangle(cx - 1,  Y(22), SX(2),  SY(7), dim);
                    b.FillRectangle(cx + 4,  Y(23), SX(2),  SY(5), dim);
                    b.FillRectangle(cx + 9,  Y(22), SX(2),  SY(7), fg);
                    break;

                case "Through":
                    b.FillRectangle(cx - 11, Y(27), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 2,  Y(22), SX(4),  SY(12), accent);
                    b.FillRectangle(cx - 4,  Y(24), SX(8),  SY(8), bg);
                    b.FillRectangle(cx - 2,  Y(25), SX(4),  SY(6), accent);
                    break;

                case "Both":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 11, Y(29), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 8,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 8,  Y(29), SX(3),  SY(3), fg);
                    break;

                case "EraseSrc":
                    b.FillRectangle(cx - 9,  Y(19), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 5,  Y(23), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 1,  Y(27), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 3,  Y(23), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 7,  Y(19), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 9,  Y(31), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 5,  Y(27), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 3,  Y(27), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 7,  Y(31), SX(4),  SY(4), fg);
                    break;

                case "Layer":
                    b.FillRectangle(cx - 10, Y(19), SX(20), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(25), SX(20), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(31), SX(20), SY(3), dim);
                    break;

                case "Fence":
                    b.FillRectangle(cx - 11, Y(31), SX(5),  SY(3), fg);
                    b.FillRectangle(cx - 7,  Y(27), SX(5),  SY(4), fg);
                    b.FillRectangle(cx - 3,  Y(23), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 1,  Y(19), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 4,  Y(23), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 7,  Y(27), SX(4),  SY(4), fg);
                    break;

                case "Crossing":
                    b.FillRectangle(cx - 10, Y(19), SX(5),  SY(2), fg);
                    b.FillRectangle(cx + 5,  Y(19), SX(5),  SY(2), fg);
                    b.FillRectangle(cx - 10, Y(33), SX(5),  SY(2), fg);
                    b.FillRectangle(cx + 5,  Y(33), SX(5),  SY(2), fg);
                    b.FillRectangle(cx - 12, Y(21), SX(2),  SY(5), fg);
                    b.FillRectangle(cx - 12, Y(29), SX(2),  SY(4), fg);
                    b.FillRectangle(cx + 10, Y(21), SX(2),  SY(5), fg);
                    b.FillRectangle(cx + 10, Y(29), SX(2),  SY(4), fg);
                    b.FillRectangle(cx - 4,  Y(23), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 1,  Y(28), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 1,  Y(23), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 4,  Y(28), SX(3),  SY(3), accent);
                    break;

                case "UndoTrim":
                case "UndoExt":
                    b.FillRectangle(cx - 8,  Y(25), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(28), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(22), SX(3),  SY(8), dim);
                    b.FillRectangle(cx + 4,  Y(20), SX(6),  SY(2), dim);
                    break;

                case "Extend":
                    b.FillRectangle(cx + 7,  Y(18), SX(3),  SY(17), fg);
                    b.FillRectangle(cx - 10, Y(26), SX(18), SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(29), SX(3),  SY(3), fg);
                    break;

                case "Trim":
                    b.FillRectangle(cx - 11, Y(26), SX(7),  SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(26), SX(7),  SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(19), SX(3),  SY(15), accent);
                    break;

                case "All":
                    b.FillRectangle(cx - 10, Y(19), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(35), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 12, Y(21), SX(2),  SY(14), fg);
                    b.FillRectangle(cx + 10, Y(21), SX(2),  SY(14), fg);
                    b.FillRectangle(cx - 6,  Y(28), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 2,  Y(31), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 1,  Y(26), SX(3),  SY(5), fg);
                    break;

                case "LastSel":
                    b.FillRectangle(cx - 7,  Y(19), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 9,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 6,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 7,  Y(31), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(22), SX(3),  SY(8), accent);
                    b.FillRectangle(cx - 1,  Y(26), SX(6),  SY(3), accent);
                    break;

                case "Ref":
                    b.FillRectangle(cx - 10, Y(33), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(19), SX(2),  SY(14), fg);
                    b.FillRectangle(cx - 4,  Y(29), SX(4),  SY(2), accent);
                    b.FillRectangle(cx,      Y(25), SX(4),  SY(2), accent);
                    b.FillRectangle(cx + 4,  Y(21), SX(4),  SY(2), accent);
                    break;

                case "Copy":
                    b.FillRectangle(cx - 9,  Y(19), SX(10), SY(14), fg);
                    b.FillRectangle(cx - 4,  Y(24), SX(10), SY(11), accent);
                    break;

                case "Xonly":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx + 7,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 7,  Y(29), SX(3),  SY(3), fg);
                    // X mark (pixel art, no text)
                    b.FillRectangle(cx - 10, Y(17), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 7,  Y(20), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 4,  Y(23), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 4,  Y(17), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 7,  Y(20), SX(3),  SY(3), accent);
                    b.FillRectangle(cx - 10, Y(23), SX(3),  SY(3), accent);
                    break;

                case "Yonly":
                    b.FillRectangle(cx - 1,  Y(26), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 1,  Y(16), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 5,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 3,  Y(22), SX(3),  SY(3), fg);
                    // Y mark (pixel art, no text)
                    b.FillRectangle(cx + 4,  Y(17), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 7,  Y(20), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 6,  Y(23), SX(2),  SY(4), accent);
                    b.FillRectangle(cx + 4,  Y(23), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 10, Y(20), SX(3),  SY(3), accent);
                    break;

                case "AxisPick":
                    b.FillRectangle(cx - 1,  Y(18), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 1,  Y(24), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 1,  Y(30), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 3,  Y(16), SX(7),  SY(2), accent);
                    b.FillRectangle(cx - 5,  Y(14), SX(11), SY(2), accent);
                    break;

                case "Horiz":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 11, Y(29), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 8,  Y(22), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 8,  Y(29), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(19), SX(2),  SY(14), dim);
                    break;

                case "Vert":
                    b.FillRectangle(cx - 1,  Y(18), SX(3),  SY(22), fg);
                    b.FillRectangle(cx - 4,  Y(18), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 2,  Y(18), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 4,  Y(37), SX(3),  SY(3), fg);
                    b.FillRectangle(cx + 2,  Y(37), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 10, Y(27), SX(20), SY(2), dim);
                    break;

                case "Rot180":
                    b.FillRectangle(cx - 9,  Y(19), SX(18), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 8,  Y(22), SX(3),  SY(9), fg);
                    b.FillRectangle(cx - 9,  Y(31), SX(18), SY(3), fg);
                    b.FillRectangle(cx - 2,  Y(24), SX(4),  SY(5), accent);
                    break;

                case "Polyline":
                    b.FillRectangle(cx - 11, Y(31), SX(6),  SY(3), fg);
                    b.FillRectangle(cx - 6,  Y(25), SX(3),  SY(6), fg);
                    b.FillRectangle(cx - 6,  Y(25), SX(8),  SY(3), fg);
                    b.FillRectangle(cx + 1,  Y(25), SX(3),  SY(9), fg);
                    b.FillRectangle(cx + 1,  Y(31), SX(8),  SY(3), fg);
                    b.FillRectangle(cx + 8,  Y(19), SX(3),  SY(12), fg);
                    break;

                case "Chain":
                    b.FillRectangle(cx - 11, Y(24), SX(8),  SY(7), fg);
                    b.FillRectangle(cx - 9,  Y(26), SX(4),  SY(3), bg);
                    b.FillRectangle(cx - 2,  Y(24), SX(2),  SY(7), fg);
                    b.FillRectangle(cx + 1,  Y(24), SX(8),  SY(7), fg);
                    b.FillRectangle(cx + 3,  Y(26), SX(4),  SY(3), bg);
                    break;

                case "PolarArr":
                    b.FillRectangle(cx - 2,  Y(18), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 7,  Y(24), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 2,  Y(31), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 11, Y(24), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 2,  Y(26), SX(4),  SY(3), accent);
                    break;

                case "Path":
                    b.FillRectangle(cx - 11, Y(33), SX(6),  SY(3), fg);
                    b.FillRectangle(cx - 6,  Y(29), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 4,  Y(25), SX(4),  SY(4), fg);
                    b.FillRectangle(cx,      Y(22), SX(5),  SY(3), fg);
                    b.FillRectangle(cx + 4,  Y(19), SX(6),  SY(3), fg);
                    b.FillRectangle(cx + 9,  Y(18), SX(3),  SY(6), fg);
                    break;

                case "Rows":
                    b.FillRectangle(cx - 10, Y(19), SX(20), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(26), SX(20), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(33), SX(20), SY(3), dim);
                    break;

                case "Cols":
                    b.FillRectangle(cx - 10, Y(19), SX(3),  SY(18), fg);
                    b.FillRectangle(cx - 1,  Y(19), SX(3),  SY(18), fg);
                    b.FillRectangle(cx + 8,  Y(19), SX(3),  SY(18), dim);
                    break;

                case "Space":
                    b.FillRectangle(cx - 11, Y(26), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(21), SX(2),  SY(12), fg);
                    b.FillRectangle(cx + 9,  Y(21), SX(2),  SY(12), fg);
                    b.FillRectangle(cx - 3,  Y(22), SX(6),  SY(2), accent);
                    b.FillRectangle(cx - 3,  Y(30), SX(6),  SY(2), accent);
                    break;

                case "Linear":
                    b.FillRectangle(cx - 11, Y(28), SX(22), SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(2),  SY(9), fg);
                    b.FillRectangle(cx + 9,  Y(22), SX(2),  SY(9), fg);
                    b.FillRectangle(cx - 3,  Y(23), SX(6),  SY(2), accent);
                    break;

                case "Aligned":
                    b.FillRectangle(cx - 11, Y(32), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(28), SX(3),  SY(4), fg);
                    b.FillRectangle(cx - 5,  Y(24), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 1,  Y(20), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 3,  Y(19), SX(3),  SY(4), fg);
                    b.FillRectangle(cx + 7,  Y(19), SX(3),  SY(16), fg);
                    break;

                case "Continue":
                    b.FillRectangle(cx - 11, Y(26), SX(8),  SY(3), fg);
                    b.FillRectangle(cx + 3,  Y(26), SX(8),  SY(3), fg);
                    b.FillRectangle(cx - 11, Y(22), SX(2),  SY(8), fg);
                    b.FillRectangle(cx - 1,  Y(22), SX(2),  SY(8), fg);
                    b.FillRectangle(cx + 1,  Y(22), SX(2),  SY(8), fg);
                    b.FillRectangle(cx + 11, Y(22), SX(2),  SY(8), fg);
                    break;

                case "Scale":
                    b.FillRectangle(cx - 2,  Y(26), SX(4),  SY(3), accent);
                    b.FillRectangle(cx - 11, Y(26), SX(8),  SY(3), fg);
                    b.FillRectangle(cx + 3,  Y(26), SX(8),  SY(3), fg);
                    b.FillRectangle(cx - 2,  Y(18), SX(4),  SY(7), fg);
                    b.FillRectangle(cx - 2,  Y(33), SX(4),  SY(7), fg);
                    break;

                case "Flip":
                    b.FillRectangle(cx - 11, Y(28), SX(10), SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(24), SX(7),  SY(4), fg);
                    b.FillRectangle(cx - 5,  Y(21), SX(4),  SY(3), fg);
                    b.FillRectangle(cx + 1,  Y(24), SX(7),  SY(4), accent);
                    b.FillRectangle(cx + 1,  Y(28), SX(10), SY(3), accent);
                    b.FillRectangle(cx + 1,  Y(21), SX(4),  SY(3), accent);
                    b.FillRectangle(cx - 1,  Y(19), SX(2),  SY(18), dim);
                    break;

                case "Single":
                    b.FillRectangle(cx - 3,  Y(18), SX(2),  SY(16), fg);
                    b.FillRectangle(cx + 1,  Y(18), SX(2),  SY(16), fg);
                    b.FillRectangle(cx - 1,  Y(24), SX(2),  SY(2), fg);
                    b.FillRectangle(cx - 10, Y(33), SX(20), SY(2), dim);
                    break;

                case "Multi":
                    b.FillRectangle(cx - 10, Y(19), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(24), SX(16), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(29), SX(18), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(34), SX(12), SY(2), dim);
                    break;

                case "Height":
                    b.FillRectangle(cx - 1,  Y(18), SX(3),  SY(19), fg);
                    b.FillRectangle(cx - 4,  Y(18), SX(9),  SY(3), fg);
                    b.FillRectangle(cx - 4,  Y(34), SX(9),  SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(25), SX(7),  SY(3), dim);
                    break;

                case "Style":
                    b.FillRectangle(cx - 6,  Y(19), SX(12), SY(3), fg);
                    b.FillRectangle(cx - 8,  Y(22), SX(3),  SY(5), fg);
                    b.FillRectangle(cx - 6,  Y(27), SX(12), SY(3), fg);
                    b.FillRectangle(cx + 5,  Y(30), SX(3),  SY(5), fg);
                    b.FillRectangle(cx - 6,  Y(35), SX(12), SY(3), fg);
                    break;

                case "Align":
                    b.FillRectangle(cx - 10, Y(20), SX(20), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(26), SX(14), SY(2), fg);
                    b.FillRectangle(cx - 10, Y(32), SX(17), SY(2), fg);
                    b.FillRectangle(cx - 12, Y(19), SX(2),  SY(16), accent);
                    break;

                default:
                    // Generic diamond fallback
                    b.FillRectangle(cx - 1,  Y(19), SX(3),  SY(3), fg);
                    b.FillRectangle(cx - 4,  Y(22), SX(9),  SY(3), fg);
                    b.FillRectangle(cx - 7,  Y(25), SX(15), SY(3), fg);
                    b.FillRectangle(cx - 4,  Y(28), SX(9),  SY(3), fg);
                    b.FillRectangle(cx - 1,  Y(31), SX(3),  SY(3), fg);
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