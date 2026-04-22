namespace Loupedeck.CadFlow
{
    public class CadFlowPlugin : Plugin
    {
        public override bool UsesApplicationApiOnly => false;
        public override bool HasNoApplication        => false;

        public CadFlowPlugin()
        {
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
        }

        public override void Load()   { }
        public override void Unload() { }
    }
}
