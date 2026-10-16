using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Powers.Gods
{
    public sealed class Temples
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly Urbanization _cities;
        private readonly Pantheon _pantheon;
        private readonly ITempleRules _rules;
        private readonly SeededRandom _random;
        private readonly Dictionary<GridCoord, (God God, GridCoord City)> _temples = new();
        private readonly Dictionary<God, int> _neglect = new();
        private readonly HashSet<God> _honored = new();

        public Temples(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, Urbanization cities, Pantheon pantheon,
            ITempleRules rules, SeededRandom random, TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _cities = cities;
            _pantheon = pantheon;
            _rules = rules;
            _random = random;

            _tiles.Cleared += Desecrate;
            _pantheon.Fallen += OnGodFallen;
            _pantheon.Died += OnGodFallen;
            _pantheon.SetPower(CountOf);
            guards.Add(_temples.ContainsKey);
        }

        public event Action<God, GridCoord> Built;
        public event Action<God, GridCoord> Lost;
        public event Action<God, GridCoord> Desecrated;

        public bool TryGetAt(GridCoord cell, out God god)
        {
            var found = _temples.TryGetValue(cell, out var temple);
            god = found ? temple.God : null;
            return found;
        }

        public int CountOf(God god) => _temples.Values.Count(temple => temple.God == god);

        public void Update(int year)
        {
            if (year % _rules.TemplePeriod != 0) return;

            Abandon();
            Build();
            Neglect();
        }

        public void Desecrate(GridCoord cell)
        {
            if (!_temples.Remove(cell, out var temple)) return;

            _settlements.Weaken(temple.City, _rules.TempleDesecrationLoss);
            Desecrated?.Invoke(temple.God, cell);
        }

        private void Abandon()
        {
            foreach (var (cell, temple) in _temples.ToList())
            {
                if (_cities.IsCity(temple.City) && _shrines.TryGet(temple.City, out var faith) && faith == temple.God.Religion) continue;

                _temples.Remove(cell);
                Lost?.Invoke(temple.God, cell);
            }
        }

        private void Build()
        {
            foreach (var city in _cities.All.ToList())
            {
                if (_temples.Values.Any(temple => temple.City == city) || !_shrines.TryGet(city, out var faith)) continue;

                var god = _pantheon.Gods.FirstOrDefault(candidate => candidate.Religion == faith);
                if (god == null || CountOf(god) >= _rules.MaxTemplesPerGod) continue;

                var sites = _grid.Neighbors(city)
                    .Where(cell => _tiles.TryGet(cell, out var id) && god.Rule.DomainTileIds.Contains(id)
                        && !_temples.ContainsKey(cell) && !_pantheon.TryGetAt(cell, out _))
                    .ToList();
                if (sites.Count == 0) continue;

                var site = sites[_random.Next(sites.Count)];
                _temples[site] = (god, city);
                _honored.Add(god);
                Built?.Invoke(god, site);
            }
        }

        private void Neglect()
        {
            if (_cities.All.Count == 0) return;

            foreach (var god in _pantheon.Gods.ToList())
            {
                if (CountOf(god) > 0 || !_honored.Contains(god) || god.PantheonId != null)
                {
                    _neglect.Remove(god);
                    continue;
                }

                var neglect = _neglect.GetValueOrDefault(god) + 1;
                _neglect[god] = neglect;
                if (neglect < _rules.NeglectLimit) continue;

                _neglect.Remove(god);
                _honored.Remove(god);
                _pantheon.Forget(god);
            }
        }

        private void OnGodFallen(God god)
        {
            _neglect.Remove(god);
            _honored.Remove(god);
            foreach (var cell in _temples.Where(pair => pair.Value.God == god).Select(pair => pair.Key).ToList())
                _temples.Remove(cell);
        }
    }
}
