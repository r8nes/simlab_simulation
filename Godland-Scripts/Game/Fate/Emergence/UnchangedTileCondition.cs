using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class UnchangedTileCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, AssetsOnly] private List<TileDefinition> _tiles = new();
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 500;

        public IEnumerable<GridCoord> Sites(EmergenceContext context) =>
            context.Tiles.All.Where(pair => _tiles.Any(tile => tile.Id == pair.Value) && !context.Guards.IsProtected(pair.Key))
                .Select(pair => pair.Key)
                .Where(cell => LastChange(context, cell) is { } year && context.Clock.Year - year >= _years);

        public (int Value, int Threshold) Progress(EmergenceContext context)
        {
            var best = context.Tiles.All.Where(pair => _tiles.Any(tile => tile.Id == pair.Value))
                .Select(pair => LastChange(context, pair.Key) is { } year ? context.Clock.Year - year : 0)
                .DefaultIfEmpty(0).Max();
            return (best, _years);
        }

        public void Consume(EmergenceContext context, GridCoord site) { }

        private static int? LastChange(EmergenceContext context, GridCoord cell) =>
            context.History.Of(cell).Where(record => record.TileId != null).Select(record => (int?)record.Year).LastOrDefault();
    }
}
