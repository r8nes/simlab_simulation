using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Sovereigns
{
    public sealed class SovereignInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SovereignPresenter>();
        }
    }
}
