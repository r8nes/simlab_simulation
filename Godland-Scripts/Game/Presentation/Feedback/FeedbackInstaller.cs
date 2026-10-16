using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class FeedbackInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<FloatingTextSpawner>(Lifetime.Singleton);
            builder.Register<PlayerFocus>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerFocusTracker>();
            builder.RegisterEntryPoint<RewardPopupPresenter>();
            builder.RegisterEntryPoint<FertilityPopupPresenter>();
            builder.RegisterEntryPoint<TileBadgesPresenter>();
            builder.RegisterComponentInHierarchy<BadgeGlossaryView>();
            builder.RegisterEntryPoint<BadgeGlossaryPresenter>();
        }
    }
}
