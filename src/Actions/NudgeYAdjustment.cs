namespace Loupedeck.CadFlow
{
    // Pan Y — assigned to the Scroller (vertical wheel) via DefaultProfile.
    // Rotating Scroller pans the AutoCAD drawing up and down.

    public class NudgeYAdjustment : PluginDynamicAdjustment
    {
        public NudgeYAdjustment()
            : base(
                displayName: "Pan Y",
                description:  "Scroller → pan drawing up / down",
                groupName:    "CadFlow",
                hasReset:     false)
        { }

        protected override void ApplyAdjustment(string actionParameter, int diff)
            => NudgeEngine.Instance.FeedY(diff);

        protected override string GetAdjustmentValue(string actionParameter) => "";
    }
}
