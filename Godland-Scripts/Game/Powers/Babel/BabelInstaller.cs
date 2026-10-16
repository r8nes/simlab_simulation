using Godland.Core.Powers.Babel;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Babel
{
    public sealed class BabelInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<BabelBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<BabelPresenter>();
        }
    }
}
