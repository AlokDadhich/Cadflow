namespace Loupedeck.CadFlow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  TOOL CONTEXT
    //  Tracks which CadToolFolder is currently active so that context-aware
    //  buttons (Confirm, Exit) know what state the console is in.
    //
    //  Usage:
    //    ToolContext.Activate(this)    — call from CadToolFolder.Activate()
    //    ToolContext.Deactivate(this)  — call from CadToolFolder.Deactivate()
    //    ToolContext.Current           — returns the active folder or null
    //    ToolContext.IsActive          — true if any folder is open
    // ══════════════════════════════════════════════════════════════════════════

    internal static class ToolContext
    {
        private static CadToolFolder _current;

        /// <summary>The folder that is currently open on the console, or null.</summary>
        public static CadToolFolder Current => _current;

        /// <summary>True when a tool folder is open on the console.</summary>
        public static bool IsActive => _current != null;

        /// <summary>Called by CadToolFolder.Activate() — registers the active folder.</summary>
        public static void Activate(CadToolFolder folder)
        {
            _current = folder;
            PluginLog.Info($"[ToolContext] Activated: {folder?.DisplayName ?? "(null)"}");
        }

        /// <summary>Called by CadToolFolder.Deactivate() — clears the active folder.</summary>
        public static void Deactivate(CadToolFolder folder)
        {
            if (_current == folder)
            {
                PluginLog.Info($"[ToolContext] Deactivated: {folder?.DisplayName ?? "(null)"}");
                _current = null;
            }
        }
    }
}
