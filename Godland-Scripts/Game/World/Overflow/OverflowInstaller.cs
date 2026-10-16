using Godland.Core.World.Cells;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Overflow
{
    public sealed class OverflowInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<OverflowPresenter>();
        }
    }
}
