using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.Writer.Run
{
    public sealed class RunStarter : IStartable
    {
        private readonly RunConfig _config;
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;

        public RunStarter(RunConfig config, WorldGrid grid, WorldTiles tiles)
        {
            _config = config;
            _grid = grid;
            _tiles = tiles;
        }

        public void Start() => _tiles.Place(_grid.Center, _config.StartingTile.Id, "начало мира");
    }
}
