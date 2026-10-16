using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Progression
{
    public sealed class ProgressionInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ProfileStore>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FlowDirector>().AsSelf();
            builder.RegisterEntryPoint<DiscoveryKeeper>().AsSelf();
            builder.RegisterEntryPoint<DiscoveryPresenter>();
        }
    }
}
