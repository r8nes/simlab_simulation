using Godland.Core.Powers.Titans;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Titans
{
    public sealed class TitansInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<TitansPresenter>();
        }
    }
}
