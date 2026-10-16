using System.Collections.Generic;
using System.Linq;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [CreateAssetMenu(menuName = "Godland/Tile Action", fileName = "TileAction")]
    public sealed class TileActionDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField, MinValue(0)]
        public int Cost { get; private set; } = 2;

        [field: SerializeField, MinValue(0)]
        public int FaithCost { get; private set; }

        [field: SerializeField]
        public bool OncePerTile { get; private set; } = true;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Великие чудеса: после применения недоступно столько лет.")]
        public int CooldownYears { get; private set; }

        [SerializeField, AssetsOnly, InfoBox("Пусто — применимо к любому тайлу.")]
        private List<TileDefinition> _applicableTiles = new();

        [SerializeReference, Required]
        private List<ITileEffect> _effects = new();

        public IReadOnlyList<ITileEffect> Effects => _effects;

        public bool AppliesTo(string tileId) => _applicableTiles.Count == 0 || _applicableTiles.Any(tile => tile.Id == tileId);

        public string CostText(int faithCost) => faithCost > 0
            ? Cost > 0 ? $"{Cost} + {faithCost} веры" : $"{faithCost} веры"
            : Cost.ToString();

        public string Describe() => string.Join(", ", _effects.Select(effect => effect.Describe()));

        public string DescribeAt(Godland.Core.World.Grid.GridCoord target, TileEffectContext context) =>
            string.Join(", ", _effects.Select(effect => effect is ITargetedDescription targeted ? targeted.DescribeAt(target, context) : effect.Describe()));
    }
}
