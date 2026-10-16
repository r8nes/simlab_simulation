using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class TransformRandomTileEffect : IWorldEventEffect
    {
        [SerializeField, AssetsOnly] private List<TileDefinition> _from = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _to;

        public bool CanApply(WorldEventContext context) => Targets(context).Count > 0;

        public void Apply(WorldEventContext context) => context.Tiles.Place(context.Pick(Targets(context)), _to.Id, "событие мира");

        private IReadOnlyList<GridCoord> Targets(WorldEventContext context) =>
            context.Tiles.All.Where(pair => _from.Any(tile => tile.Id == pair.Value)).Select(pair => pair.Key).ToList();
    }
}
