namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  MODIFY TOOLS
    //  Group: "CAD Modify"
    // ══════════════════════════════════════════════════════════════════════════

    public class MoveTool : ModifyToolFolder
    {
        public MoveTool() { Init(); }
        protected override string ToolName => "Move";
        protected override string ToolCmd  => "move";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("BasePt",   ""),
            new CtxBtn("Displace", "d"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("Ortho",    "orthomode"),
            new CtxBtn("AxisLock", "_axislock"),
            new CtxBtn("CopyMode", "c"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class CopyTool : ModifyToolFolder
    {
        public CopyTool() { Init(); }
        protected override string ToolName => "Copy";
        protected override string ToolCmd  => "copy";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("BasePt",   ""),
            new CtxBtn("Multiple", "m"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("RotateCp", "r"),
            new CtxBtn("MirrorCp", "mirror"),
            new CtxBtn("ArrayCp",  "a"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class OffsetTool : ModifyToolFolder
    {
        public OffsetTool() { Init(); }
        protected override string ToolName => "Offset";
        protected override string ToolCmd  => "offset";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Distance", ""),
            new CtxBtn("Through",  "t"),
            new CtxBtn("Both",     "b"),
            new CtxBtn("EraseSrc", "e"),
            new CtxBtn("Layer",    "l"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class TrimTool : ModifyToolFolder
    {
        public TrimTool() { Init(); }
        protected override string ToolName => "Trim";
        protected override string ToolCmd  => "trim";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Fence",    "f"),
            new CtxBtn("Crossing", "c"),
            new CtxBtn("Edge",     "e"),
            new CtxBtn("UndoTrim", "u"),
            new CtxBtn("Extend",   "ex"),
            new CtxBtn("All",      "all"),
            new CtxBtn("LastSel",  "p"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class ExtendTool : ModifyToolFolder
    {
        public ExtendTool() { Init(); }
        protected override string ToolName => "Extend";
        protected override string ToolCmd  => "extend";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Fence",    "f"),
            new CtxBtn("Crossing", "c"),
            new CtxBtn("Edge",     "e"),
            new CtxBtn("UndoExt",  "u"),
            new CtxBtn("Trim",     "t"),
            new CtxBtn("All",      "all"),
            new CtxBtn("LastSel",  "p"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class RotateTool : ModifyToolFolder
    {
        public RotateTool() { Init(); }
        protected override string ToolName => "Rotate";
        protected override string ToolCmd  => "rotate";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("BasePt",  ""),
            new CtxBtn("Copy",    "c"),
            new CtxBtn("Ref",     "r"),
            new CtxBtn("AngleLk", "_angbase"),
            new CtxBtn("Snap",    "snapmode"),
            new CtxBtn("Axis",    "_axislock"),
            new CtxBtn("LastVal", "@"),
            new CtxBtn("Confirm", ""),
        };
    }

    public class ScaleTool : ModifyToolFolder
    {
        public ScaleTool() { Init(); }
        protected override string ToolName => "Scale";
        protected override string ToolCmd  => "scale";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("BasePt",  ""),
            new CtxBtn("Ref",     "r"),
            new CtxBtn("Copy",    "c"),
            new CtxBtn("Uniform", ""),
            new CtxBtn("Xonly",   "x"),
            new CtxBtn("Yonly",   "y"),
            new CtxBtn("LastVal", "@"),
            new CtxBtn("Confirm", ""),
        };
    }

    public class MirrorTool : ModifyToolFolder
    {
        public MirrorTool() { Init(); }
        protected override string ToolName => "Mirror";
        protected override string ToolCmd  => "mirror";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("AxisPick", ""),
            new CtxBtn("Copy",     "c"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("Horiz",    "h"),
            new CtxBtn("Vert",     "v"),
            new CtxBtn("Rot180",   "r"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class FilletTool : ModifyToolFolder
    {
        public FilletTool() { Init(); }
        protected override string ToolName => "Fillet";
        protected override string ToolCmd  => "fillet";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Radius",   "r"),
            new CtxBtn("Trim",     "t"),
            new CtxBtn("Multiple", "m"),
            new CtxBtn("Polyline", "p"),
            new CtxBtn("Chain",    "c"),
            new CtxBtn("Snap",     "snapmode"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }

    public class ChamferTool : ModifyToolFolder
    {
        public ChamferTool() { Init(); }
        protected override string ToolName => "Chamfer";
        protected override string ToolCmd  => "chamfer";
        protected override CtxBtn[] ContextBtns => new[]
        {
            new CtxBtn("Dist1",    "d"),
            new CtxBtn("Dist2",    "d"),
            new CtxBtn("Angle",    "a"),
            new CtxBtn("Trim",     "t"),
            new CtxBtn("Multiple", "m"),
            new CtxBtn("Polyline", "p"),
            new CtxBtn("LastVal",  "@"),
            new CtxBtn("Confirm",  ""),
        };
    }
}
