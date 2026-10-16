using Godland.Core;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Game.Powers.Sovereigns
{
    public sealed class SovereignContext
    {
        public SovereignContext(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, Pantheon pantheon, Temples temples,
            WorldModifiers modifiers, TileGuards guards, SeededRandom random, FaithPoints faith)
        {
            Faith = faith;
            Grid = grid;
            Tiles = tiles;
            Settlements = settlements;
            Shrines = shrines;
            Pantheon = pantheon;
            Temples = temples;
            Modifiers = modifiers;
            Guards = guards;
            Random = random;
        }

        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public Settlements Settlements { get; }
        public Shrines Shrines { get; }
        public Pantheon Pantheon { get; }
        public Temples Temples { get; }
        public WorldModifiers Modifiers { get; }
        public TileGuards Guards { get; }
        public SeededRandom Random { get; }
        public FaithPoints Faith { get; }
    }
}
