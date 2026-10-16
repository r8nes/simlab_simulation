using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;

namespace Godland.Core.Life.People
{
    public sealed class Godfighters
    {
        private readonly WorldGrid _grid;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly IGodfighterRules _rules;
        private readonly SeededRandom _random;
        private readonly WorldModifiers _modifiers;
        private readonly HashSet<GridCoord> _rebels = new();
        private readonly Dictionary<GridCoord, int> _ire = new();

        public Godfighters(WorldGrid grid, Settlements settlements, Shrines shrines, IGodfighterRules rules, SeededRandom random,
            WorldModifiers modifiers)
        {
            _grid = grid;
            _settlements = settlements;
            _shrines = shrines;
            _rules = rules;
            _random = random;
            _modifiers = modifiers;

            _settlements.Abandoned += Forget;
            _settlements.SetFaithless(IsRebel);
            _shrines.SetFaithBlock(IsRebel);
        }

        public event Action<GridCoord> Rebelled;
        public event Action<GridCoord, GridCoord> ShrineDestroyed;

        public bool IsRebel(GridCoord settlement) => _rebels.Contains(settlement);

        public int IreOf(GridCoord settlement) => _ire.GetValueOrDefault(settlement);

        public void Witness(GridCoord settlement)
        {
            if (_rebels.Contains(settlement) || !_settlements.TryGet(settlement, out _)) return;

            var ire = _ire.GetValueOrDefault(settlement) + 1;
            _ire[settlement] = ire;
            if (ire >= _rules.IreThreshold + _modifiers.IreThresholdBonus && _random.Next(100) < _rules.RevoltChancePercent) Rebel(settlement);
        }

        public void Update(int year)
        {
            if (year % _rules.IreDecayPeriod == 0)
                foreach (var settlement in _ire.Keys.ToList())
                    if (--_ire[settlement] <= 0) _ire.Remove(settlement);

            if (year % _rules.GodfighterPeriod != 0) return;

            foreach (var rebel in _rebels.ToList())
            {
                foreach (var neighbor in _grid.Neighbors(rebel))
                {
                    if (_shrines.IsShrine(neighbor) && _random.Next(100) < _rules.GodfighterSpreadChancePercent)
                    {
                        _shrines.DesecrateShrine(neighbor);
                        ShrineDestroyed?.Invoke(rebel, neighbor);
                    }

                    if (_settlements.TryGet(neighbor, out _) && !_rebels.Contains(neighbor)
                        && _random.Next(100) < _rules.GodfighterSpreadChancePercent)
                        Rebel(neighbor);
                }
            }
        }

        private void Rebel(GridCoord settlement)
        {
            _rebels.Add(settlement);
            _ire.Remove(settlement);
            _shrines.Renounce(settlement);
            Rebelled?.Invoke(settlement);
        }

        private void Forget(GridCoord settlement)
        {
            _rebels.Remove(settlement);
            _ire.Remove(settlement);
        }
    }
}
