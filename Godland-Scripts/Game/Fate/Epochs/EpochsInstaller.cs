using Godland.Core.Fate.Epochs;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Epochs
{
    public sealed class EpochsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<EpochView>();
            builder.RegisterEntryPoint<EpochPresenter>();
        }
    }
}
