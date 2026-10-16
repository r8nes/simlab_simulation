using System.Collections.Generic;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Life.Features
{
    public sealed class FeatureBadgeSource : ITileBadgeSource
    {
        private readonly TileFeatures _features;
        private readonly FeatureInteractions _interactions;
        private readonly FeatureDepletion _depletion;
        private readonly Monsters _monsters;

        public FeatureBadgeSource(TileFeatures features, FeatureInteractions interactions, FeatureDepletion depletion, Monsters monsters)
        {
            _monsters = monsters;
            _features = features;
            _interactions = interactions;
            _depletion = depletion;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_monsters.IsMonster(coord)) badges.Add(BadgeKind.Monster);
            if (_interactions.TryGetThreat(coord, out _)) badges.Add(BadgeKind.Threat);
            if (_depletion.IsDepleted(coord)) badges.Add(BadgeKind.Depleted);
            else if (_features.IsWild(coord)) badges.Add(BadgeKind.Wild);
        }
    }
}
