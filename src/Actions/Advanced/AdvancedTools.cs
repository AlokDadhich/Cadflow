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
            new CtxBtn("Rect",     "r"),                          // Rectangular array
            new CtxBtn("Polar",    "po"),                         // Polar/circular array
            new CtxBtn("Path",     "pa"),                         // Path array along a curve
            new CtxBtn("Rows",     "\n"),                         // FIX: was "r" – conflicts with Rect, confirm row count
            new CtxBtn("Cols",     "\n"),                         // FIX: was "c" – confirm column count
            new CtxBtn("Space",    "s"),                          // Set spacing between items
            new CtxBtn("LastVal",  "'cal"),                       // FIX: was "@" – recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class TextTool : AdvancedToolFolder
    {
        public TextTool() { Init(); }
        protected override string ToolName => "Text";
        protected override string ToolCmd  => "text";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // FIX: added – snap to geometry points
            new CtxBtn("Multi",    "mtext"),                      // Switch to multiline text command
            new CtxBtn("Height",   "h"),                          // Set text height
            new CtxBtn("Rotate",   "r"),                          // Set text rotation angle
            new CtxBtn("Style",    "style"),                      // Open text style manager
            new CtxBtn("Align",    "j"),                          // Set text justification
            new CtxBtn("LastVal",  "'cal"),                       // FIX: was "@" – recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class BlockTool : AdvancedToolFolder
    {
        public BlockTool() { Init(); }
        protected override string ToolName => "Block";
        protected override string ToolCmd  => "block";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Ortho",    "ORTHOMODE", isToggle: true),  // Toggle – force straight base point
            new CtxBtn("BasePt",   "\n"),                         // Confirm base point pick
            new CtxBtn("Select",   "\n"),                         // Confirm object selection
            new CtxBtn("Annotate", "a"),                          // Make block annotative
            new CtxBtn("Scale",    "s"),                          // Allow unequal scaling
            new CtxBtn("UndoSel",  "u"),                          // Undo last selection pick
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class InsertTool : AdvancedToolFolder
    {
        public InsertTool() { Init(); }
        protected override string ToolName => "Insert";
        protected override string ToolCmd  => "insert";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Ortho",    "ORTHOMODE", isToggle: true),  // Toggle – force straight insertion
            new CtxBtn("Scale",    "s"),                          // Set insertion scale
            new CtxBtn("Rotate",   "r"),                          // Set insertion rotation angle
            new CtxBtn("Explode",  "e"),                          // Explode block on insertion
            new CtxBtn("Multiple", "m"),                          // Insert multiple instances
            new CtxBtn("LastVal",  "'cal"),                       // Recall last value via CAL
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class ExplodeTool : AdvancedToolFolder
    {
        public ExplodeTool() { Init(); }
        protected override string ToolName => "Explode";
        protected override string ToolCmd  => "explode";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Select",   "\n"),                         // Confirm current selection
            new CtxBtn("Previous", "p"),                          // Reuse previous selection set
            new CtxBtn("All",      "all"),                        // Select all objects
            new CtxBtn("Window",   "w"),                          // Window selection
            new CtxBtn("Crossing", "c"),                          // Crossing window selection
            new CtxBtn("UndoSel",  "u"),                          // Undo last selection pick
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }

    public class HatchTool : AdvancedToolFolder
    {
        public HatchTool() { Init(); }
        protected override string ToolName => "Hatch";
        protected override string ToolCmd  => "hatch";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("OSnap",    "OSMODE",    isToggle: true),  // Toggle – snap to geometry points
            new CtxBtn("Pick",     "p"),                          // Pick internal point to hatch
            new CtxBtn("Select",   "s"),                          // Select boundary objects
            new CtxBtn("Pattern",  "t"),                          // Set hatch pattern type
            new CtxBtn("Scale",    "sc"),                         // Set hatch pattern scale
            new CtxBtn("Angle",    "an"),                         // Set hatch pattern angle
            new CtxBtn("UndoHtch", "u"),                          // Undo last hatch pick
            new CtxBtn("Confirm",  "\n"),                         // Confirm – accept value, stay in command
        };
    }
}