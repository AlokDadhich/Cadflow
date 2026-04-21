namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  DRAW TOOLS
    //  Group: "CAD Draw"
    // ══════════════════════════════════════════════════════════════════════════

    public class LineTool : DrawToolFolder
    {
        public LineTool() { Init(); }
        protected override string ToolName => "Line";
        protected override string ToolCmd  => "line";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Ortho",   "ORTHOMODE", isToggle: true),  // Toggle – force straight lines
            new CtxBtn("Polar",   "POLARMODE", isToggle: true),  // Toggle – fixed angle guides
            new CtxBtn("OSnap",   "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("OTrack",  "AUTOSNAP",  isToggle: true),  // Toggle – object tracking
            new CtxBtn("UndoSeg", "u"),                          // Action – remove last segment
            new CtxBtn("Tab",     "TAB"),                        // Tab – toggle angle/length input
            new CtxBtn("Close",   "c"),                          // Smart – connect back to start
            new CtxBtn("Confirm", ""),                           // Global – Enter / finish
        };
    }

    public class PolylineTool : DrawToolFolder
    {
        public PolylineTool() { Init(); }
        protected override string ToolName => "Polyline";
        protected override string ToolCmd  => "pline";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Close",   "c"),
            new CtxBtn("ArcMode", "a"),
            new CtxBtn("Width",   "w"),
            new CtxBtn("UndoSeg", "u"),
            new CtxBtn("Snap",    "snapmode"),
            new CtxBtn("Join",    "j"),
            new CtxBtn("Tab",     "TAB"),
            new CtxBtn("Confirm", ""),
        };
    }

    public class CircleTool : DrawToolFolder
    {
        public CircleTool() { Init(); }
        protected override string ToolName => "Circle";
        protected override string ToolCmd  => "circle";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Center",   ""),
            new CtxBtn("2Point",   "2p"),
            new CtxBtn("3Point",   "3p"),
            new CtxBtn("Radius",   "r"),
            new CtxBtn("Diameter", "d"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class ArcTool : DrawToolFolder
    {
        public ArcTool() { Init(); }
        protected override string ToolName => "Arc";
        protected override string ToolCmd  => "arc";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("3Point",    ""),
            new CtxBtn("StartCtr",  "c"),
            new CtxBtn("StartEnd",  "e"),
            new CtxBtn("Direction", "d"),
            new CtxBtn("Radius",    "r"),
            new CtxBtn("Snap",      "snapmode"),
            new CtxBtn("LastVal",   "@"),
            new CtxBtn("Confirm",   ""),
        };
    }

    public class PolygonTool : DrawToolFolder
    {
        public PolygonTool() { Init(); }
        protected override string ToolName => "Polygon";
        protected override string ToolCmd  => "polygon";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Sides",     ""),
            new CtxBtn("Inscribed", "i"),
            new CtxBtn("Circum",    "c"),
            new CtxBtn("Edge",      "e"),
            new CtxBtn("Snap",      "snapmode"),
            new CtxBtn("Rotate",    "r"),
            new CtxBtn("LastVal",   "@"),
            new CtxBtn("Confirm",   ""),
        };
    }

    public class RectangleTool : DrawToolFolder
    {
        public RectangleTool() { Init(); }
        protected override string ToolName => "Rectangle";
        protected override string ToolCmd  => "rectang";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Chamfer",  "c"),
            new CtxBtn("Fillet",   "f"),
            new CtxBtn("Width",    "w"),
            new CtxBtn("Rotation", "r"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("Area",     "a"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }
}