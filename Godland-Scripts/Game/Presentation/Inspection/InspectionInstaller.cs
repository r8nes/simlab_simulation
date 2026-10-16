using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Inspection
{
    public sealed class InspectionInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TileInspection>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<InspectToggleView>();
            builder.RegisterEntryPoint<InspectionPresenter>();
        }
    }
}
