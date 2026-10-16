using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Catastrophes
{
    public sealed class CatastrophesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CatastrophesPresenter>();
        }
    }
}
