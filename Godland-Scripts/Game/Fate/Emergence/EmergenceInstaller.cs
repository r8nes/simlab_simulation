using Godland.Core;
using Godland.Core.Fate.Emergence;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Emergence
{
    public sealed class EmergenceInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EmergencePresenter>();
        }
    }
}
