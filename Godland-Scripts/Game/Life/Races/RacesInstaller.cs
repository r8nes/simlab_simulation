using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Races
{
    public sealed class RacesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<RacesPresenter>();
        }
    }
}
