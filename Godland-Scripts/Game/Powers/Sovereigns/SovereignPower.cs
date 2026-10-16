using System.Linq;
using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public static class SovereignPower
    {
        public static int Followers(SovereignContext context, Religion claimant) => context.Shrines.FollowersOf(claimant).Count;

        public static int Rivals(SovereignContext context, Religion claimant) =>
            context.Shrines.Religions.Where(religion => religion != claimant).Sum(religion => context.Shrines.FollowersOf(religion).Count);

        public static int PercentOfWorld(SovereignContext context, Religion claimant)
        {
            var total = context.Shrines.BelieverCount;
            return total == 0 ? 0 : Followers(context, claimant) * 100 / total;
        }

        public static bool Borders(SovereignContext context, Religion claimant, Godland.Core.World.Grid.GridCoord settlement) =>
            context.Grid.Neighbors(settlement).Any(cell => context.Shrines.TryGet(cell, out var faith) && faith == claimant);
    }
}
