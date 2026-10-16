using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Corruption;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Corruption
{
    [CreateAssetMenu(menuName = "Godland/Corruption Config", fileName = "CorruptionConfig")]
    public sealed class CorruptionConfig : ScriptableObject, ICorruptionRules
    {
        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly, InfoBox("Источник порчи: щедрый, но расползается.")]
        private TileDefinition _source;

        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly]
        private TileDefinition _corrupted;

        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly, InfoBox("Во что заживает порченая земля, когда Источника больше нет.")]
        private TileDefinition _healed;

        [SerializeField, BoxGroup("Tiles"), AssetsOnly, InfoBox("Порча не переходит на эти тайлы: вода сдерживает её.")]
        private List<TileDefinition> _resistant = new();

        [field: SerializeField, BoxGroup("Spread"), MinValue(1), SuffixLabel("years")]
        public int CorruptionPeriod { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Spread"), Range(0, 100), SuffixLabel("%")]
        public int CorruptionChancePercent { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Spread"), MinValue(0), InfoBox("Сколько жителей теряет поселение на порченом тайле за период.")]
        public int CorruptionPopulationLoss { get; private set; } = 1;

        public string CorruptedTileId => _corrupted.Id;

        public string HealedTileId => _healed.Id;

        public bool IsSource(string tileId) => _source.Id == tileId;

        public bool ResistsCorruption(string tileId) => _resistant.Any(tile => tile.Id == tileId);
    }
}
