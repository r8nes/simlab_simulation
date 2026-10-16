using System;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class TileShareFall : ISovereignFall
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;
        [SerializeField, Range(0, 100), SuffixLabel("%")] private int _belowPercent = 20;

        public string Describe() => $"«{_tile.DisplayName}» меньше {_belowPercent}% тайлов мира";

        public bool IsOverthrown(SovereignContext context, Religion claimant)
        {
            var tiles = context.Tiles.All;
            return tiles.Count > 0 && tiles.Values.Count(id => id == _tile.Id) * 100 < tiles.Count * _belowPercent;
        }
    }
}
