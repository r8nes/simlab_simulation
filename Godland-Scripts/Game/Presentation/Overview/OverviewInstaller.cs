using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Overview
{
    public sealed class OverviewInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<OverviewView>();
            builder.RegisterEntryPoint<OverviewPresenter>();
        }
    }
}
