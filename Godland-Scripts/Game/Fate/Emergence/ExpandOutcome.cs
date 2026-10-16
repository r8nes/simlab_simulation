using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class ExpandOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(1)] private int _population = 2;
        [SerializeField, MinValue(0), InfoBox("Сколько жителей уходит из исходного поселения.")] private int _cost = 1;
        [SerializeField, AssetsOnly, InfoBox("Во что превращается новая земля (пусто — не меняется).")] private TileDefinition _home;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var target = context.Grid.Neighbors(site)
                .Where(cell => context.Tiles.All.ContainsKey(cell) && cell != context.Grid.Center && !context.Settlements.TryGet(cell, out _))
                .Where(cell => !context.Guards.IsProtected(cell))
                .OrderBy(_ => context.Random.Next(1000))
                .Cast<GridCoord?>()
                .FirstOrDefault();
            if (target is not { } cell) return;

            if (_home != null) context.Tiles.Place(cell, _home.Id, title);
            if (context.Settlements.TrySettle(cell, _population) && _cost > 0) context.Settlements.Weaken(site, _cost);
        }
    }
}
