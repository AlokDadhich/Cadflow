namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  UNDO — assigned to Top-Left button via DefaultProfile.
    //  Sends Ctrl+Z (Command+Z on Mac) to AutoCAD.
    // ══════════════════════════════════════════════════════════════════════════
    public class UndoAction : PluginDynamicCommand
    {
        public UndoAction()
            : base(
                displayName: "Undo",
                description:  "Undo last action in AutoCAD (Cmd+Z)",
                groupName:    "CadFlow")
        { }

        protected override void RunCommand(string actionParameter)
        {
            AcadSend.SendShortcut("z", "command down");
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            return DrawUndoTile(imageSize);
        }

        private static BitmapImage DrawUndoTile(PluginImageSize size)
        {
            var bg     = new BitmapColor(14, 16, 36);
            var fg     = new BitmapColor(110, 170, 255);
            var accent = new BitmapColor(55, 95, 215);
            var dim    = new BitmapColor(30, 55, 130);

            var b = new BitmapBuilder(size);
            int W = b.Width; int H2 = b.Height; int cx = W / 2;
            double sy = H2 / 60.0; double sx = W / 60.0;
            int Y(int y) => (int)(y * sy);
            int SY(int h) => System.Math.Max(1, (int)(h * sy));
            int SX(int w) => System.Math.Max(1, (int)(w * sx));
            b.FillRectangle(0, 0, W, b.Height, bg);
            b.FillRectangle(0, 0, W, 3, accent);

            // Counter-clockwise arc arrow
            b.FillRectangle(cx - 9, Y(20), SX(16), SY(3), fg);   // arc top
            b.FillRectangle(cx + 5, Y(23), SX(3), SY(9), fg);   // arc right side
            b.FillRectangle(cx - 9, Y(32), SX(12), SY(3), fg);   // arc bottom
            b.FillRectangle(cx - 11, Y(23), SX(3), SY(9), dim);  // arc left (dim = open)
            // arrowhead pointing left
            b.FillRectangle(cx - 12, Y(20), SX(3), SY(3), fg);
            b.FillRectangle(cx - 12, Y(23), SX(3), SY(3), fg);
            b.FillRectangle(cx - 9, Y(17), SX(3), SY(3), fg);
            b.FillRectangle(cx - 9, Y(26), SX(3), SY(3), fg);

            return b.ToImage();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  REDO — assigned to Top-Right button via DefaultProfile.
    //  Sends Ctrl+Y (Command+Shift+Z on Mac) to AutoCAD.
    // ══════════════════════════════════════════════════════════════════════════
    public class RedoAction : PluginDynamicCommand
    {
        public RedoAction()
            : base(
                displayName: "Redo",
                description:  "Redo last undone action in AutoCAD (Cmd+Shift+Z)",
                groupName:    "CadFlow")
        { }

        protected override void RunCommand(string actionParameter)
        {
            // AutoCAD Mac uses Command+Shift+Z for redo
            AcadSend.SendShortcut("Z", "command down, shift down");
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            return DrawRedoTile(imageSize);
        }

        private static BitmapImage DrawRedoTile(PluginImageSize size)
        {
            var bg     = new BitmapColor(14, 28, 18);
            var fg     = new BitmapColor(80, 210, 120);
            var accent = new BitmapColor(35, 150, 70);
            var dim    = new BitmapColor(20, 80, 40);

            var b = new BitmapBuilder(size);
            int W = b.Width; int H2 = b.Height; int cx = W / 2;
            double sy = H2 / 60.0; double sx = W / 60.0;
            int Y(int y) => (int)(y * sy);
            int SY(int h) => System.Math.Max(1, (int)(h * sy));
            int SX(int w) => System.Math.Max(1, (int)(w * sx));
            b.FillRectangle(0, 0, W, b.Height, bg);
            b.FillRectangle(0, 0, W, 3, accent);

            // Clockwise arc arrow (mirror of undo)
            b.FillRectangle(cx - 9, Y(20), SX(16), SY(3), fg);
            b.FillRectangle(cx - 11, Y(23), SX(3), SY(9), fg);
            b.FillRectangle(cx - 9, Y(32), SX(12), SY(3), fg);
            b.FillRectangle(cx + 6, Y(23), SX(3), SY(9), dim);
            // arrowhead pointing right
            b.FillRectangle(cx + 9, Y(20), SX(3), SY(3), fg);
            b.FillRectangle(cx + 9, Y(23), SX(3), SY(3), fg);
            b.FillRectangle(cx + 6, Y(17), SX(3), SY(3), fg);
            b.FillRectangle(cx + 6, Y(26), SX(3), SY(3), fg);

            return b.ToImage();
        }
    }
}
