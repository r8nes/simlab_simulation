using System;
using System.Linq;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Powers.Babel
{
    public sealed class TowerOfBabel
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly Urbanization _cities;
        private readonly WorldModifiers _modifiers;
        private readonly IBabelRules _rules;
        private readonly SeededRandom _random;
        private bool _happened;
        private int _year;

        public TowerOfBabel(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, Urbanization cities, WorldModifiers modifiers,
            TileGuards guards, IBabelRules rules, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _cities = cities;
            _modifiers = modifiers;
            _rules = rules;
            _random = random;

            guards.Add(cell => IsRising && cell == Site);
            _tiles.Cleared += OnCleared;
        }

        public event Action<GridCoord> Founded;
        public event Action<int> FloorBuilt;
        public event Action<bool> Completed;
        public event Action Toppled;

        public bool IsRising { get; private set; }
        public GridCoord Site { get; private set; }
        public GridCoord City { get; private set; }
        public int Floors { get; private set; }
        public int MaxFloors => _rules.Floors;
        public bool Blessed { get; private set; }

        public void Update(int year)
        {
            _year = year;
            if (IsRising)
            {
                if (year % _rules.FloorPeriod == 0) Build();
                return;
            }

            if (!_happened && year >= _rules.StartYear && year % _rules.CheckPeriod == 0 && _random.Next(100) < _rules.ChancePercent)
                TryFound();
        }

        public void Bless()
        {
            if (!IsRising) return;

            Blessed = true;
            AddFloor();
        }

        public void Topple()
        {
            if (!IsRising) return;

            IsRising = false;
            _tiles.Place(Site, _rules.RuinTileId, "башня низвергнута");
            if (_settlements.TryGet(City, out var population)) _settlements.Weaken(City, population / 2);
            foreach (var settlement in _settlements.All.Keys.ToList())
                if (_shrines.TryGet(settlement, out _) && _random.Next(100) < _rules.ToppleRenouncePercent)
                    _shrines.Renounce(settlement);
            Toppled?.Invoke();
        }

        private void TryFound()
        {
            if (_cities.All.Count < _rules.MinCities) return;

            var city = _cities.All.OrderByDescending(PopulationOf).First();
            if (PopulationOf(city) < _rules.MinCityPopulation) return;

            var sites = _grid.Neighbors(city)
                .Where(cell => _tiles.TryGet(cell, out _) && !_settlements.TryGet(cell, out _) && !_shrines.IsShrine(cell) && cell != _grid.Center)
                .ToList();
            if (sites.Count == 0) return;

            _happened = true;
            City = city;
            Site = sites[_random.Next(sites.Count)];
            Floors = 0;
            _tiles.Place(Site, _rules.TowerTileId, "заложена Вавилонская башня");
            IsRising = true;
            Founded?.Invoke(Site);
        }

        private int PopulationOf(GridCoord coord) => _settlements.TryGet(coord, out var population) ? population : 0;

        private void Build()
        {
            var worker = _grid.Neighbors(Site).Append(City)
                .Where(cell => _settlements.TryGet(cell, out var population) && population >= 2)
                .OrderByDescending(cell => _settlements.All[cell])
                .Cast<GridCoord?>()
                .FirstOrDefault();
            if (worker is not { } builder) return;

            _settlements.Weaken(builder, 1);
            AddFloor();
        }

        private void AddFloor()
        {
            Floors++;
            FloorBuilt?.Invoke(Floors);
            if (Floors >= _rules.Floors) Complete();
        }

        private void Complete()
        {
            IsRising = false;
            if (Blessed)
            {
                if (_shrines.TryGet(City, out var faith))
                    foreach (var settlement in _settlements.All.Keys.ToList())
                        _shrines.Impose(settlement, faith, BeliefChange.Calling);
                _cities.BoostFaith(_rules.ChildrenFaithMultiplier);
            }
            else
            {
                _modifiers.BlockMiracles(_year + _rules.DefianceYears);
            }

            Completed?.Invoke(Blessed);
        }

        private void OnCleared(GridCoord coord)
        {
            if (!IsRising || coord != Site) return;

            IsRising = false;
            Toppled?.Invoke();
        }
    }
}
