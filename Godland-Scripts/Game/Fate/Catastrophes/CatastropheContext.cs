using System.Linq;
using Godland.Core.Fate.Anomalies;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Emergence;

namespace Godland.Game.Fate.Catastrophes
{
    public sealed class CatastropheContext
    {
        public CatastropheContext(EmergenceContext world, Devourer devourer, CellFertility fertility, Godland.Game.Life.Races.RaceKeeper races, Godland.Core.Powers.Gods.Temples temples, Godland.Core.World.Patterns.GrowthPatterns patterns, Godland.Core.Life.Cities.Urbanization cities, Godland.Game.Powers.Pantheons.PantheonRegistry pantheons)
        {
            Pantheons = pantheons;
            Cities = cities;
            Patterns = patterns;
            Temples = temples;
            Races = races;
            World = world;
            Devourer = devourer;
            Fertility = fertility;
        }

        public EmergenceContext World { get; }
        public Devourer Devourer { get; }
        public CellFertility Fertility { get; }
        public Godland.Game.Life.Races.RaceKeeper Races { get; }
        public Godland.Core.Powers.Gods.Temples Temples { get; }
        public Godland.Core.World.Patterns.GrowthPatterns Patterns { get; }
        public Godland.Core.Life.Cities.Urbanization Cities { get; }
        public Godland.Game.Powers.Pantheons.PantheonRegistry Pantheons { get; }
        public int PeakTiles { get; private set; }

        public void TrackPeak() => PeakTiles = System.Math.Max(PeakTiles, World.Tiles.CountedCount);

        public bool IsShaped(GridCoord cell) => cell != World.Grid.Center && World.Tiles.All.ContainsKey(cell);

        public GridCoord[] Shuffled(System.Collections.Generic.IEnumerable<GridCoord> cells) =>
            cells.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).OrderBy(_ => World.Random.Next(100000)).ToArray();
    }
}
