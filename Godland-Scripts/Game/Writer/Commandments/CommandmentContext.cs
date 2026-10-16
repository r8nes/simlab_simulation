using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Epochs;
using Godland.Game.Life.Features;
using Godland.Game.Powers.Gods;
using Godland.Game.Life.Races;
using Godland.Game.Powers.Titans;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentContext
    {
        public CommandmentContext(
            EmergenceContext world, RaceKeeper races, GodsConfig gods, TitanConfig titans, FeatureCatalog features,
            EpochProgression epochs, CatastropheKeeper catastrophes, Godland.Core.World.Peaks.SuperPeaks peaks)
        {
            Peaks = peaks;
            World = world;
            Races = races;
            Gods = gods;
            Titans = titans;
            Features = features;
            Epochs = epochs;
            Catastrophes = catastrophes;
        }

        public EmergenceContext World { get; }
        public RaceKeeper Races { get; }
        public GodsConfig Gods { get; }
        public TitanConfig Titans { get; }
        public FeatureCatalog Features { get; }
        public EpochProgression Epochs { get; }
        public CatastropheKeeper Catastrophes { get; }
        public Godland.Core.World.Peaks.SuperPeaks Peaks { get; }

        public bool IsShaped(GridCoord cell) => cell != World.Grid.Center && World.Tiles.All.ContainsKey(cell);

        public GridCoord? RandomShaped() => Pick(World.Tiles.All.Keys.Where(IsShaped));

        public GridCoord? Pick(System.Collections.Generic.IEnumerable<GridCoord> cells)
        {
            var list = cells.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            return list.Count == 0 ? null : list[World.Random.Next(list.Count)];
        }
    }
}
