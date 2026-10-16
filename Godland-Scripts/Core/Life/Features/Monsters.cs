using System;
using System.Linq;
using Godland.Core.World.Corruption;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Features
{
    public sealed class Monsters
    {
        private readonly WorldGrid _grid;
        private readonly TileFeatures _features;
        private readonly FeatureDepletion _depletion;
        private readonly IFeatureRules _rules;
        private readonly Settlements _settlements;
        private readonly CorruptionSpread _corruption;
        private readonly SeededRandom _random;
        private readonly WorldModifiers _modifiers;
        private bool _night;

        public Monsters(
            WorldGrid grid, TileFeatures features, FeatureDepletion depletion, IFeatureRules rules,
            Settlements settlements, CorruptionSpread corruption, SeededRandom random, WorldModifiers modifiers)
        {
            _grid = grid;
            _features = features;
            _depletion = depletion;
            _rules = rules;
            _settlements = settlements;
            _corruption = corruption;
            _random = random;
            _modifiers = modifiers;
        }

        public event Action<GridCoord, GridCoord, string> Attacked;

        private Func<GridCoord, bool> _warded = _ => false;

        public void SetWard(Func<GridCoord, bool> warded) => _warded = warded;
        public event Action<GridCoord, string, string> Mutated;

        public bool IsMonster(GridCoord coord) => _features.TryGet(coord, out var id) && IsMonster(id);

        public void Update(int year)
        {
            if (year % _rules.MonsterPeriod != 0) return;

            _night = _modifiers.IsNight(year);

            foreach (var (coord, featureId) in _features.All.ToList())
            {
                if (!_features.TryGet(coord, out var current) || current != featureId) continue;

                if (IsMonster(featureId)) Rampage(coord, featureId);
                else if (_corruption.IsTainted(coord) || _grid.Neighbors(coord).Any(_corruption.IsTainted)) Mutate(coord, featureId);
            }
        }

        private bool IsMonster(string featureId) => _rules.MonsterAttack(featureId) > 0 || _rules.ContagionPercent(featureId) > 0;

        private void Rampage(GridCoord monster, string monsterId)
        {
            if (_depletion.IsDepleted(monster)) return;

            var attack = _rules.MonsterAttack(monsterId) * (_night ? 2 : 1);
            var contagion = _rules.ContagionPercent(monsterId);
            foreach (var neighbor in _grid.Neighbors(monster))
            {
                if (attack > 0 && _settlements.TryGet(neighbor, out _) && !_warded(neighbor) && _random.Next(100) < _rules.MonsterAttackChancePercent)
                {
                    _settlements.Weaken(neighbor, attack);
                    Attacked?.Invoke(monster, neighbor, monsterId);
                }

                if (contagion > 0 && _features.TryGet(neighbor, out var victimId) && !IsMonster(victimId) && _random.Next(100) < contagion)
                    Mutate(neighbor, victimId);
            }
        }

        private void Mutate(GridCoord coord, string featureId)
        {
            var mutant = _rules.MutantOf(featureId);
            if (mutant == null) return;

            _features.Replace(coord, mutant, _features.IsWild(coord));
            Mutated?.Invoke(coord, featureId, mutant);
        }
    }
}
