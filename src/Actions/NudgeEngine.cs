namespace Loupedeck.CadFlow
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class NudgeEngine
    {
        public static readonly NudgeEngine Instance = new NudgeEngine();
        private NudgeEngine() { }

        private const double FineScale  = 0.6;
        private const double TurboCoeff = 1.5;
        private const double Exponent   = 1.65;
        private const int    MaxDelta   = 12;
        private const string AcadApp    = "AutoCAD 2027";

        // ── Pending delta — accumulates while osascript is busy ───────────
        private int _pendingDx = 0;
        private int _pendingDy = 0;
        private readonly object _pendingLock = new object();

        // ── Per-axis timing ───────────────────────────────────────────────
        private double   _accumX;
        private double   _accumY;
        private DateTime _lastEventTime = DateTime.MinValue;
        private const int IdleResetMs = 60;

        // ── In-flight: 0 = free, 1 = osascript running ───────────────────
        private int _inFlight = 0;

        public void FeedX(int diff)
        {
            var now = DateTime.Now;
            ResetIfIdle(now);
            _lastEventTime = now;

            _accumX += Velocity(diff);

            int dx = (int)Math.Truncate(_accumX);
            if (dx == 0) return;
            _accumX -= dx;

            // Add to pending — will be picked up next free send
            lock (_pendingLock)
            {
                _pendingDx += -dx;
            }

            TryFire();
        }

        public void FeedY(int diff)
        {
            var now = DateTime.Now;
            ResetIfIdle(now);
            _lastEventTime = now;

            _accumY += Velocity(diff);

            int dy = (int)Math.Truncate(_accumY);
            if (dy == 0) return;
            _accumY -= dy;

            lock (_pendingLock)
            {
                _pendingDy += dy;
            }

            TryFire();
        }

        private void ResetIfIdle(DateTime now)
        {
            if (_lastEventTime == DateTime.MinValue) return;
            if ((now - _lastEventTime).TotalMilliseconds > IdleResetMs)
            {
                // Gap detected — wipe everything so old values don't fire
                _accumX = 0;
                _accumY = 0;
                lock (_pendingLock)
                {
                    _pendingDx = 0;
                    _pendingDy = 0;
                }
            }
        }

        private void TryFire()
        {
            // If already sending, just leave pending values for when it finishes
            if (Interlocked.CompareExchange(ref _inFlight, 1, 0) != 0)
                return;

            Task.Run(() =>
            {
                try
                {
                    // Drain the full pending delta in one shot
                    int dx, dy;
                    lock (_pendingLock)
                    {
                        dx = _pendingDx; _pendingDx = 0;
                        dy = _pendingDy; _pendingDy = 0;
                    }

                    if (dx != 0 || dy != 0)
                        SendPan(dx, dy);
                }
                finally
                {
                    Interlocked.Exchange(ref _inFlight, 0);

                    // If more pending arrived while we were sending, fire again
                    bool hasMore;
                    lock (_pendingLock)
                        hasMore = _pendingDx != 0 || _pendingDy != 0;

                    if (hasMore) TryFire();
                }
            });
        }

        private static double Velocity(int raw)
        {
            int c = Math.Clamp(raw, -MaxDelta, MaxDelta);
            double abs = Math.Abs(c);
            return Math.Sign(c) * (FineScale + TurboCoeff * Math.Pow(abs, Exponent));
        }

        private static void SendPan(int dx, int dy)
        {
            string command = $"'_-pan 0,0 {dx},{dy} ";
            string script  =
                $"tell application \"System Events\" to tell process \"{AcadApp}\" to keystroke \"{command}\"";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = "/usr/bin/osascript",
                    ArgumentList           = { "-e", script },
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError  = false,
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(80);
            }
            catch { }
        }
    }
}