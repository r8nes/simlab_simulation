using System.Collections.Generic;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Fate.Cataclysms
{
    public sealed class CataclysmBadgeSource : ITileBadgeSource
    {
        private readonly IdolCataclysms _cataclysms;

        public CataclysmBadgeSource(IdolCataclysms cataclysms)
        {
            _cataclysms = cataclysms;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_cataclysms.Current?.Religion.Shrine == coord) badges.Add(BadgeKind.Summon);
        }
    }
}
