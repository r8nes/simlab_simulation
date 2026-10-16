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
    public sealed class TransformAroundOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;
        [SerializeField, MinValue(1)] private int _count = 2;
        [SerializeField, MinValue(1)] private int _radius = 2;
        [SerializeField, AssetsOnly, InfoBox("Какие тайлы превращаются (пусто — любые).")] private List<TileDefinition> _from = new();
        [SerializeField, InfoBox("Выбирать клетки по краю мира, а не вокруг места.")] private bool _atEdges;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var cells = context.Tiles.All.Keys
                .Where(cell => cell != context.Grid.Center && !context.Guards.IsProtected(cell))
                .Where(cell => _from.Count == 0 || (context.Tiles.TryGet(cell, out var id) && _from.Any(tile => tile.Id == id)))
                .Where(cell => _atEdges
                    ? context.Grid.Neighbors(cell).Count(context.Tiles.All.ContainsKey) < 4
                    : cell != site && Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius)
                .OrderBy(_ => context.Random.Next(10000))
                .Take(_count)
                .ToList();
            foreach (var cell in cells) context.Tiles.Place(cell, _tile.Id, title);
        }
    }
}
