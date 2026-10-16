using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Life.Features;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class WildArrivalEffect : IWorldEventEffect
    {
        [SerializeField, AssetsOnly, InfoBox("Кто может прийти: на свободный тайл своей среды, дикими.")]
        private List<FeatureDefinition> _candidates = new();

        public bool CanApply(WorldEventContext context) => _candidates.Count > 0;

        public void Apply(WorldEventContext context)
        {
            var options = Options(context).ToList();
            if (options.Count == 0)
            {
                var stranger = context.Pick(_candidates);
                context.Outcome = $"издалека пришли {stranger.DisplayName.ToLowerInvariant()}, но не нашли себе места и ушли";
                return;
            }

            var (cell, featureId) = context.Pick(options);
            context.Features.AddWild(cell, featureId);
            var arrived = _candidates.First(feature => feature.Id == featureId);
            context.Outcome = $"издалека пришли {arrived.DisplayName.ToLowerInvariant()} и поселились на клетке ({cell.X}, {cell.Y})";
        }

        private IEnumerable<(GridCoord, string)> Options(WorldEventContext context) =>
            from pair in context.Tiles.All
            where !context.Features.TryGet(pair.Key, out _)
            from feature in _candidates
            where context.FeatureRules.CanLiveOn(feature.Id, pair.Value)
            select (pair.Key, feature.Id);
    }
}
