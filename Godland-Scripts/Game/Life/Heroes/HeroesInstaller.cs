using Godland.Core.Life.Heroes;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Heroes
{
    public sealed class HeroesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<HeroesPresenter>();
        }
    }
}
