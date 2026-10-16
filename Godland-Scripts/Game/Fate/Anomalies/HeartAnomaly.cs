using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class HeartAnomaly : IAnomalyEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _heart;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(1)] private int _radius = 2;
        [SerializeField, AssetsOnly, InfoBox("Во что перерождаются тайлы от удара сердца.")] private List<TileDefinition> _rebirths = new();

        public bool CanBegin(AnomalyContext context) => !context.Heart.IsBeating && Sites(context).Any();

        public void Begin(AnomalyContext context, int year)
        {
            var sites = Sites(context).ToList();
            context.Heart.Awaken(sites[context.Random.Next(sites.Count)], _heart.Id, _period, _radius, _rebirths.Select(tile => tile.Id).ToList());
        }

        private static IEnumerable<Godland.Core.World.Grid.GridCoord> Sites(AnomalyContext context) =>
            context.Tiles.All.Keys.Where(cell => cell != context.Grid.Center && !context.Guards.IsProtected(cell) && !context.Settlements.TryGet(cell, out _));
    }
}
