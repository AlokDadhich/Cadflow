namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ADVANCED TOOLS
    //  Group: "CAD Advanced"
    // ══════════════════════════════════════════════════════════════════════════

    public class ArrayTool : AdvancedToolFolder
    {
        public ArrayTool() { Init(); }
        protected override string ToolName => "Array";
        protected override string ToolCmd  => "array";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Rect",    "r"),
            new CtxBtn("Polar",   "po"),
            new CtxBtn("Path",    "pa"),
            new CtxBtn("Rows",    "r"),
            new CtxBtn("Cols",    "c"),
            new CtxBtn("Space",   "s"),
            new CtxBtn("LastVal", "@"),
            new CtxBtn("Confirm", ""),
        };
    }

    public class TextTool : AdvancedToolFolder
    {
        public TextTool() { Init(); }
        protected override string ToolName => "Text";
        protected override string ToolCmd  => "text";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Single",  "text"),
            new CtxBtn("Multi",   "mtext"),
            new CtxBtn("Height",  "h"),
            new CtxBtn("Rotate",  "r"),
            new CtxBtn("Style",   "style"),
            new CtxBtn("Align",   "j"),
            new CtxBtn("LastVal", "@"),
            new CtxBtn("Confirm", ""),
        };
    }
}
