using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Cells;
using Godland.Core.World.Corruption;
using Godland.Core.World.Creation;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Writer.Actions;
using Godland.Game.Writer.Run;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftContext
    {
        public GiftContext(
            RunUnlocks unlocks, InteractionModifiers interactions, WorldModifiers world, WorldGrid grid, WorldTiles tiles,
            TileFeatures features, IFeatureRules featureRules, CellFertility fertility, Settlements settlements, Shrines shrines,
            CreationPoints creation, SeededRandom random, DelayedWorld delayed, WorldClock clock,
            TileActions actions, CorruptionSpread corruption)
        {
            Actions = actions;
            Corruption = corruption;
            Unlocks = unlocks;
            Interactions = interactions;
            World = world;
            Grid = grid;
            Tiles = tiles;
            Features = features;
            FeatureRules = featureRules;
            Fertility = fertility;
            Settlements = settlements;
            Shrines = shrines;
            Creation = creation;
            Random = random;
            Delayed = delayed;
            Clock = clock;
        }

        public RunUnlocks Unlocks { get; }
        public InteractionModifiers Interactions { get; }
        public WorldModifiers World { get; }
        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public TileFeatures Features { get; }
        public IFeatureRules FeatureRules { get; }
        public CellFertility Fertility { get; }
        public Settlements Settlements { get; }
        public Shrines Shrines { get; }
        public CreationPoints Creation { get; }
        public SeededRandom Random { get; }
        public DelayedWorld Delayed { get; }
        public WorldClock Clock { get; }
        public TileActions Actions { get; }
        public CorruptionSpread Corruption { get; }

        public bool Any(IReadOnlyCollection<string> tileIds) => Tiles.All.Values.Any(tileIds.Contains);

        public T Pick<T>(IReadOnlyList<T> items) => items[Random.Next(items.Count)];

        public void Transform(GridCoord cell, string tileId, int populationLoss)
        {
            Tiles.Place(cell, tileId, "дар");
            Settlements.Weaken(cell, populationLoss);
        }

        public IReadOnlyList<GridCoord> TilesOf(IReadOnlyCollection<string> tileIds) =>
            Tiles.All.Where(pair => tileIds.Contains(pair.Value)).Select(pair => pair.Key).ToList();
    }
}
