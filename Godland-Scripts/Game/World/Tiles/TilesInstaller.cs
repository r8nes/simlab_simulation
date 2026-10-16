using Godland.Core.World.Tiles;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TilesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TileSelection>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<TilesView>();
            builder.RegisterComponentInHierarchy<TileBrushView>();
            builder.RegisterComponentInHierarchy<TileInfoView>();

            builder.RegisterEntryPoint<TilesPresenter>().AsSelf();
            builder.RegisterEntryPoint<TilePulsePresenter>();
            builder.RegisterEntryPoint<TileBrushHotkeys>();
            builder.RegisterEntryPoint<TileBrushPresenter>();
            builder.RegisterEntryPoint<TileInfoPresenter>();
            builder.RegisterEntryPoint<TilePlacementController>();
        }
    }
}
