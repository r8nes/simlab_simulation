using System;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Evolution
{
    [Serializable]
    public sealed class NeighborRequirement
    {
        [SerializeField, Required, AssetsOnly, HorizontalGroup, HideLabel] private TileDefinition _tile;
        [SerializeField, MinValue(1), HorizontalGroup(60), HideLabel, SuffixLabel("×")] private int _count = 1;

        public NeighborRequirement(TileDefinition tile, int count)
        {
            _tile = tile;
            _count = count;
        }

        public TileDefinition Tile => _tile;
        public int Count => _count;
    }
}
