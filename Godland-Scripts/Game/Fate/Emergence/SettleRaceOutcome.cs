using System;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class SettleRaceOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Тайл нового народа: его культура задаёт расу.")] private TileDefinition _home;
        [SerializeField, MinValue(1)] private int _population = 3;
        [SerializeField] private bool _removeFeature = true;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            if (_removeFeature && context.Features.TryGet(site, out _)) context.Features.Remove(site);
            context.Tiles.Place(site, _home.Id, title);
            if (!context.Settlements.TryGet(site, out _)) context.Settlements.TrySettle(site, _population);
        }
    }
}
