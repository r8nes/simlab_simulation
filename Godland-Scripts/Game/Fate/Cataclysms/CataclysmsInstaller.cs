using Godland.Core.Fate.Cataclysms;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Cataclysms
{
    public sealed class CataclysmsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CataclysmBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<CataclysmPresenter>();
        }
    }
}
