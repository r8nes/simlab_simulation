using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.World.Evolution
{
    public sealed class EvolutionBadgeSource : ITileBadgeSource
    {
        private readonly TileEvolutions _evolutions;

        public EvolutionBadgeSource(TileEvolutions evolutions)
        {
            _evolutions = evolutions;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_evolutions.HasReadyOption(coord)) badges.Add(BadgeKind.EvolutionReady);
        }
    }
}
