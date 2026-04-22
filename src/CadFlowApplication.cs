namespace Loupedeck.CadFlow
{
    using System;

    // This is what makes the Roller and Scroller ONLY act as X/Y nudge
    // when AutoCAD is the active window. Any other app = default behaviour.
    //
    // If your wheels still don't switch, run this in Terminal to get your exact bundle ID:
    //   osascript -e 'id of app "AutoCAD 2027"'
    // Then paste the result into GetBundleName() below.

    public class CadFlowApplication : ClientApplication
    {
        public CadFlowApplication() { }

        protected override String GetBundleName() => "com.autodesk.autocad2027";

        protected override String GetProcessName() => "";

        public override ClientApplicationStatus GetApplicationStatus()
            => ClientApplicationStatus.Unknown;
    }
}
