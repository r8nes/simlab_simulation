using Godland.Core;
using Godland.Core.Fate.Anomalies;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class AnomalyContext
    {
        public AnomalyContext(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, SeededRandom random, WorldModifiers modifiers,
            WorldSleep sleep, Devourer devourer, LandDrift drift, WorldHeart heart, SkyFolk skyFolk, IPastWorld past, TileGuards guards)
        {
            Grid = grid;
            Tiles = tiles;
            Settlements = settlements;
            Random = random;
            Modifiers = modifiers;
            Sleep = sleep;
            Devourer = devourer;
            Drift = drift;
            Heart = heart;
            SkyFolk = skyFolk;
            Past = past;
            Guards = guards;
        }

        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public Settlements Settlements { get; }
        public SeededRandom Random { get; }
        public WorldModifiers Modifiers { get; }
        public WorldSleep Sleep { get; }
        public Devourer Devourer { get; }
        public LandDrift Drift { get; }
        public WorldHeart Heart { get; }
        public SkyFolk SkyFolk { get; }
        public IPastWorld Past { get; }
        public TileGuards Guards { get; }
    }
}
