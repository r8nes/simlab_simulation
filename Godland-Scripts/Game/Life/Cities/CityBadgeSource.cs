using System.Collections.Generic;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Life.Cities
{
    public sealed class CityBadgeSource : ITileBadgeSource
    {
        private readonly Urbanization _cities;

        public CityBadgeSource(Urbanization cities)
        {
            _cities = cities;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_cities.IsCity(coord)) badges.Add(BadgeKind.City);
        }
    }
}
