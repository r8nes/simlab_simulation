using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Features
{
    [CreateAssetMenu(menuName = "Godland/Tile Feature", fileName = "Feature")]
    public sealed class FeatureDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField, MinValue(0)]
        public int Cost { get; private set; } = 3;

        [field: SerializeField, Required, AssetsOnly]
        public TileView Prefab { get; private set; }

        [SerializeField, AssetsOnly, InfoBox("На какие тайлы можно поставить. Пусто — ни на какие (например, мутанты).")]
        private List<TileDefinition> _applicableTiles = new();

        [field: SerializeField, BoxGroup("Rules")]
        public int NeighborFertility { get; private set; }

        [field: SerializeField, BoxGroup("Rules")]
        public int SelfFertility { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(0), SuffixLabel("years"), InfoBox("Насколько дольше хищники охотятся на соседних жертв.")]
        public int ShelterYears { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), InfoBox("Еда для людей: рядом с ней могут появиться и расти поселения.")]
        public bool IsFood { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(0)]
        public int CreationYield { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(1), SuffixLabel("years"), ShowIf(nameof(HasCreationYield))]
        public int CreationYieldPeriod { get; private set; } = 50;

        [SerializeField, BoxGroup("Evolution"), InfoBox("Во что обитатель может эволюционировать сам. Условия проверяются, затем бросается скрытый кубик.")]
        private List<FeatureEvolutionDefinition> _evolutions = new();

        public IReadOnlyList<FeatureEvolutionRule> EvolutionRules => _evolutions.Select(evolution => evolution.ToCore()).ToList();

        [SerializeField, BoxGroup("Mutation"), AssetsOnly, InfoBox("Во что этот зверь мутирует (от порчи или заразы чудовищ). Пусто — не мутирует.")]
        private FeatureDefinition _mutant;

        [field: SerializeField, BoxGroup("Mutation"), MinValue(0), InfoBox("Чудовище: сколько жителей теряют соседние поселения раз в период чудовищ.")]
        public int MonsterAttack { get; private set; }

        [field: SerializeField, BoxGroup("Mutation"), Range(0, 100), SuffixLabel("%"), InfoBox("Шанс за период заразить мутацией соседнего зверя.")]
        public int ContagionPercent { get; private set; }

        public FeatureDefinition Mutant => _mutant;

        public bool IsMonster => MonsterAttack > 0 || ContagionPercent > 0;

        public bool HasCreationYield => CreationYield > 0;

        public bool OnlyFeedsItsTile =>
            SelfFertility > 0 && NeighborFertility == 0 && !IsFood && !HasCreationYield && ShelterYears == 0 && _evolutions.Count == 0;

        public bool AppliesTo(string tileId) => _applicableTiles.Any(tile => tile.Id == tileId);

        public string Describe()
        {
            var parts = new List<string>();
            if (SelfFertility > 0) parts.Add($"тайлу +{SelfFertility} плодородия в год");
            if (NeighborFertility > 0) parts.Add($"соседям +{NeighborFertility} плодородия в год");
            if (NeighborFertility < 0) parts.Add($"отравляет соседей: {NeighborFertility} плодородия в год");
            if (MonsterAttack > 0) parts.Add($"чудовище: нападает на соседние поселения (−{MonsterAttack})");
            if (ContagionPercent > 0) parts.Add("заражает соседних зверей мутацией");
            if (HasCreationYield) parts.Add($"+{CreationYield} созидания раз в {CreationYieldPeriod} лет");
            if (IsFood) parts.Add("еда для людей");
            if (ShelterYears > 0) parts.Add($"прячет соседних жертв: охота дольше на {ShelterYears} лет");
            return string.Join(", ", parts);
        }
    }
}
