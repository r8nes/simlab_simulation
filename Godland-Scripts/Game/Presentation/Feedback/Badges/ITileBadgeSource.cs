using System.Collections.Generic;
using Godland.Core.World.Grid;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public interface ITileBadgeSource
    {
        void Collect(GridCoord coord, ICollection<BadgeKind> badges);
    }
}
