using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Fate.Anomalies
{
    public sealed class SkyFolk
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly SeededRandom _random;
        private int _period;

        public SkyFolk(WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _random = random;
        }

        public event Action<GridCoord> Landed;
        public event Action<GridCoord> Converted;

        public bool HasLanded { get; private set; }
        public GridCoord City { get; private set; }
        public Religion Faith { get; private set; }

        public bool TryLand(string cityTileId, Dogma dogma, int population, int period)
        {
            var landing = _grid.Coords.Where(_grid.IsPurchasable).ToList();
            if (landing.Count == 0) return false;

            City = landing[_random.Next(landing.Count)];
            _period = period;
            _grid.Unlock(City);
            _tiles.Place(City, cityTileId, "спустился Небесный град");
            if (!_settlements.TrySettle(City, population)) return false;

            Faith = _shrines.Ordain(City, dogma, null);
            _shrines.Impose(City, Faith, BeliefChange.Calling);
            HasLanded = true;
            Landed?.Invoke(City);
            return true;
        }

        public void Update(int year)
        {
            if (!HasLanded || year % _period != 0) return;
            if (!_settlements.TryGet(City, out _))
            {
                HasLanded = false;
                return;
            }

            var followers = _shrines.FollowersOf(Faith).ToHashSet();
            var targets = _settlements.All.Keys
                .Where(cell => !followers.Contains(cell) && _grid.Neighbors(cell).Any(followers.Contains))
                .ToList();
            if (targets.Count == 0) return;

            var target = targets[_random.Next(targets.Count)];
            _shrines.Impose(target, Faith, BeliefChange.Calling);
            Converted?.Invoke(target);
        }
    }
}
