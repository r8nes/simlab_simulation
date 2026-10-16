using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;

namespace Godland.Game.World.Tiles
{
    public sealed class WildLand
    {
        private readonly WildLandConfig _config;
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;

        public WildLand(WildLandConfig config, WorldGrid grid, WorldTiles tiles, Settlements settlements)
        {
            _config = config;
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _tiles.SetSubstitute(Substitute);
        }

        private string Substitute(GridCoord cell, string tileId, string cause) =>
            _config.BuildTile != null && tileId == _config.BuildTile.Id ? Choose(cell, cause ?? string.Empty) : tileId;

        private string Choose(GridCoord cell, string cause)
        {
            foreach (var rule in _config.Rules)
            {
                if (rule.Result == null) continue;
                if (!string.IsNullOrEmpty(rule.CauseContains) && !cause.Contains(rule.CauseContains)) continue;
                if (rule.NeedsSettlement && !_settlements.TryGet(cell, out _)) continue;
                if (rule.Neighbors.Count > 0 && Count(cell, rule) < rule.MinNeighbors) continue;

                return rule.Result.Id;
            }

            return _config.Fallback.Id;
        }

        private int Count(GridCoord cell, WildLandRule rule) =>
            _grid.Neighbors(cell).Count(neighbor => _tiles.TryGet(neighbor, out var id) && rule.Neighbors.Any(tile => tile != null && tile.Id == id));
    }
}
