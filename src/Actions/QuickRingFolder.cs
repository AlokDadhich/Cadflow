namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  CAD QUICK RING — assigned to Bottom-Left button via DefaultProfile.
    //
    //  8 Ring Functions:
    //   0  Zoom In/Out     — wheel zooms the viewport
    //   1  Line Width      — sets LWEIGHT in real time while drawing
    //   2  Layer Switch    — cycles through layer list (CLAYER up/down)
    //   3  Rotate Angle    — nudges rotation angle in 15° increments
    //   4  Scale Factor    — increments/decrements scale by 0.1 steps
    //   5  Hatch Scale     — adjusts HPSCALE for active hatch
    //   6  Text Height     — nudges TEXTSIZE up or down
    //   7  Confirm / Enter — green button; finish / accept current input
    // ══════════════════════════════════════════════════════════════════════════

    public class QuickRingFolder : PluginDynamicFolder
    {
        // ── Ring slot definitions ────────────────────────────────────────────
        private static readonly RingSlot[] Slots = new RingSlot[]
        {
            new RingSlot(label: "Zoom",    cmdUp: "zoom\nw\n",    cmdDown: "zoom\np\n",    accentR: 90,  accentG: 180, accentB: 255),
            new RingSlot(label: "LW+",     cmdUp: "lweight\n",    cmdDown: "lweight\n",    accentR: 255, accentG: 200, accentB: 60),
            new RingSlot(label: "Layer",   cmdUp: "clayer\n",     cmdDown: "clayer\n",     accentR: 160, accentG: 120, accentB: 255),
            new RingSlot(label: "Angle",   cmdUp: "rotate\n",     cmdDown: "rotate\n",     accentR: 255, accentG: 140, accentB: 60),
            new RingSlot(label: "Scale",   cmdUp: "scale\n",      cmdDown: "scale\n",      accentR: 80,  accentG: 220, accentB: 160),
            new RingSlot(label: "Hatch",   cmdUp: "hpscale\n",    cmdDown: "hpscale\n",    accentR: 220, accentG: 80,  accentB: 160),
            new RingSlot(label: "TxtH",    cmdUp: "textsize\n",   cmdDown: "textsize\n",   accentR: 255, accentG: 255, accentB: 100),
            new RingSlot(label: "Confirm", cmdUp: "",             cmdDown: "",             accentR: 60,  accentG: 200, accentB: 80),
        };

        private static readonly string[] CtxParams =
            { "btn0","btn1","btn2","btn3","btn4","btn5","btn6","btn7" };

        // ── Constructor ──────────────────────────────────────────────────────
        public QuickRingFolder()
        {
            this.DisplayName = "Quick Ring";
            this.GroupName   = "CadFlow";
        }

        // ── Folder lifecycle ─────────────────────────────────────────────────
        public override bool Load()   => true;
        public override bool Unload() => true;

        public override bool Activate()
        {
            this.ButtonActionNamesChanged();
            return true;
        }

        public override bool Deactivate()
        {
            this.Close();
            return true;
        }

        // ── Button list ──────────────────────────────────────────────────────
        public override System.Collections.Generic.IEnumerable<string>
            GetButtonPressActionNames(DeviceType deviceType)
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

        // ── Button press → run command ───────────────────────────────────────
        public override void RunCommand(string param)
        {
            if (param == null) return;
            int idx = SlotIndex(param);
            if (idx < 0) return;

            if (idx == 7)
                AcadSend.SendEnter();
            else if (!string.IsNullOrEmpty(Slots[idx].CmdUp))
                AcadSend.Send(Slots[idx].CmdUp);
        }

        // ── Button images ────────────────────────────────────────────────────
        public override BitmapImage GetCommandImage(string param, PluginImageSize size)
        {
            if (param == null) return null;
            int idx = SlotIndex(param);
            if (idx < 0) return Blank(size);
            return DrawTile(size, idx);
        }

        public override string GetCommandDisplayName(string param, PluginImageSize size)
        {
            if (param == null) return "";
            int idx = SlotIndex(param);
            return idx >= 0 ? Slots[idx].Label : "";
        }

        // ── Tile renderer ────────────────────────────────────────────────────
        private static BitmapImage DrawTile(PluginImageSize size, int idx)
        {
            var slot = Slots[idx];

            BitmapColor fg, accent, dim;

            if (idx == 7)
            {
                fg     = new BitmapColor(70, 210, 70);
                accent = new BitmapColor(40, 160, 40);
                dim    = new BitmapColor(30, 100, 30);
            }
            else
            {
                fg     = new BitmapColor(210, 215, 225);
                accent = new BitmapColor(slot.AccentR, slot.AccentG, slot.AccentB);
                dim    = new BitmapColor(
                    (byte)(slot.AccentR / 3),
                    (byte)(slot.AccentG / 3),
                    (byte)(slot.AccentB / 3));
            }

            var b  = new BitmapBuilder(size);
            int W  = b.Width;
            int H  = b.Height;
            int cx = W / 2;

            // Pure black background — no coloured fill, no accent bar
            b.FillRectangle(0, 0, W, H, new BitmapColor(0, 0, 0));

            // Increased scale: virtual canvas 45px → ~33% larger icons
            double sy = H / 45.0;
            double sx = W / 45.0;
            int Y(int y)  => (int)(y * sy);
            int SY(int h) => System.Math.Max(1, (int)(h * sy));
            int SX(int w) => System.Math.Max(1, (int)(w * sx));

            switch (idx)
            {
                case 0: // Zoom — magnifying glass
                    b.FillRectangle(cx - 8,  Y(17), SX(14), SY(3), fg);
                    b.FillRectangle(cx - 10, Y(20), SX(3),  SY(10), fg);
                    b.FillRectangle(cx + 4,  Y(20), SX(3),  SY(10), fg);
                    b.FillRectangle(cx - 8,  Y(30), SX(14), SY(3), fg);
                    // handle
                    b.FillRectangle(cx + 4,  Y(31), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 6,  Y(34), SX(3),  SY(4), accent);
                    // + inside lens
                    b.FillRectangle(cx - 2,  Y(23), SX(7),  SY(2), accent);
                    b.FillRectangle(cx + 1,  Y(21), SX(2),  SY(6), accent);
                    break;

                case 1: // Line Weight — three lines of decreasing thickness
                    b.FillRectangle(cx - 10, Y(18), SX(20), SY(6), fg);   // thick
                    b.FillRectangle(cx - 10, Y(27), SX(20), SY(4), fg);   // medium
                    b.FillRectangle(cx - 10, Y(34), SX(20), SY(2), dim);  // thin
                    break;

                case 2: // Layer — stack of three sheets
                    b.FillRectangle(cx - 9,  Y(17), SX(18), SY(5), fg);
                    b.FillRectangle(cx - 9,  Y(24), SX(18), SY(5), fg);
                    b.FillRectangle(cx - 9,  Y(31), SX(18), SY(4), dim);
                    // dog-ear on top sheet
                    b.FillRectangle(cx + 4,  Y(17), SX(5),  SY(6), new BitmapColor(0, 0, 0));
                    b.FillRectangle(cx + 4,  Y(21), SX(5),  SY(2), accent);
                    b.FillRectangle(cx + 6,  Y(17), SX(2),  SY(6), accent);
                    break;

                case 3: // Angle — protractor arc with two legs
                    b.FillRectangle(cx - 10, Y(34), SX(20), SY(2), fg);  // base
                    b.FillRectangle(cx - 10, Y(20), SX(2),  SY(14), fg); // left leg
                    // arc stepped
                    b.FillRectangle(cx - 8,  Y(19), SX(6),  SY(2), accent);
                    b.FillRectangle(cx - 2,  Y(20), SX(4),  SY(2), accent);
                    b.FillRectangle(cx + 2,  Y(22), SX(3),  SY(2), accent);
                    b.FillRectangle(cx + 4,  Y(25), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 5,  Y(29), SX(3),  SY(3), accent);
                    // small angle mark
                    b.FillRectangle(cx - 7,  Y(29), SX(5),  SY(2), dim);
                    b.FillRectangle(cx - 5,  Y(27), SX(2),  SY(3), dim);
                    break;

                case 4: // Scale — four expanding arrows from centre
                    b.FillRectangle(cx - 1,  Y(26), SX(3),  SY(3), accent); // centre dot
                    b.FillRectangle(cx - 11, Y(26), SX(8),  SY(3), fg);    // left shaft
                    b.FillRectangle(cx + 3,  Y(26), SX(8),  SY(3), fg);    // right shaft
                    b.FillRectangle(cx - 2,  Y(17), SX(4),  SY(8), fg);    // up shaft
                    b.FillRectangle(cx - 2,  Y(33), SX(4),  SY(8), fg);    // down shaft
                    b.FillRectangle(cx - 11, Y(22), SX(3),  SY(3), accent); // left head
                    b.FillRectangle(cx - 11, Y(29), SX(3),  SY(3), accent);
                    b.FillRectangle(cx + 8,  Y(22), SX(3),  SY(3), accent); // right head
                    b.FillRectangle(cx + 8,  Y(29), SX(3),  SY(3), accent);
                    break;

                case 5: // Hatch — bordered box with dot-grid fill
                    b.FillRectangle(cx - 10, Y(18), SX(20), SY(2), fg);  // top border
                    b.FillRectangle(cx - 10, Y(36), SX(20), SY(2), fg);  // bot border
                    b.FillRectangle(cx - 10, Y(18), SX(2),  SY(20), fg); // left border
                    b.FillRectangle(cx + 8,  Y(18), SX(2),  SY(20), fg); // right border
                    // 3×3 dot grid inside
                    for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        b.FillRectangle(cx - 6 + col * 5, Y(22) + row * SY(5), SX(2), SY(2), accent);
                    break;

                case 6: // Text Height — T glyph with up/down arrow
                    b.FillRectangle(cx - 8,  Y(18), SX(16), SY(3), fg);   // T crossbar
                    b.FillRectangle(cx - 1,  Y(21), SX(3),  SY(14), fg);  // T stem
                    // arrow on right side
                    b.FillRectangle(cx + 7,  Y(18), SX(3),  SY(18), accent); // arrow shaft
                    b.FillRectangle(cx + 5,  Y(21), SX(7),  SY(2), accent);  // up head
                    b.FillRectangle(cx + 5,  Y(34), SX(7),  SY(2), accent);  // down head
                    break;

                case 7: // Confirm — checkmark
                    b.FillRectangle(cx - 10, Y(28), SX(4),  SY(8), fg);
                    b.FillRectangle(cx - 7,  Y(33), SX(4),  SY(4), fg);
                    b.FillRectangle(cx - 4,  Y(26), SX(4),  SY(4), fg);
                    b.FillRectangle(cx,      Y(22), SX(4),  SY(4), fg);
                    b.FillRectangle(cx + 4,  Y(18), SX(4),  SY(4), fg);
                    break;
            }

            return b.ToImage();
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

        // ── Inner data type ──────────────────────────────────────────────────
        private struct RingSlot
        {
            public readonly string Label;
            public readonly string CmdUp;
            public readonly string CmdDown;
            public readonly byte   AccentR;
            public readonly byte   AccentG;
            public readonly byte   AccentB;

            public RingSlot(string label, string cmdUp, string cmdDown,
                            int accentR, int accentG, int accentB)
            {
                Label   = label;
                CmdUp   = cmdUp;
                CmdDown = cmdDown;
                AccentR = (byte)accentR;
                AccentG = (byte)accentG;
                AccentB = (byte)accentB;
            }
        }
    }
}