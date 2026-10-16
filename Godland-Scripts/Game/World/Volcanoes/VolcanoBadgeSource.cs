using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Volcanoes;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.World.Volcanoes
{
    public sealed class VolcanoBadgeSource : ITileBadgeSource
    {
        private readonly VolcanoActivity _volcanoes;

        public VolcanoBadgeSource(VolcanoActivity volcanoes)
        {
            _volcanoes = volcanoes;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_volcanoes.TryGetAwakening(coord, out _)) badges.Add(BadgeKind.Eruption);
        }
    }
}
