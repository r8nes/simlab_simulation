using System;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Cities
{
    [Serializable]
    public sealed class CityBuild
    {
        [field: SerializeField, Required, AssetsOnly, HorizontalGroup]
        public TileDefinition From { get; private set; }

        [field: SerializeField, Required, AssetsOnly, HorizontalGroup]
        public TileDefinition To { get; private set; }
    }
}
