namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  PRECISION TOOLS
    //  Group: "CAD Precision"
    // ══════════════════════════════════════════════════════════════════════════

    public class DimensionTool : PrecisionToolFolder
    {
        public DimensionTool() { Init(); }
        protected override string ToolName => "Dimension";
        protected override string ToolCmd  => "dimlinear";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Linear",   "dimlinear"),
            new CtxBtn("Aligned",  "dimaligned"),
            new CtxBtn("Radius",   "dimradius"),
            new CtxBtn("Diameter", "dimdiameter"),
            new CtxBtn("Angle",    "dimangular"),
            new CtxBtn("Continue", "dimcontinue"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class AlignTool : PrecisionToolFolder
    {
        public AlignTool() { Init(); }
        protected override string ToolName => "Align";
        protected override string ToolCmd  => "align";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("2Point",  ""),
            new CtxBtn("3Point",  ""),
            new CtxBtn("Scale",   "s"),
            new CtxBtn("Rotate",  "r"),
            new CtxBtn("Snap",    "snapmode"),
            new CtxBtn("Flip",    "f"),
            new CtxBtn("LastVal", "@"),
            new CtxBtn("Confirm", ""),
        };
    }
}
