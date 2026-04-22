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
            new CtxBtn("Confirm", "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class PolylineTool : DrawToolFolder
    {
        public PolylineTool() { Init(); }
        protected override string ToolName => "Polyline";
        protected override string ToolCmd  => "pline";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Close",   "c"),                          // Smart – connect back to start
            new CtxBtn("ArcMode", "a"),                          // Switch to arc segment mode
            new CtxBtn("Polar",   "POLARMODE", isToggle: true),  // Set segment width
            new CtxBtn("Ortho",   "ORTHOMODE", isToggle: true),  // Remove last segment
            new CtxBtn("OSnap",   "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("OTrack",  "AUTOSNAP",  isToggle: true),  // Join existing lines into pline
            new CtxBtn("UndoSeg", "u"),                          // Toggle angle/length input
            new CtxBtn("Confirm", "\n"),                         // Confirm – accept value, stay in command
        };
    }

   public class CircleTool : DrawToolFolder
{
    public CircleTool() { Init(); }
    protected override string ToolName => "Circle";
    protected override string ToolCmd  => "circle";
    protected override CtxBtn[] ContextBtns => new[]
    {
        new CtxBtn("2Point",   "2p"),                        // Slot 0 – Define circle by 2 points
        new CtxBtn("3Point",   "3p"),                        // Slot 1 – Define circle by 3 points
        new CtxBtn("Polar",    "POLARMODE", isToggle: true), // Slot 2 – Toggle polar angle guides
        new CtxBtn("Ortho",    "ORTHOMODE", isToggle: true), // Slot 3 – Toggle ortho constraint
        new CtxBtn("OSnap",    "OSMODE",    isToggle: true), // Slot 4 – Toggle snap to geometry points
        new CtxBtn("OTrack",   "AUTOSNAP",  isToggle: true), // Slot 5 – Toggle object snap tracking
        new CtxBtn("TTR",      "t"),                         // Slot 6 – Tangent Tangent Radius
        new CtxBtn("Confirm",  "\n"),                        // Slot 7 – Confirm / accept value
    };
}

    public class ArcTool : DrawToolFolder
    {
        public ArcTool() { Init(); }
        protected override string ToolName => "Arc";
        protected override string ToolCmd  => "arc";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("3Point",    ""),                         // Default 3-point mode
            new CtxBtn("StartCtr",  "c"),                        // Start-Center arc
            new CtxBtn("StartEnd",  "e"),                        // Start-End arc
            new CtxBtn("Polar",   "POLARMODE", isToggle: true),  // Set segment width
            new CtxBtn("Ortho",   "ORTHOMODE", isToggle: true),  // Remove last segment
            new CtxBtn("OSnap",     "OSMODE",  isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("OTrack",  "AUTOSNAP",  isToggle: true),  // Join existing lines into pline
            new CtxBtn("Confirm",   "\n"),                       // Confirm – accept value, stay in command
        };
    }

    public class PolygonTool : DrawToolFolder
    {
        public PolygonTool() { Init(); }
        protected override string ToolName => "Polygon";
        protected override string ToolCmd  => "polygon";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Sides",     "\n"),                       // Confirm default side count
            new CtxBtn("Inscribed", "i"),                        // Inscribed in circle
            new CtxBtn("Circum",    "c"),                        // Circumscribed about circle
            new CtxBtn("Edge",      "e"),                        // Define by edge length
            new CtxBtn("OSnap",     "OSMODE",  isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Rotate",    "r"),                        // Rotate polygon
            new CtxBtn("LastVal",   "'cal"),                     // Recall last value via CAL
            new CtxBtn("Confirm",   "\n"),                       // Confirm – accept value, stay in command
        };
    }

    public class RectangleTool : DrawToolFolder
    {
        public RectangleTool() { Init(); }
        protected override string ToolName => "Rectangle";
        protected override string ToolCmd  => "rectang";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Chamfer",  "c"),                         // Set chamfer distances
            new CtxBtn("Fillet",   "f"),                         // Set fillet radius
            new CtxBtn("Width",    "w"),                         // Set line width
            new CtxBtn("Rotation", "r"),                         // Rotate rectangle
            new CtxBtn("OSnap",    "OSMODE",   isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Area",     "a"),                         // Define by area
            new CtxBtn("LastVal",  "'cal"),                      // Recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                        // Confirm – accept value, stay in command
        };
    }

    public class SplineTool : DrawToolFolder
    {
        public SplineTool() { Init(); }
        protected override string ToolName => "Spline";
        protected override string ToolCmd  => "spline";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Fit",     "f"),                          // Fit points mode (passes through points)
            new CtxBtn("CV",      "cv"),                         // Control vertices mode
            new CtxBtn("Degree",  "d"),                          // Set spline degree (1–10)
            new CtxBtn("Knots",   "k"),                          // Set knot parameterization
            new CtxBtn("UndoPt",  "u"),                          // Remove last point
            new CtxBtn("Close",   "c"),                          // Close spline back to start
            new CtxBtn("OSnap",   "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Confirm", "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class CenterlineTool : DrawToolFolder
    {
        public CenterlineTool() { Init(); }
        protected override string ToolName => "Centerline";
        protected override string ToolCmd  => "centerline";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Select1",  "\n"),                        // Confirm first line selection
            new CtxBtn("Select2",  "\n"),                        // Confirm second line selection
            new CtxBtn("Bisect",   "b"),                         // Bisect angle between two lines
            new CtxBtn("OSnap",    "OSMODE",   isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Ortho",    "ORTHOMODE",isToggle: true),  // Toggle – force orthogonal direction
            new CtxBtn("UndoSeg",  "u"),                         // Remove last placed centerline
            new CtxBtn("LastVal",  "'cal"),                      // Recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                        // Confirm – accept value, stay in command
        };
    }

    public class EllipseTool : DrawToolFolder
    {
        public EllipseTool() { Init(); }
        protected override string ToolName => "Ellipse";
        protected override string ToolCmd  => "ellipse";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Center",   "c"),                         // Define by center point
            new CtxBtn("Axis/End", ""),                          // Default – define by axis endpoint
            new CtxBtn("Arc",      "a"),                         // Draw elliptical arc instead
            new CtxBtn("Rotation", "r"),                         // Define by rotation around major axis
            new CtxBtn("OSnap",    "OSMODE",   isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Isocircle","i"),                         // Isometric circle (when isoplane active)
            new CtxBtn("LastVal",  "'cal"),                      // Recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                        // Confirm – accept value, stay in command
        };
    }
}