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
    public sealed class ExpandDeed : ISovereignDeed
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;
        [SerializeField, MinValue(1)] private int _count = 3;
        [SerializeField, AssetsOnly] private List<TileDefinition> _immune = new();

        public string Name => $"стихия наступает: {_tile.DisplayName}";

        public int Score(SovereignContext context, Religion claimant, Temperament temperament) =>
            Frontier(context).Any() ? temperament.Greed + 20 : 0;

        public string Perform(SovereignContext context, Religion claimant)
        {
            var cells = Frontier(context).OrderBy(_ => context.Random.Next(10000)).Take(_count).ToList();
            foreach (var cell in cells) context.Tiles.Place(cell, _tile.Id, "власть правителя мира");
            return $"{Name} ({cells.Count} тайлов)";
        }

        private IEnumerable<Godland.Core.World.Grid.GridCoord> Frontier(SovereignContext context) =>
            context.Tiles.All.Where(pair => pair.Value == _tile.Id)
                .SelectMany(pair => context.Grid.Neighbors(pair.Key))
                .Distinct()
                .Where(cell => context.Tiles.TryGet(cell, out var id) && id != _tile.Id && _immune.All(tile => tile.Id != id)
                    && !context.Guards.IsProtected(cell) && cell != context.Grid.Center);
    }
}
