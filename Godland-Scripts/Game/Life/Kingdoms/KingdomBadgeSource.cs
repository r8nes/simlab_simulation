using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Life.Kingdoms
{
    public sealed class KingdomBadgeSource : ITileBadgeSource
    {
        private readonly Realms _realms;

        public KingdomBadgeSource(Realms realms)
        {
            _realms = realms;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (!_realms.TryGetAt(coord, out var kingdom)) return;

            if (kingdom.Capital == coord) badges.Add(BadgeKind.Capital);
            if (_realms.IsAtWar(kingdom)) badges.Add(BadgeKind.War);
        }
    }
}
