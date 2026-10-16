using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class MemoryCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, Required, InfoBox("Что мир должен запомнить: miracle, eruption, war, plague-survived, monster-slain, hunt:<id хищника>.")]
        private string _key;
        [SerializeField, MinValue(1)] private int _threshold = 5;
        [SerializeField, MinValue(0), InfoBox("Суммировать память в этом радиусе вокруг клетки.")] private int _radius;
        [SerializeField, AssetsOnly, InfoBox("На каком тайле может случиться (пусто — любой).")] private List<TileDefinition> _tiles = new();
        [SerializeField, AssetsOnly, InfoBox("Какой обитатель должен быть на клетке (пусто — не важно).")] private List<FeatureDefinition> _features = new();
        [SerializeField] private bool _needsSettlement;

        public IEnumerable<GridCoord> Sites(EmergenceContext context) =>
            context.Tiles.All.Keys.Where(cell => Fits(context, cell) && context.Memory.Around(cell, _key, _radius) >= _threshold);

        public (int Value, int Threshold) Progress(EmergenceContext context)
        {
            var best = context.Tiles.All.Keys.Where(cell => Fits(context, cell)).Select(cell => context.Memory.Around(cell, _key, _radius)).DefaultIfEmpty(0).Max();
            return (best, _threshold);
        }

        public void Consume(EmergenceContext context, GridCoord site) => context.Memory.Forget(site, _key, _radius);

        private bool Fits(EmergenceContext context, GridCoord cell) =>
            (_tiles.Count == 0 || (context.Tiles.TryGet(cell, out var id) && _tiles.Any(tile => tile.Id == id)))
            && (_features.Count == 0 || (context.Features.TryGet(cell, out var feature) && _features.Any(f => f.Id == feature)))
            && (!_needsSettlement || context.Settlements.TryGet(cell, out _))
            && !context.Guards.IsProtected(cell);
    }
}
