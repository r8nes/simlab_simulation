using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Godland.Game.Life.People
{
    [CreateAssetMenu(menuName = "Godland/Settlement Culture", fileName = "Culture")]
    public sealed class CultureDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField, InfoBox("Во множественном числе: «земледельцы», «рыбаки».")]
        public string DisplayName { get; private set; }

        [SerializeField, AssetsOnly, InfoBox("На каких тайлах люди живут этой культурой.")]
        private List<TileDefinition> _tiles = new();

        [field: SerializeField, BoxGroup("Rules")]
        public int CapacityBonus { get; private set; }

        [field: SerializeField, FormerlySerializedAs("<FaithPerPerson>k__BackingField"), BoxGroup("Rules"), MinValue(0)]
        public int FaithMultiplier { get; private set; } = 1;

        [field: SerializeField, BoxGroup("Rules"), MinValue(0)]
        public int NeighborFertility { get; private set; }

        [SerializeField, BoxGroup("Rules"), AssetsOnly, InfoBox("Соседние тайлы этих типов тоже считаются едой.")]
        private List<TileDefinition> _foodTiles = new();

        [SerializeField, BoxGroup("Shapes the world"), InfoBox("Раз в период люди превращают один соседний тайл «из» в тайл «в».")]
        private List<CultureTransformDefinition> _transforms = new();

        [SerializeField, BoxGroup("Faith"), AssetsOnly, InfoBox("Соседние тайлы, которые эти люди могут объявить своим богом.")]
        private List<TileDefinition> _sacredTiles = new();

        public bool LivesOn(string tileId) => _tiles.Any(tile => tile.Id == tileId);

        public SettlementCulture ToCore() =>
            new SettlementCulture(
                Id, CapacityBonus, FaithMultiplier, NeighborFertility,
                _foodTiles.Select(tile => tile.Id).ToList(),
                _transforms.Select(transform => transform.ToCore()).ToList(),
                _sacredTiles.Select(tile => tile.Id).ToList());

        public string Describe()
        {
            var parts = new List<string>();
            if (CapacityBonus != 0) parts.Add($"предел населения {CapacityBonus:+#;-#}");
            if (FaithMultiplier != 1) parts.Add($"вера ×{FaithMultiplier}");
            if (NeighborFertility > 0) parts.Add($"соседям +{NeighborFertility} плодородия");
            foreach (var transform in _transforms)
                parts.Add($"{transform.From.DisplayName} → {transform.To.DisplayName} раз в {transform.PeriodYears} лет");
            if (_sacredTiles.Count > 0) parts.Add($"почитают: {string.Join(", ", _sacredTiles.Select(tile => tile.DisplayName))}");
            if (_foodTiles.Count > 0) parts.Add($"еда: {string.Join(", ", _foodTiles.Select(tile => tile.DisplayName))}");
            return string.Join(", ", parts);
        }
    }
}
