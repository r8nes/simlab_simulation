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
    public sealed class WardOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(0)] private int _radius = 2;
        [SerializeField, InfoBox("Защищать от чудовищ поселения вокруг.")] private bool _wardFromMonsters;
        [SerializeField, AssetsOnly, InfoBox("Эти тайлы вокруг мир больше не меняет (например, лес дриад).")] private List<TileDefinition> _protectTiles = new();

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var radius = _radius;
            bool Near(GridCoord cell) => Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= radius;

            if (_wardFromMonsters) context.Wards.Add(Near);
            if (_protectTiles.Count == 0) return;

            var tiles = context.Tiles;
            var ids = _protectTiles.Select(tile => tile.Id).ToList();
            context.Guards.Add(cell => Near(cell) && tiles.TryGet(cell, out var id) && ids.Contains(id));
        }
    }
}
