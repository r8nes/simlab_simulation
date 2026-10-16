using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Fate.Anomalies
{
    public sealed class LandDrift
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileGuards _guards;
        private readonly SeededRandom _random;
        private int _period;
        private int _until;

        public LandDrift(WorldGrid grid, WorldTiles tiles, TileGuards guards, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _guards = guards;
            _random = random;
        }

        public event Action<int, bool> Drifted;

        public bool IsActive { get; private set; }

        public void Begin(int period, int untilYear)
        {
            _period = period;
            _until = untilYear;
            IsActive = true;
        }

        public void Update(int year)
        {
            if (!IsActive) return;
            if (year >= _until)
            {
                IsActive = false;
                return;
            }

            if (year % _period != 0) return;

            var vertical = _random.Next(2) == 0;
            var lines = _tiles.All.Keys.Select(cell => vertical ? cell.X : cell.Y).Distinct().ToList();
            if (lines.Count == 0) return;

            var line = lines[_random.Next(lines.Count)];
            var cells = _tiles.All.Keys
                .Where(cell => (vertical ? cell.X : cell.Y) == line && cell != _grid.Center && !_guards.IsProtected(cell))
                .OrderBy(cell => vertical ? cell.Y : cell.X)
                .ToList();
            if (cells.Count < 2) return;

            var ids = cells.Select(cell => _tiles.All[cell]).ToList();
            for (var i = 0; i < cells.Count; i++)
            {
                var shifted = ids[(i + ids.Count - 1) % ids.Count];
                if (_tiles.All[cells[i]] != shifted) _tiles.Place(cells[i], shifted, "дрейф земель");
            }

            Drifted?.Invoke(line, vertical);
        }
    }
}
