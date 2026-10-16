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
    public sealed class GreatSnowfallEffect : IWorldEventEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _snow;
        [SerializeField, AssetsOnly, InfoBox("Тёплые тайлы: их снег не заметает.")] private List<TileDefinition> _warm = new();
        [SerializeField, AssetsOnly, InfoBox("Источники тепла: вокруг них снег не ложится.")] private List<TileDefinition> _heat = new();
        [SerializeField, MinValue(0)] private int _heatRadius = 2;
        [SerializeField, MinValue(1), InfoBox("Событие возможно, только если в мире хотя бы столько тайлов.")] private int _minTiles = 15;

        public bool CanApply(WorldEventContext context) => context.Tiles.CountedCount >= _minTiles && Targets(context).Count > 0;

        public void Apply(WorldEventContext context)
        {
            foreach (var cell in Targets(context)) context.Tiles.Place(cell, _snow.Id, "великий снегопад");
        }

        private IReadOnlyList<GridCoord> Targets(WorldEventContext context)
        {
            var heat = context.Tiles.All.Where(pair => _heat.Any(tile => tile.Id == pair.Value)).Select(pair => pair.Key).ToList();
            return context.Tiles.All
                .Where(pair => pair.Value != _snow.Id && _warm.All(tile => tile.Id != pair.Value))
                .Select(pair => pair.Key)
                .Where(cell => cell != context.Grid.Center && !context.Guards.IsProtected(cell)
                    && heat.All(source => Math.Max(Math.Abs(source.X - cell.X), Math.Abs(source.Y - cell.Y)) > _heatRadius))
                .ToList();
        }
    }
}
