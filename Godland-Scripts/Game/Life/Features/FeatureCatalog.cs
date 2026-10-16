using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Features
{
    [CreateAssetMenu(menuName = "Godland/Feature Catalog", fileName = "FeatureCatalog")]
    public sealed class FeatureCatalog : ScriptableObject, IFeatureRules
    {
        [SerializeField, AssetsOnly]
        private List<FeatureDefinition> _features = new();

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Обитатели не погибают: съеденный или добытый истощается на столько лет и потом восстанавливается сам.")]
        public int RecoveryYears { get; private set; } = 40;

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период каждая подсаженная колония выпускает дикого обитателя на соседний подходящий тайл. Дикие не расселяются и не дают бонусов; подсадка их заменяет.")]
        public int SpreadPeriod { get; private set; } = 80;

        [field: SerializeField, MinValue(1), InfoBox("Бюджет энергии зверей: колония каждый год копит энергию от плодородия своей земли и теряет всё, когда её съели. Одно расселение стоит столько; на богатой земле бум (до двух за раз), на бедной никого.")]
        public int SpreadEnergy { get; private set; } = 300;

        [field: SerializeField, MinValue(0), InfoBox("На тайле с плодородием ниже этого обитатели не расселяются и не эволюционируют (тайлы без плодородия не затронуты).")]
        public int MinFertilityToThrive { get; private set; } = 3;

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Как часто чудовища нападают и заражают, а звери у порчи мутируют.")]
        public int MonsterPeriod { get; private set; } = 20;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что чудовище нападёт на соседнее поселение в свой период.")]
        public int MonsterAttackChancePercent { get; private set; } = 50;

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Как часто обитатели бросают кубик эволюции.")]
        public int EvolutionPeriod { get; private set; } = 50;

        [SerializeField, AssetsOnly]
        private List<FeatureInteractionDefinition> _interactions = new();

        public IReadOnlyList<FeatureInteraction> Interactions =>
            _interactions.Select(interaction => interaction.ToCore()).ToList();

        public IReadOnlyList<FeatureDefinition> All => _features;

        public FeatureDefinition Get(string id) =>
            _features.FirstOrDefault(feature => feature.Id == id)
            ?? throw new KeyNotFoundException($"Feature '{id}' is not in the catalog.");

        public FeatureInteractionDefinition GetInteraction(FeatureInteraction interaction) =>
            _interactions.First(definition => definition.Predator.Id == interaction.PredatorId && definition.Prey.Id == interaction.PreyId);

        public int GetNeighborFertility(string featureId) => Get(featureId).NeighborFertility;

        public int GetSelfFertility(string featureId) => Get(featureId).SelfFertility;

        public int GetShelterYears(string featureId) => Get(featureId).ShelterYears;

        public bool IsFood(string featureId) => Get(featureId).IsFood;

        public IReadOnlyList<FeatureEvolutionRule> EvolutionsOf(string featureId) => Get(featureId).EvolutionRules;

        public bool TakesPartInHunts(string featureId) =>
            _interactions.Any(interaction => interaction.Predator.Id == featureId || interaction.Prey.Id == featureId);

        public string MutantOf(string featureId) => Get(featureId).Mutant != null ? Get(featureId).Mutant.Id : null;

        public int MonsterAttack(string featureId) => Get(featureId).MonsterAttack;

        public int ContagionPercent(string featureId) => Get(featureId).ContagionPercent;

        public bool CanLiveOn(string featureId, string tileId) => Get(featureId).AppliesTo(tileId);

        public bool TryGetCreationYield(string featureId, out int amount, out int periodYears)
        {
            var feature = Get(featureId);
            amount = feature.CreationYield;
            periodYears = feature.CreationYieldPeriod;
            return feature.HasCreationYield;
        }
    }
}
