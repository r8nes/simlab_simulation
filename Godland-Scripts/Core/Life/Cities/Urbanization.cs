using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Cities
{
    public sealed class Urbanization
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly WorldEpoch _epoch;
        private readonly TileGuards _guards;
        private readonly ICityRules _rules;
        private readonly SeededRandom _random;
        private readonly HashSet<GridCoord> _cities = new();
        private readonly Dictionary<GridCoord, int> _foundedYear = new();
        private int _year;
        private int? _eraStart;
        private int _faithBoost = 1;

        public Urbanization(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, WorldEpoch epoch, TileGuards guards,
            ICityRules rules, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _epoch = epoch;
            _guards = guards;
            _rules = rules;
            _random = random;

            _settlements.Founded += OnFounded;
            _settlements.Abandoned += OnAbandoned;
            _settlements.PopulationChanged += OnPopulationChanged;
            _settlements.SetSturdy(_cities.Contains);
            _settlements.SetFaithMultiplier(coord => _cities.Contains(coord) ? _rules.CityFaithMultiplier * _faithBoost : 1);
        }

        public event Action<GridCoord> Risen;
        public event Action<GridCoord> Fallen;
        public event Action<GridCoord, GridCoord, string> Built;

        public IReadOnlyCollection<GridCoord> All => _cities;

        public bool IsCity(GridCoord coord) => _cities.Contains(coord);

        public void BoostFaith(int factor) => _faithBoost *= factor;

        public bool AreCitiesForbidden { get; private set; }

        public void ForbidCities() => AreCitiesForbidden = true;

        public void Update(int year)
        {
            _year = year;
            if (_epoch.Index < _rules.CitiesFromEpoch) return;

            _eraStart ??= year;
            if (year - _eraStart < _rules.CityDelayYears) return;

            if (year % _rules.RisePeriod == 0 && !AreCitiesForbidden) Rise();
            if (year % _rules.BuildPeriod == 0) Build();
        }

        private void Rise()
        {
            foreach (var (coord, population) in _settlements.All.ToList())
            {
                if (_cities.Contains(coord) || population < _rules.CityPopulation) continue;
                if (_year - _foundedYear.GetValueOrDefault(coord, _year) < _rules.CityMinAge) continue;
                if (_grid.Neighbors(coord).Count(cell => _settlements.TryGet(cell, out _)) < _rules.CityMinNeighbors) continue;

                _cities.Add(coord);
                Risen?.Invoke(coord);
            }
        }

        private void Build()
        {
            foreach (var city in _cities.ToList())
            {
                var candidates = _grid.Neighbors(city).Where(CanBuildOn).ToList();
                if (candidates.Count == 0) continue;

                var target = candidates[_random.Next(candidates.Count)];
                _tiles.TryGet(target, out var tileId);
                var result = _rules.BuildResult(tileId);
                _tiles.Place(target, result, "город застроил");
                Built?.Invoke(city, target, result);
            }
        }

        private bool CanBuildOn(GridCoord cell) =>
            _tiles.TryGet(cell, out var tileId) && _rules.BuildResult(tileId) != null
            && !_guards.IsProtected(cell) && !_shrines.IsShrine(cell);

        private void OnFounded(GridCoord coord) => _foundedYear[coord] = _year;

        private void OnAbandoned(GridCoord coord)
        {
            _foundedYear.Remove(coord);
            _cities.Remove(coord);
        }

        private void OnPopulationChanged(GridCoord coord, int previous, int population)
        {
            if (population >= _rules.CityFallPopulation || !_cities.Remove(coord)) return;

            Fallen?.Invoke(coord);
        }
    }
}
