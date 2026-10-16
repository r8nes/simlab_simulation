using Godland.Core.Life.Kingdoms;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Kingdoms
{
    public sealed class KingdomsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<KingdomBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<DynastiesPresenter>();
            builder.RegisterEntryPoint<KingdomsPresenter>();
            builder.RegisterEntryPoint<KingdomBordersPresenter>();
        }
    }
}
