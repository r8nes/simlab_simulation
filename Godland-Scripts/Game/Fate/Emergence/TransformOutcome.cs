using System;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class TransformOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;

        public void Apply(EmergenceContext context, GridCoord site, string title) => context.Tiles.Place(site, _tile.Id, title);
    }
}
