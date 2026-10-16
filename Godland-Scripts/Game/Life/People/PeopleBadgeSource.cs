using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Presentation.Feedback.Badges;

namespace Godland.Game.Life.People
{
    public sealed class PeopleBadgeSource : ITileBadgeSource
    {
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly Feuds _feuds;
        private readonly Plague _plague;
        private readonly Godfighters _godfighters;

        public PeopleBadgeSource(Settlements settlements, Shrines shrines, Feuds feuds, Plague plague, Godfighters godfighters)
        {
            _plague = plague;
            _godfighters = godfighters;
            _feuds = feuds;
            _settlements = settlements;
            _shrines = shrines;
        }

        public void Collect(GridCoord coord, ICollection<BadgeKind> badges)
        {
            if (_shrines.IsShrine(coord)) badges.Add(BadgeKind.Shrine);
            if (!_settlements.TryGet(coord, out _)) return;

            if (_settlements.IsHungry(coord)) badges.Add(BadgeKind.Hunger);
            if (_settlements.TryGetHunt(coord, out _)) badges.Add(BadgeKind.Hunting);
            if (_feuds.IsFeuding(coord)) badges.Add(BadgeKind.Feud);
            if (_plague.IsSick(coord)) badges.Add(BadgeKind.Plague);
            if (_godfighters.IsRebel(coord)) badges.Add(BadgeKind.Godfighters);
            if (_shrines.TryGet(coord, out _)) badges.Add(BadgeKind.IdolFaith);
        }
    }
}
