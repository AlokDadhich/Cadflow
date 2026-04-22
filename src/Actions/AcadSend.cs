namespace Loupedeck.CadFlow
{
    using System.Diagnostics;
    using System.IO;

    internal static class AcadSend
    {
        private const string AcadApp = "AutoCAD 2027";

        // ── Public API ────────────────────────────────────────────────────────

        // Blocking — for tool buttons (types keystrokes + Enter into AutoCAD)
        public static void Send(string command)
        {
            try { RunScript(KeystrokeScript(Sanitise(command)), blocking: true); }
            catch { }
        }

        // Non-blocking — for NudgeEngine (high frequency wheel input)
        public static void SendAsync(string command)
        {
            try { RunScript(KeystrokeScript(Sanitise(command)), blocking: false); }
            catch { }
        }

        // Sends Enter key (key code 36)
        public static void SendEnter()
        {
            try { RunScript(KeyCodeScript(36), blocking: true); }
            catch { }
        }

        // Sends Escape key (key code 53)
        public static void SendEscape()
        {
            try { RunScript(KeyCodeScript(53), blocking: true); }
            catch { }
        }

        // Sends Tab key (key code 48) — toggles between angle/length input in LINE/PLINE
        public static void SendTab()
        {
            try { RunScript(KeyCodeScript(48), blocking: true); }
            catch { }
        }

        // Sends a keystroke with modifier(s) — e.g. Cmd+Z for Undo, Cmd+Shift+Z for Redo
        // modifiers: comma-separated list like "command down" or "command down, shift down"
        public static void SendShortcut(string key, string modifiers)
        {
            try { RunScript(ShortcutScript(key, modifiers), blocking: true); }
            catch { }
        }

        // Toggles AutoCAD Mac modes using verified working shortcuts.
        // All work INSIDE active commands (mid-LINE, mid-PLINE, etc.)
        //
        //   ORTHOMODE → Command + L           keystroke "l" using {command down}
        //   POLARMODE → Command + U           keystroke "u" using {command down}
        //   OSMODE    → F3                    key code 99  (no modifier needed)
        //   AUTOSNAP  → Shift + Command + T   keystroke "T" using {command down}
        //                                     (uppercase T implies shift)
        public static void SendToggle(string sysVar)
        {
            try
            {
                string script = sysVar.ToUpperInvariant() switch
                {
                    "ORTHOMODE" => ShortcutScript("l", "command down"),
                    "POLARMODE" => ShortcutScript("u", "command down"),
                    "OSMODE"    => KeyCodeScript(99),
                    "AUTOSNAP"  => ShortcutScript("T", "command down"),
                    _           => null,
                };

                if (script == null) return;
                RunScript(script, blocking: true);
            }
            catch { }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static string Sanitise(string cmd) =>
            cmd.Replace("\\", "\\\\")
               .Replace("\"", "\\\"")
               .Replace("\n", "")
               .ToLower();

        // Types a string into AutoCAD then presses Enter — used for normal commands
        private static string KeystrokeScript(string safe) => $@"
tell application ""{AcadApp}""
    activate
end tell
delay 0.15
tell application ""System Events""
    tell process ""{AcadApp}""
        set frontmost to true
        keystroke ""{safe}""
        key code 36
    end tell
end tell";

        // Sends a raw key code — Enter (36), Escape (53), Tab (48), F3 (99)
        private static string KeyCodeScript(int code) => $@"
tell application ""{AcadApp}""
    activate
end tell
delay 0.15
tell application ""System Events""
    tell process ""{AcadApp}""
        set frontmost to true
        key code {code}
    end tell
end tell";

        // Sends a keystroke with modifier keys — Command+L, Command+U, Command+T
        private static string ShortcutScript(string key, string modifiers) => $@"
tell application ""{AcadApp}""
    activate
end tell
delay 0.15
tell application ""System Events""
    tell process ""{AcadApp}""
        set frontmost to true
        keystroke ""{key}"" using {{{modifiers}}}
    end tell
end tell";

        private static void RunScript(string script, bool blocking)
        {
            string tmp = Path.GetTempFileName() + ".scpt";
            File.WriteAllText(tmp, script);
            var psi = new ProcessStartInfo
            {
                FileName        = "/usr/bin/osascript",
                ArgumentList    = { tmp },
                UseShellExecute = false,
                CreateNoWindow  = true,
            };
            var p = Process.Start(psi);
            if (blocking)
            {
                p?.WaitForExit(3000);
                try { File.Delete(tmp); } catch { }
            }
        }
    }
}