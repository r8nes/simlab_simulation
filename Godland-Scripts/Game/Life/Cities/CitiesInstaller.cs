using Godland.Core.Life.Cities;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Cities
{
    public sealed class CitiesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CityBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
            builder.RegisterEntryPoint<CitiesPresenter>();
        }
    }
}
