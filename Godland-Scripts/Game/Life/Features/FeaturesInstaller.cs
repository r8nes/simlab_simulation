using Godland.Core.Life.Features;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Features
{
    public sealed class FeaturesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CreaturesPresenter>();

            builder.RegisterEntryPoint<FeaturesPresenter>().AsSelf();
            builder.RegisterEntryPoint<ThreatPresenter>();
            builder.Register<FeatureBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
        }
    }
}
