using Godland.Core.World.Tiles;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.History
{
    public sealed class HistoryInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<TileNotesPresenter>();
            builder.RegisterEntryPoint<ChronicleRecorder>();
            builder.RegisterComponentInHierarchy<ChronicleView>();
            builder.RegisterEntryPoint<ChroniclePresenter>();
        }
    }
}
