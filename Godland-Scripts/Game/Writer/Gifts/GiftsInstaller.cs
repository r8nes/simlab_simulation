using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GiftOfferView>();
            builder.RegisterEntryPoint<GiftOfferPresenter>();
            builder.RegisterEntryPoint<GiftScheduler>();
        }
    }
}
