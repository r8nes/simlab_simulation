using System;
using System.Linq;
using Godland.Game.Writer.Gifts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class SplitWorldEffect : IMilestoneEffect
    {
        [SerializeField, Required, InfoBox("Разлом мира: тот же эффект, что у дара, плюс клетки разлома навсегда защищены от мира.")]
        private RiftGift _rift = new();

        [SerializeField, Required, AssetsOnly] private Godland.Game.World.Tiles.TileDefinition _riftTile;

        public void Apply(ProphecyContext context)
        {
            var tiles = context.Tiles;
            var riftId = _riftTile.Id;
            var before = tiles.All.Where(pair => pair.Value == riftId).Select(pair => pair.Key).ToHashSet();
            _rift.Apply(context.Gifts);
            var chasm = tiles.All.Where(pair => pair.Value == riftId && !before.Contains(pair.Key)).Select(pair => pair.Key).ToHashSet();
            context.Guards.Add(chasm.Contains);
        }
    }
}
