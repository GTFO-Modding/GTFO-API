namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    /// <summary>
    /// <see cref="AssetBundleProfile"/>: Unspecified
    /// <para>
    /// Do nothing ;)
    /// </para>
    /// </summary>
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
