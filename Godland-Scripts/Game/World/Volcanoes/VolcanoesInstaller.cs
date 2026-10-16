using Godland.Core.World.Volcanoes;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Volcanoes
{
    public sealed class VolcanoesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<VolcanoBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<VolcanoPresenter>();
        }
    }
}
