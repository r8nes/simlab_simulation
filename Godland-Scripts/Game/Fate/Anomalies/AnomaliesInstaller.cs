using Godland.Core;
using Godland.Core.Fate.Anomalies;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class AnomaliesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GhostArchive>().As<IPastWorld>();
            builder.RegisterEntryPoint<AnomalyPresenter>();
        }
    }
}
