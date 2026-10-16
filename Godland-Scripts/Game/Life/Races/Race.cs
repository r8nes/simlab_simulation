using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.Minds;

namespace Godland.Game.Life.Races
{
    public sealed class Race
    {
        public Race(RaceDefinition definition, string godName, string origin, int bornYear, Mind mind, RaceGenome genome, string name = null)
        {
            Genome = genome;
            Name = name ?? definition.DisplayName;
            Definition = definition;
            GodName = godName;
            Origin = origin;
            BornYear = bornYear;
            Mind = mind;
        }

        public RaceDefinition Definition { get; }
        public string Name { get; }
        public RaceGenome Genome { get; }
        public Race Parent { get; set; }
        public string GodName { get; }
        public string Origin { get; }
        public int BornYear { get; }
        public Mind Mind { get; }
        public HashSet<GridCoord> Cells { get; } = new();
        public bool IsAlive => Cells.Count > 0;
        public bool IsVeteran { get; set; }
        public string LastDeed { get; set; }

        public void ForgetMind() => Mind.Forget();
    }
}
