using VContainer;
using VContainer.Unity;
using Godland.Core.World.Climate;

namespace Godland.Game.World.Climate
{
    public sealed class ClimateInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ClimatePresenter>();
        }
    }
}
