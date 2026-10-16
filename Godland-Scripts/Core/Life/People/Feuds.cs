using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;

namespace Godland.Core.Life.People
{
    public sealed class Feuds
    {
        private readonly WorldGrid _grid;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly IFeudRules _rules;
        private readonly SeededRandom _random;
        private readonly Dictionary<(GridCoord, GridCoord), int> _tension = new();
        private Func<GridCoord, GridCoord, bool> _truce = (_, _) => false;

        public Feuds(WorldGrid grid, Settlements settlements, Shrines shrines, IFeudRules rules, SeededRandom random)
        {
            _grid = grid;
            _settlements = settlements;
            _shrines = shrines;
            _rules = rules;
            _random = random;
        }

        public event Action<GridCoord, GridCoord, bool> Raided;

        public int TensionOf(GridCoord settlement) =>
            _tension.Where(pair => pair.Key.Item1 == settlement || pair.Key.Item2 == settlement).Select(pair => pair.Value).DefaultIfEmpty(0).Max();

        public bool IsFeuding(GridCoord settlement) => TensionOf(settlement) * 2 >= _rules.RaidTension;

        public void SetTruce(Func<GridCoord, GridCoord, bool> truce) => _truce = truce;

        public void Calm(IEnumerable<GridCoord> settlements)
        {
            var calmed = settlements.ToHashSet();
            foreach (var pair in _tension.Keys.Where(pair => calmed.Contains(pair.Item1) || calmed.Contains(pair.Item2)).ToList())
                _tension.Remove(pair);
        }

        public void Update(int year)
        {
            if (year % _rules.FeudPeriod != 0) return;

            var rivals = RivalPairs().ToHashSet();
            foreach (var pair in _tension.Keys.Where(pair => !rivals.Contains(pair)).ToList())
                _tension.Remove(pair);

            foreach (var pair in rivals)
            {
                var tension = _tension.GetValueOrDefault(pair) + 1;
                if (tension < _rules.RaidTension)
                {
                    _tension[pair] = tension;
                    continue;
                }

                _tension.Remove(pair);
                Raid(pair.Item1, pair.Item2);
            }
        }

        private IEnumerable<(GridCoord, GridCoord)> RivalPairs()
        {
            foreach (var settlement in _settlements.All.Keys)
            {
                if (!_shrines.TryGet(settlement, out var faith)) continue;

                foreach (var neighbor in _grid.Neighbors(settlement))
                {
                    if (Order(settlement, neighbor) != (settlement, neighbor) || _truce(settlement, neighbor)) continue;
                    if (_shrines.TryGet(neighbor, out var other) && other != faith) yield return (settlement, neighbor);
                }
            }
        }

        private static (GridCoord, GridCoord) Order(GridCoord a, GridCoord b) =>
            a.X < b.X || (a.X == b.X && a.Y < b.Y) ? (a, b) : (b, a);

        private void Raid(GridCoord a, GridCoord b)
        {
            _settlements.TryGet(a, out var populationA);
            _settlements.TryGet(b, out var populationB);
            var aAttacks = populationA > populationB || (populationA == populationB && _random.Next(2) == 0);
            var attacker = aAttacks ? a : b;
            var defender = aAttacks ? b : a;

            _settlements.Weaken(defender, _rules.RaidPopulationLoss);
            var conquered = false;
            if (_settlements.TryGet(defender, out _) && _shrines.TryGet(attacker, out var winnerFaith)
                && _random.Next(100) < _rules.ConquestChancePercent)
            {
                _shrines.Impose(defender, winnerFaith);
                conquered = true;
            }

            Raided?.Invoke(attacker, defender, conquered);
        }
    }
}
