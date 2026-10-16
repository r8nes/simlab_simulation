using System.Collections.Generic;
using Godland.Core.World.Corruption;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.World.Corruption
{
    public sealed class CorruptionBadgeSource : ITileBadgeSource
    {
        private readonly WorldTiles _tiles;
        private readonly ICorruptionRules _rules;

        public CorruptionBadgeSource(WorldTiles tiles, ICorruptionRules rules)
        {
            _tiles = tiles;
            _rules = rules;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_tiles.TryGet(coord, out var id) && _rules.IsSource(id)) badges.Add(BadgeKind.Corruption);
        }
    }
}
