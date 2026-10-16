using Godland.Game.World.Evolution;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Actions
{
    public sealed class InteractionInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {

            builder.RegisterComponentInHierarchy<TileActionsView>();
            builder.RegisterEntryPoint<TileActionsPresenter>();
            builder.Register<HintSettings>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<HintsToggleView>();
            builder.RegisterEntryPoint<HintsTogglePresenter>();
            builder.Register<EvolutionBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
        }
    }
}
