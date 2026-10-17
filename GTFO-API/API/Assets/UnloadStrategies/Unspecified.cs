namespace GTFO.API.Assets.UnloadStrategies;

public abstract partial class AssetBundleUnloadStrategy
{
    public sealed class Unspecified : AssetBundleUnloadStrategy
    {
        public override string StrategyName => nameof(Unspecified);

        public override void Apply()
        {

        }

        public override void Unapply()
        {

        }
    }
}
