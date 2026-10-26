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
        /// <inheritdoc/>
        public override string ProfileName => nameof(Unspecified);

        /// <inheritdoc/>
        public override void Apply()
        {

        }

        /// <inheritdoc/>
        public override void Unapply()
        {

        }
    }
}
