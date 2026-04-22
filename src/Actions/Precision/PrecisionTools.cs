namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  PRECISION TOOLS
    //  Group: "CAD Precision"
    // ══════════════════════════════════════════════════════════════════════════

    public class AlignTool : PrecisionToolFolder
    {
        public AlignTool() { Init(); }
        protected override string ToolName => "Align";
        protected override string ToolCmd  => "align";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Ortho",    "ORTHOMODE",  isToggle: true),  // Toggle – lock alignment axis straight
            new CtxBtn("OSnap",    "OSMODE",     isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Scale",    "s"),                           // Scale object to fit alignment
            new CtxBtn("Rotate",   "r"),                           // Rotate to match alignment
            new CtxBtn("2Point",   "\n"),                          // Confirm 2-point alignment
            new CtxBtn("3Point",   "\n"),                          // Confirm 3-point alignment
            new CtxBtn("UndoAln",  "u"),                           // Undo last alignment point
            new CtxBtn("Confirm",  "\n"),                          // Confirm – accept value, stay in command
        };
    }

    public class AreaTool : PrecisionToolFolder
    {
        public AreaTool() { Init(); }
        protected override string ToolName => "Area";
        protected override string ToolCmd  => "measuregeom";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Ortho",    "ORTHOMODE", isToggle: true),  // Toggle – force straight picks
            new CtxBtn("Object",   "o"),                          // Measure area of a closed object
            new CtxBtn("Add",      "a"),                          // Add area to running total
            new CtxBtn("Subtract", "s"),                          // Subtract area from running total
            new CtxBtn("UndoPt",   "u"),                          // Undo last picked point
            new CtxBtn("LastVal",  "'cal"),                       // Recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class ListTool : PrecisionToolFolder
    {
        public ListTool() { Init(); }
        protected override string ToolName => "List";
        protected override string ToolCmd  => "list";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Select",   "\n"),                         // Confirm current selection
            new CtxBtn("Previous", "p"),                          // Reuse previous selection set
            new CtxBtn("All",      "all"),                        // Select all objects to list
            new CtxBtn("Window",   "w"),                          // Window selection
            new CtxBtn("Crossing", "c"),                          // Crossing window selection
            new CtxBtn("UndoSel",  "u"),                          // Undo last selection pick
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class PropertiesTool : PrecisionToolFolder
    {
        public PropertiesTool() { Init(); }
        protected override string ToolName => "Properties";
        protected override string ToolCmd  => "properties";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Select",   "\n"),                         // Confirm current selection
            new CtxBtn("Previous", "p"),                          // Reuse previous selection set
            new CtxBtn("All",      "all"),                        // Select all objects
            new CtxBtn("Window",   "w"),                          // Window selection
            new CtxBtn("Filter",   "filter"),                     // Open selection filter dialog
            new CtxBtn("Quick",    "qselect"),                    // Quick select by property
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class IdPointTool : PrecisionToolFolder
    {
        public IdPointTool() { Init(); }
        protected override string ToolName => "ID Point";
        protected override string ToolCmd  => "id";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",     "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Ortho",     "ORTHOMODE", isToggle: true),  // Toggle – force straight picks
            new CtxBtn("Polar",     "POLARMODE", isToggle: true),  // Toggle – polar angle guides
            new CtxBtn("Node",      "no"),                         // Snap to node/point object
            new CtxBtn("Nearest",   "nea"),                        // Snap to nearest point on object
            new CtxBtn("Intersect", "int"),                        // Snap to intersection
            new CtxBtn("LastVal",   "'cal"),                       // Recall last value via CAL
            new CtxBtn("Confirm",   "\n"),                         // Confirm – accept value, stay in command
        };
    }
}