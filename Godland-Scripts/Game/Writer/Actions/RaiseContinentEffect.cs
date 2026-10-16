using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class RaiseContinentEffect : ITileEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _land;
        [SerializeField, MinValue(1)] private int _cells = 5;

        public string Describe() => $"из океана поднимается материк: {_cells} новых клеток плодородной земли";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Grid.Directions.Any(context.Grid.CanExpand);

        public void Apply(GridCoord target, TileEffectContext context)
        {
            var directions = context.Grid.Directions.Where(context.Grid.CanExpand).ToList();
            var risen = new List<GridCoord>();
            void Collect(GridCoord cell) => risen.Add(cell);

            context.Grid.Unlocked += Collect;
            context.Grid.UnlockSide(directions[context.Random.Next(directions.Count)], _cells);
            context.Grid.Unlocked -= Collect;

            foreach (var cell in risen.Where(context.Tiles.CanPlace))
            {
                context.Tiles.Place(cell, _land.Id, "великое чудо: материк");
                context.Fertility.Add(cell, context.Fertility.Max);
            }
        }
    }
}
