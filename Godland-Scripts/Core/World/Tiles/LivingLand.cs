using System;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;

namespace Godland.Core.World.Tiles
{
    public sealed class LivingLand
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly TileGuards _guards;
        private readonly SeededRandom _random;
        private int _period;
        private int _spreadsPerPeriod;

        public LivingLand(
            WorldGrid grid, WorldTiles tiles, CellFertility fertility, Settlements settlements, Shrines shrines, TileGuards guards,
            SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _fertility = fertility;
            _settlements = settlements;
            _shrines = shrines;
            _guards = guards;
            _random = random;
        }

        public event Action<GridCoord, GridCoord, string> Spread;

        public bool IsAwake => _period > 0;

        public void Awaken(int period, int spreadsPerPeriod)
        {
            _period = period;
            _spreadsPerPeriod = spreadsPerPeriod;
        }

        public void Update(int year)
        {
            if (!IsAwake || year % _period != 0) return;

            var spreads = 0;
            foreach (var (source, tileId) in _tiles.All.ToList().OrderBy(_ => _random.Next(10000)))
            {
                if (spreads >= _spreadsPerPeriod) return;
                if (!_tiles.TryGet(source, out var current) || current != tileId) continue;

                var weaker = _grid.Neighbors(source)
                    .Where(cell => _tiles.TryGet(cell, out var id) && id != tileId && CanOvergrow(cell)
                        && _fertility.Get(cell) < _fertility.Get(source))
                    .ToList();
                if (weaker.Count == 0) continue;

                var target = weaker[_random.Next(weaker.Count)];
                _tiles.Place(target, tileId, "живая земля");
                Spread?.Invoke(source, target, tileId);
                spreads++;
            }
        }

        private bool CanOvergrow(GridCoord cell) =>
            !_settlements.TryGet(cell, out _) && !_shrines.IsShrine(cell) && !_guards.IsProtected(cell) && cell != _grid.Center;
    }
}
