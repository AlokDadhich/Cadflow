namespace Loupedeck.CadFlow
{
    using System;
    using System.Diagnostics;
    using System.IO;

    // Roller   → FeedX → pans AutoCAD view left / right
    // Scroller → FeedY → pans AutoCAD view up   / down
    //
    // Sends '_-pan via a compact single osascript call with NO delay.
    // The apostrophe prefix makes pan transparent — works during any command.
    // Accumulator prevents flooding — only fires when a full pixel is ready.

    internal sealed class NudgeEngine
    {
        public static readonly NudgeEngine Instance = new NudgeEngine();
        private NudgeEngine() { }

        private const double FineScale  = 0.6;
        private const double TurboCoeff = 1.5;
        private const double Exponent   = 1.65;
        private const int    MaxDelta   = 12;
        private const string AcadApp    = "AutoCAD 2027";

        private double _accumX;
        private double _accumY;

        public void FeedX(int diff)
        {
            _accumX += Velocity(diff);
            int dx = (int)Math.Truncate(_accumX);
            _accumX -= dx;
            if (dx == 0) return;
            SendPan(-dx, 0);
        }

        public void FeedY(int diff)
        {
            _accumY += Velocity(diff);
            int dy = (int)Math.Truncate(_accumY);
            _accumY -= dy;
            if (dy == 0) return;
            SendPan(0, dy);
        }

        private static double Velocity(int raw)
        {
            int    c   = Math.Clamp(raw, -MaxDelta, MaxDelta);
            double abs = Math.Abs(c);
            return Math.Sign(c) * (FineScale + TurboCoeff * Math.Pow(abs, Exponent));
        }

        private static void SendPan(int dx, int dy)
        {
            // Single compact AppleScript — no delay, no temp file.
            // Uses \r (return key code 52 alternative) but simpler:
            // we embed the three keystrokes + enters in one tell block.
            // key code 36 = Return/Enter
            string script =
                $"tell application \"System Events\" to tell process \"{AcadApp}\" to " +
                $"keystroke \"'_-pan\" & return & \"0,0\" & return & \"{dx},{dy}\" & return";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName        = "/usr/bin/osascript",
                    ArgumentList    = { "-e", script },
                    UseShellExecute = false,
                    CreateNoWindow  = true,
                };
                Process.Start(psi); // fire and forget — no wait
            }
            catch { }
        }
    }
}
