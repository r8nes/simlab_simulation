using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.People
{
    [CreateAssetMenu(menuName = "Godland/Dogma", fileName = "Dogma")]
    public sealed class DogmaDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [SerializeField, AssetsOnly, InfoBox("Каким тайлам-богам принадлежит этот догмат. У ересей пусто.")]
        private List<TileDefinition> _gods = new();

        [field: SerializeField, BoxGroup("Rules"), MinValue(0), InfoBox("Дополнительная вера за период с каждого поселения этой веры.")]
        public int FaithBonus { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(0), InfoBox("Плодородие соседям верующих раз в период роста.")]
        public int NeighborFertility { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), InfoBox("Верующие не охотятся.")]
        public bool HuntTaboo { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), InfoBox("Осквернение святыни не убивает жителей.")]
        public bool ResistsDesecration { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(0), SuffixLabel("%"), InfoBox("Прибавка к шансу обратить соседей.")]
        public int ConversionBonusPercent { get; private set; }

        public bool BelongsTo(string tileId) => _gods.Any(tile => tile.Id == tileId);

        public Dogma ToCore() => new(Id, FaithBonus, NeighborFertility, HuntTaboo, ResistsDesecration, ConversionBonusPercent);

        public string Describe()
        {
            var parts = new List<string>();
            if (FaithBonus > 0) parts.Add($"+{FaithBonus} веры");
            if (NeighborFertility > 0) parts.Add($"соседям +{NeighborFertility} плодородия");
            if (HuntTaboo) parts.Add("не охотятся");
            if (ResistsDesecration) parts.Add("стойкие к осквернению");
            if (ConversionBonusPercent > 0) parts.Add($"обращают охотнее (+{ConversionBonusPercent}%)");
            return string.Join(", ", parts);
        }
    }
}
