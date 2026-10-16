using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.Fate.Emergence;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Features
{
    public enum CreatureGene
    {
        Size,
        Fecundity,
        Ferocity,
        Hardiness
    }

    public sealed class Creature
    {
        private readonly int[] _genes;

        public Creature(int[] genes, CreatureGene? lineage)
        {
            _genes = genes;
            Lineage = lineage;
        }

        public CreatureGene? Lineage { get; set; }

        public int this[CreatureGene gene]
        {
            get => _genes[(int)gene];
            set => _genes[(int)gene] = Math.Clamp(value, 0, CreatureGenes.MaxGene);
        }

        public Creature Copy() => new((int[])_genes.Clone(), Lineage);

        public CreatureGene Dominant => Enum.GetValues(typeof(CreatureGene)).Cast<CreatureGene>().OrderByDescending(gene => this[gene]).First();
    }

    public sealed class CreatureGenes : IWorldStep, IDisposable
    {
        public const int MaxGene = 10;
        private const int BaseGene = 3;
        private const int Strong = 7;
        private const int SpeciesGene = 8;
        private const int MutationPeriod = 50;
        private const int MaulPeriod = 30;
        private const int MaulChancePercent = 20;

        private readonly Dictionary<GridCoord, Creature> _creatures = new();
        private readonly WorldGrid _grid;
        private readonly TileFeatures _features;
        private readonly FeatureSpread _spread;
        private readonly FeatureDepletion _depletion;
        private readonly Settlements _settlements;
        private readonly CellFertility _fertility;
        private readonly WorldMemory _memory;
        private readonly SeededRandom _random;

        public CreatureGenes(
            WorldGrid grid, TileFeatures features, FeatureSpread spread, FeatureDepletion depletion, Settlements settlements, CellFertility fertility,
            WorldMemory memory, SeededRandom random)
        {
            _grid = grid;
            _features = features;
            _spread = spread;
            _depletion = depletion;
            _settlements = settlements;
            _fertility = fertility;
            _memory = memory;
            _random = random;

            _features.Added += OnAdded;
            _features.Removed += OnRemoved;
            _spread.Spread += OnSpread;
            _settlements.SetFoodBonus(cell => Has(cell, CreatureGene.Size) ? 1 : 0);
            _spread.SetBreeding(cell => Has(cell, CreatureGene.Fecundity) ? 1 : 0, cell => Has(cell, CreatureGene.Hardiness));
        }

        public event Action<GridCoord, CreatureGene> Speciated;
        public event Action<GridCoord, GridCoord> Mauled;

        public bool TryGet(GridCoord cell, out Creature creature) => _creatures.TryGetValue(cell, out creature);

        public bool Has(GridCoord cell, CreatureGene gene) => _creatures.TryGetValue(cell, out var creature) && creature[gene] >= Strong;

        public void Dispose()
        {
            _features.Added -= OnAdded;
            _features.Removed -= OnRemoved;
            _spread.Spread -= OnSpread;
        }

        public void Advance(int year)
        {
            if (year % MutationPeriod == 0) Mutate();
            if (year % MaulPeriod == 0) Maul();
        }

        private void OnAdded(GridCoord cell, string _)
        {
            if (!_creatures.ContainsKey(cell)) _creatures[cell] = new Creature(Enumerable.Repeat(BaseGene, 4).ToArray(), null);
        }

        private void OnRemoved(GridCoord cell, string _) => _creatures.Remove(cell);

        private void OnSpread(GridCoord colony, GridCoord child, string _)
        {
            if (!_creatures.TryGetValue(colony, out var parent)) return;

            var copy = parent.Copy();
            var gene = (CreatureGene)_random.Next(4);
            copy[gene] += _random.Next(2) == 0 ? 1 : -1;
            _creatures[child] = copy;
        }

        private void Mutate()
        {
            foreach (var (cell, creature) in _creatures.OrderBy(pair => pair.Key.X).ThenBy(pair => pair.Key.Y).ToList())
            {
                if (_depletion.IsDepleted(cell)) continue;

                if (_random.Next(2) == 0) creature[Pressure(cell)] += 1;
                creature[(CreatureGene)_random.Next(4)] -= _random.Next(3) == 0 ? 1 : 0;

                if (creature.Lineage != null) continue;

                var strongest = creature.Dominant;
                if (creature[strongest] < SpeciesGene) continue;

                creature.Lineage = strongest;
                _memory.Add(cell, $"beast:{strongest.ToString().ToLowerInvariant()}");
                Speciated?.Invoke(cell, strongest);
            }
        }

        private CreatureGene Pressure(GridCoord cell)
        {
            var hunted = _grid.Neighbors(cell).Any(_settlements.All.ContainsKey);
            if (hunted && _random.Next(3) > 0) return _random.Next(2) == 0 ? CreatureGene.Fecundity : CreatureGene.Ferocity;
            if (!_fertility.CanHold(cell)) return _random.Next(2) == 0 ? CreatureGene.Hardiness : (CreatureGene)_random.Next(4);
            return _fertility.Get(cell) * 2 >= _fertility.Max ? CreatureGene.Size : CreatureGene.Hardiness;
        }

        private void Maul()
        {
            foreach (var cell in _creatures.Keys.Where(cell => Has(cell, CreatureGene.Ferocity) && !_depletion.IsDepleted(cell))
                         .OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList())
            {
                var prey = _grid.Neighbors(cell).Where(_settlements.All.ContainsKey).OrderBy(other => other.X).ThenBy(other => other.Y).ToList();
                if (prey.Count == 0 || _random.Next(100) >= MaulChancePercent) continue;

                var victim = prey[_random.Next(prey.Count)];
                _settlements.Weaken(victim, 1);
                _memory.Add(victim, "mauled");
                Mauled?.Invoke(cell, victim);
            }
        }
    }
}
