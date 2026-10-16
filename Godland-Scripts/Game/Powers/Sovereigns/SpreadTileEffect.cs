using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class SpreadTileEffect : ISovereignEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(1)] private int _count = 3;
        [SerializeField, AssetsOnly, InfoBox("На эти тайлы не распространяется.")] private List<TileDefinition> _immune = new();

        public void OnRise(SovereignContext context, Religion claimant) { }

        public void OnReign(SovereignContext context, int year)
        {
            if (year % _period != 0) return;

            var tiles = context.Tiles;
            var frontier = tiles.All.Where(pair => pair.Value == _tile.Id)
                .SelectMany(pair => context.Grid.Neighbors(pair.Key))
                .Distinct()
                .Where(cell => tiles.TryGet(cell, out var id) && id != _tile.Id && _immune.All(tile => tile.Id != id)
                    && !context.Guards.IsProtected(cell) && cell != context.Grid.Center)
                .OrderBy(_ => context.Random.Next(10000))
                .Take(_count)
                .ToList();
            foreach (var cell in frontier) tiles.Place(cell, _tile.Id, "власть правителя мира");
        }

        public void OnFall(SovereignContext context) { }
    }
}
