namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    public sealed class Unspecified : AssetBundleProfile
    {
        public override string ProfileName => nameof(Unspecified);

        public override void Apply()
        {

        }

        public override void Unapply()
        {

        }
    }
}
