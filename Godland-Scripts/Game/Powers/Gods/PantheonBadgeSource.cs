using System.Collections.Generic;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Powers.Gods
{
    public sealed class PantheonBadgeSource : ITileBadgeSource
    {
        private readonly Pantheon _pantheon;
        private readonly Temples _temples;

        public PantheonBadgeSource(Pantheon pantheon, Temples temples)
        {
            _pantheon = pantheon;
            _temples = temples;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_pantheon.TryGetAt(coord, out _)) badges.Add(BadgeKind.GodSeat);
            if (_temples.TryGetAt(coord, out _)) badges.Add(BadgeKind.Temple);
        }
    }
}
