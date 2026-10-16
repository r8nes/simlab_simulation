using Godland.Core.World.Corruption;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Corruption
{
    public sealed class CorruptionInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CorruptionBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<CorruptionPresenter>();
        }
    }
}
