using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Biomes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Biomes
{
    [CreateAssetMenu(menuName = "Godland/Super Biome Config", fileName = "SuperBiomeConfig")]
    public sealed class SuperBiomeConfig : ScriptableObject, ISuperBiomeRules
    {
        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Какую долю мира должно занять семейство, чтобы наступил супербиом. Если доля упадёт — он пройдёт.")]
        public int DominancePercent { get; private set; } = 70;

        [field: SerializeField, MinValue(1), InfoBox("Супербиом возможен только в мире хотя бы из стольких тайлов.")]
        public int MinTiles { get; private set; } = 9;

        [SerializeField]
        private List<SuperBiomeDefinition> _biomes = new();

        private IReadOnlyList<SuperBiomeRule> _rules;

        public IReadOnlyList<SuperBiomeRule> Biomes => _rules ??= _biomes.Select(biome => biome.ToCore()).ToList();

        public SuperBiomeDefinition DefinitionOf(SuperBiomeRule rule) => _biomes.First(biome => biome.Id == rule.Id);

        private void OnValidate() => _rules = null;
    }
}
