using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class GhostsAnomaly : IAnomalyEffect
    {
        [SerializeField, MinValue(1), InfoBox("Сколько тайлов прошлого мира проступает на краю.")] private int _tiles = 5;

        public bool CanBegin(AnomalyContext context) => context.Past.PastTiles.Count > 0 && context.Grid.Coords.Any(context.Grid.IsPurchasable);

        public void Begin(AnomalyContext context, int year)
        {
            var past = context.Past.PastTiles;
            for (var i = 0; i < _tiles; i++)
            {
                var edge = context.Grid.Coords.Where(context.Grid.IsPurchasable).ToList();
                if (edge.Count == 0) return;

                var cell = edge[context.Random.Next(edge.Count)];
                context.Grid.Unlock(cell);
                context.Tiles.Place(cell, past[context.Random.Next(past.Count)], "призраки прошлого");
            }
        }
    }
}
