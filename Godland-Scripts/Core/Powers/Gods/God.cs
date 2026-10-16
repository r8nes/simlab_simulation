using Godland.Core.World.Grid;
using Godland.Core.Life.People;

namespace Godland.Core.Powers.Gods
{
    public sealed class God
    {
        public God(GodRule rule, GridCoord seat, Religion religion, int bornYear, string heroName = null, string pantheonId = null)
        {
            PantheonId = pantheonId;
            Rule = rule;
            BornYear = bornYear;
            HeroName = heroName;
            Seat = seat;
            Religion = religion;
            YearsToAct = rule.ActionPeriod;
        }

        public GodRule Rule { get; }
        public GridCoord Seat { get; internal set; }
        public Religion Religion { get; }
        public int BornYear { get; }
        public string HeroName { get; }
        public string PantheonId { get; }
        public int YearsToAct { get; private set; }
        public int SilentUntil { get; internal set; }

        public bool Tick()
        {
            if (--YearsToAct > 0) return false;

            YearsToAct = Rule.ActionPeriod;
            return true;
        }
    }
}
