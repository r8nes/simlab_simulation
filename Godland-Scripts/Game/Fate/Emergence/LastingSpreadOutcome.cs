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
    public sealed class LastingSpreadOutcome : IEmergenceOutcome
    {
        [SerializeField, AssetsOnly, InfoBox("Новый закон мира: раз в период эти тайлы рядом с источником превращаются.")] private List<TileDefinition> _from = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _to;
        [SerializeField, AssetsOnly, InfoBox("Рядом с чем должен стоять тайл (пусто — рядом с результатом).")] private List<TileDefinition> _sources = new();
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(1)] private int _count = 2;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var sources = _sources.Count == 0 ? new[] { _to.Id } : _sources.Select(tile => tile.Id).ToArray();
            context.Spreads.Add(_from.Select(tile => tile.Id).ToList(), _to.Id, _period, _count, title, sources);
        }
    }
}
