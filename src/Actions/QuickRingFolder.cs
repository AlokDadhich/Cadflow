namespace Loupedeck.CadFlow
{
    public class OrthoCommand : PluginDynamicCommand
    {
        public OrthoCommand() : base("Ortho Toggle", "Toggle Ortho mode", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("ORTHO\n");
    }

    public class PolarCommand : PluginDynamicCommand
    {
        public PolarCommand() : base("Polar Toggle", "Toggle Polar mode", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("POLAR\n");
    }

    public class OsnapCommand : PluginDynamicCommand
    {
        public OsnapCommand() : base("Osnap Toggle", "Toggle Osnap", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("OSNAP\n");
    }

    public class OtrackCommand : PluginDynamicCommand
    {
        public OtrackCommand() : base("Otrack Toggle", "Toggle Otrack", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("OTRACK\n");
    }

    public class QuickUndoCommand : PluginDynamicCommand
    {
        public QuickUndoCommand() : base("Quick Undo", "Undo via command line", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("UNDO\n");
    }

    public class QuickRedoCommand : PluginDynamicCommand
    {
        public QuickRedoCommand() : base("Quick Redo", "Redo via command line", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.Send("REDO\n");
    }

    public class EnterCommand : PluginDynamicCommand
    {
        public EnterCommand() : base("Enter", "Send Enter key", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.SendEnter();
    }

    public class CancelCommand : PluginDynamicCommand
    {
        public CancelCommand() : base("Cancel", "Send Escape key", "CadFlow") { }
        protected override void RunCommand(string actionParameter) => AcadSend.SendEscape();
    }
}