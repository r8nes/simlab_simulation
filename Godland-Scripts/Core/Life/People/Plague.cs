using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.People
{
    public sealed class Plague
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly ISettlementRules _settlementRules;
        private readonly IPlagueRules _rules;
        private readonly SeededRandom _random;
        private readonly Dictionary<GridCoord, int> _sick = new();
        private readonly Dictionary<GridCoord, int> _immuneUntil = new();
        private readonly Dictionary<GridCoord, int> _raidedAt = new();
        private readonly WorldModifiers _modifiers;
        private int _year;

        public Plague(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, ISettlementRules settlementRules, IPlagueRules rules,
            SeededRandom random, Feuds feuds, WorldModifiers modifiers)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _settlementRules = settlementRules;
            _rules = rules;
            _random = random;
            _modifiers = modifiers;

            _settlements.Abandoned += coord =>
            {
                _sick.Remove(coord);
                _immuneUntil.Remove(coord);
                _raidedAt.Remove(coord);
            };
            _settlements.Migrated += (from, to) =>
            {
                if (_sick.ContainsKey(from)) Infect(to, from);
            };
            feuds.Raided += OnRaided;
        }

        public event Action<GridCoord, IReadOnlyList<string>> Outbreak;
        public event Action<GridCoord, GridCoord> Spread;
        public event Action<GridCoord> Recovered;

        public bool IsSick(GridCoord settlement) => _sick.ContainsKey(settlement);

        public int SickCount => _sick.Count;

        public void Update(int year)
        {
            _year = year;
            if (year % _rules.PlagueTickPeriod == 0) Ravage();
            if (year % _rules.PlagueCheckPeriod == 0) RollOutbreaks();
        }

        public IReadOnlyList<string> RiskFactors(GridCoord settlement)
        {
            var factors = new List<string>();
            if (!_settlements.TryGet(settlement, out var population)) return factors;

            if (population >= _settlementRules.VillagePopulation) factors.Add("теснота");
            if (_settlements.IsHungry(settlement)) factors.Add("голод");
            if (_grid.Neighbors(settlement).Append(settlement).Any(cell => _tiles.TryGet(cell, out var id) && _rules.BreedsDisease(id)))
                factors.Add("гнилые земли рядом");
            if (_raidedAt.TryGetValue(settlement, out var raided) && _year - raided <= _rules.RecentRaidYears) factors.Add("война");
            return factors;
        }

        private void RollOutbreaks()
        {
            foreach (var settlement in _settlements.All.Keys.ToList())
            {
                if (_sick.ContainsKey(settlement) || IsImmune(settlement)) continue;

                var factors = RiskFactors(settlement);
                if (factors.Count < _rules.PlagueMinRiskFactors) continue;
                if (_random.Next(100) >= factors.Count * _rules.PlagueChancePerFactorPercent) continue;

                _sick[settlement] = _rules.PlagueDuration;
                Outbreak?.Invoke(settlement, factors);
            }
        }

        private void Ravage()
        {
            foreach (var settlement in _sick.Keys.ToList())
            {
                if (!_sick.ContainsKey(settlement)) continue;

                foreach (var neighbor in _grid.Neighbors(settlement))
                    if (_settlements.TryGet(neighbor, out _) && RiskFactors(neighbor).Count > 0
                        && _random.Next(100) < _rules.PlagueSpreadChancePercent * _modifiers.PlagueSpreadMultiplier)
                        Infect(neighbor, settlement);

                _settlements.Weaken(settlement, 1);
                if (!_sick.ContainsKey(settlement)) continue;

                _sick[settlement] -= _rules.PlagueTickPeriod;
                if (_sick[settlement] > 0) continue;

                _sick.Remove(settlement);
                _immuneUntil[settlement] = _year + _rules.PlagueImmunityYears;
                Recovered?.Invoke(settlement);
            }
        }

        private void Infect(GridCoord settlement, GridCoord source)
        {
            if (_sick.ContainsKey(settlement) || IsImmune(settlement) || !_settlements.TryGet(settlement, out _)) return;

            _sick[settlement] = _rules.PlagueDuration;
            Spread?.Invoke(source, settlement);
        }

        public void Strike(GridCoord settlement) => Infect(settlement, settlement);

        public void GrantImmunity(GridCoord settlement) => _immuneUntil[settlement] = int.MaxValue;

        public bool IsForeverImmune(GridCoord settlement) => _immuneUntil.TryGetValue(settlement, out var until) && until == int.MaxValue;

        private bool IsImmune(GridCoord settlement) => _immuneUntil.TryGetValue(settlement, out var until) && _year < until;

        private void OnRaided(GridCoord attacker, GridCoord defender, bool _)
        {
            _raidedAt[attacker] = _year;
            _raidedAt[defender] = _year;
            if (_sick.ContainsKey(attacker)) Infect(defender, attacker);
            else if (_sick.ContainsKey(defender)) Infect(attacker, defender);
        }
    }
}
