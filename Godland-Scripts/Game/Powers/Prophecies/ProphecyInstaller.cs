using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Prophecies
{
    public sealed class ProphecyInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ProphecyPresenter>();
        }
    }
}
