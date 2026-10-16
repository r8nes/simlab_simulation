using Godland.Core.World.Cells;
using Godland.Core.World.Creation;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Game.Fate.WorldEvents
{
    public sealed class WorldEventContext
    {
        public WorldEventContext(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, IFeatureRules featureRules, CellFertility fertility,
            Settlements settlements, CreationPoints creation, FaithPoints faith, SeededRandom random, TileGuards guards)
        {
            Guards = guards;
            Grid = grid;
            Tiles = tiles;
            Features = features;
            FeatureRules = featureRules;
            Fertility = fertility;
            Settlements = settlements;
            Creation = creation;
            Faith = faith;
            Random = random;
        }

        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public TileFeatures Features { get; }
        public IFeatureRules FeatureRules { get; }
        public CellFertility Fertility { get; }
        public Settlements Settlements { get; }
        public CreationPoints Creation { get; }
        public FaithPoints Faith { get; }
        public SeededRandom Random { get; }
        public TileGuards Guards { get; }

        public string Outcome { get; set; }

        public T Pick<T>(System.Collections.Generic.IReadOnlyList<T> items) => items[Random.Next(items.Count)];
    }
}
