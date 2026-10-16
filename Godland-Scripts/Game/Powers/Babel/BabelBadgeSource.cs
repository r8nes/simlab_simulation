using System.Collections.Generic;
using Godland.Core.Powers.Babel;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Powers.Babel
{
    public sealed class BabelBadgeSource : ITileBadgeSource
    {
        private readonly TowerOfBabel _tower;

        public BabelBadgeSource(TowerOfBabel tower)
        {
            _tower = tower;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_tower.IsRising && _tower.Site == coord) badges.Add(BadgeKind.Tower);
        }
    }
}
