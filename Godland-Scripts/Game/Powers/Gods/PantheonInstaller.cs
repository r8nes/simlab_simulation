using Godland.Core.Powers.Gods;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Gods
{
    public sealed class PantheonInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<PantheonBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<GodsPresenter>();
            builder.RegisterEntryPoint<TemplesPresenter>();
        }
    }
}
