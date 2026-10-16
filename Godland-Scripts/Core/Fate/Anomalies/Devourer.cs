using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Fate.Anomalies
{
    public sealed class Devourer
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly SeededRandom _random;
        private int _period;
        private int _feedingsLeft;

        public Devourer(WorldGrid grid, WorldTiles tiles, TileGuards guards, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _random = random;

            guards.Add(cell => IsActive && cell == Maw);
        }

        public event Action<GridCoord> Ate;
        public event Action<int> Fed;
        public event Action Sated;

        public bool IsActive { get; private set; }
        public GridCoord Maw { get; private set; }
        public int FeedingsLeft => _feedingsLeft;

        public void Awaken(GridCoord maw, string abyssTileId, int period, int feedings)
        {
            Maw = maw;
            _period = period;
            _feedingsLeft = feedings;
            _tiles.Place(maw, abyssTileId, "бездна Пожирателя");
            IsActive = true;
        }

        public void Update(int year)
        {
            if (!IsActive || year % _period != 0) return;

            var prey = _tiles.All.Keys
                .Where(cell => cell != Maw && cell != _grid.Center)
                .OrderBy(cell => Math.Abs(cell.X - Maw.X) + Math.Abs(cell.Y - Maw.Y))
                .ThenBy(_ => _random.Next(1000))
                .Cast<GridCoord?>()
                .FirstOrDefault();
            if (prey is not { } meal) return;

            _tiles.Remove(meal, "съедено Пожирателем");
            Ate?.Invoke(meal);
        }

        public void Depart(string cause)
        {
            if (!IsActive) return;

            IsActive = false;
            _feedingsLeft = 0;
            _tiles.Remove(Maw, cause);
            Sated?.Invoke();
        }

        public void Feed()
        {
            if (!IsActive) return;

            _feedingsLeft--;
            Fed?.Invoke(_feedingsLeft);
            if (_feedingsLeft > 0) return;

            IsActive = false;
            _tiles.Remove(Maw, "бездна закрылась");
            Sated?.Invoke();
        }
    }
}
