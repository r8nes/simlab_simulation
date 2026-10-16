using System;
using System.Collections.Generic;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Creation
{
    public sealed class ProsperityRewards
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly CreationPoints _creation;
        private readonly int _maxFertilityReward;
        private readonly HashSet<GridCoord> _rewarded = new();

        public ProsperityRewards(WorldGrid grid, WorldTiles tiles, CellFertility fertility, CreationPoints creation, int maxFertilityReward)
        {
            _grid = grid;
            _tiles = tiles;
            _fertility = fertility;
            _creation = creation;
            _maxFertilityReward = maxFertilityReward;

            _tiles.Cleared += coord => _rewarded.Remove(coord);
        }

        public event Action<GridCoord, int> Rewarded;

        public void Collect()
        {
            foreach (var coord in _grid.Coords)
            {
                if (_rewarded.Contains(coord) || !_tiles.TryGet(coord, out _)) continue;
                if (_fertility.Get(coord) < _fertility.Max) continue;

                _rewarded.Add(coord);
                _creation.Add(_maxFertilityReward);
                Rewarded?.Invoke(coord, _maxFertilityReward);
            }
        }
    }
}
