namespace Loupedeck.CadFlow
{
    // Pan X — assigned to the Roller (horizontal wheel) via DefaultProfile.
    // Rotating Roller pans the AutoCAD drawing left and right.

    public class NudgeXAdjustment : PluginDynamicAdjustment
    {
        public NudgeXAdjustment()
            : base(
                displayName: "Pan X",
                description:  "Roller → pan drawing left / right",
                groupName:    "CadFlow",
                hasReset:     false)
        { }

        protected override void ApplyAdjustment(string actionParameter, int diff)
            => NudgeEngine.Instance.FeedX(diff);

        protected override string GetAdjustmentValue(string actionParameter) => "";
    }
}
