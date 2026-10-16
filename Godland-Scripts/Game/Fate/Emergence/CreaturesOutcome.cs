using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Life.Features;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class CreaturesOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly] private FeatureDefinition _creature;
        [SerializeField, InfoBox("Существо появляется на самой клетке (заменяя обитателя).")] private bool _onSite = true;
        [SerializeField, MinValue(0), InfoBox("Сколько ещё соседних клеток занимают существа.")] private int _neighbors;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            if (_onSite)
            {
                if (context.Features.TryGet(site, out _)) context.Features.Remove(site);
                if (context.Features.CanAdd(site)) context.Features.Add(site, _creature.Id);
            }

            foreach (var cell in context.Grid.Neighbors(site).Where(context.Features.CanAdd).OrderBy(_ => context.Random.Next(1000)).Take(_neighbors))
                context.Features.Add(cell, _creature.Id);
        }
    }
}
