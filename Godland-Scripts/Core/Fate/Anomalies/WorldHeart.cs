using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Fate.Anomalies
{
    public sealed class WorldHeart
    {
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly CellFertility _fertility;
        private readonly TileGuards _guards;
        private readonly SeededRandom _random;
        private IReadOnlyList<string> _rebirths = Array.Empty<string>();
        private int _period;
        private int _radius;

        public WorldHeart(WorldTiles tiles, Settlements settlements, CellFertility fertility, TileGuards guards, SeededRandom random)
        {
            _tiles = tiles;
            _settlements = settlements;
            _fertility = fertility;
            _guards = guards;
            _random = random;

            guards.Add(cell => IsBeating && cell == Heart);
            _tiles.Cleared += OnCleared;
        }

        public event Action<int> Beat;
        public event Action Died;

        public bool IsBeating { get; private set; }
        public GridCoord Heart { get; private set; }

        public void Awaken(GridCoord heart, string heartTileId, int period, int radius, IReadOnlyList<string> rebirths)
        {
            Heart = heart;
            _period = period;
            _radius = radius;
            _rebirths = rebirths;
            _tiles.Place(heart, heartTileId, "забилось сердце мира");
            IsBeating = true;
        }

        public void Update(int year)
        {
            if (!IsBeating || year % _period != 0 || _rebirths.Count == 0) return;

            var reborn = 0;
            foreach (var cell in _tiles.All.Keys.ToList())
            {
                if (cell == Heart || Math.Max(Math.Abs(cell.X - Heart.X), Math.Abs(cell.Y - Heart.Y)) > _radius) continue;
                if (_guards.IsProtected(cell) || _random.Next(2) != 0) continue;

                _tiles.Place(cell, _rebirths[_random.Next(_rebirths.Count)], "удар сердца мира");
                reborn++;
            }

            Beat?.Invoke(reborn);
        }

        private void OnCleared(GridCoord coord)
        {
            if (!IsBeating || coord != Heart) return;

            IsBeating = false;
            foreach (var (settlement, population) in _settlements.All.ToList())
                _settlements.Weaken(settlement, population / 2);
            foreach (var cell in _tiles.All.Keys.ToList())
                _fertility.Add(cell, -_fertility.Max);
            Died?.Invoke();
        }
    }
}
