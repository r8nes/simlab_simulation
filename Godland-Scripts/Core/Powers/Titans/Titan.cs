using System.Collections.Generic;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;

namespace Godland.Core.Powers.Titans
{
    public sealed class Titan
    {
        public Titan(TitanRule rule, GridCoord position, int bornYear)
        {
            Rule = rule;
            Position = position;
            BornYear = bornYear;
            IsAwake = rule.Target != TitanTarget.Sleep;
        }

        public TitanRule Rule { get; }
        public GridCoord Position { get; internal set; }
        public int BornYear { get; internal set; }
        public bool IsAwake { get; internal set; }
        public string Origin { get; internal set; }
        public bool IsTamed { get; internal set; }
        public GridCoord? Destination { get; internal set; }
        public int WakeYear { get; internal set; }
        internal int LulledYear { get; set; }
        internal HashSet<God> Fought { get; } = new();
        internal HashSet<God> Allies { get; } = new();

        public IReadOnlyCollection<God> Pacts => Allies;
    }
}
