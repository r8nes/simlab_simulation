using Godland.Core.Powers.Gods;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Writer.Gifts;

namespace Godland.Game.Powers.Prophecies
{
    public sealed class ProphecyContext
    {
        public ProphecyContext(
            Pantheon pantheon, HeroSaga heroes, ProphecyRecord record, Shrines shrines, Settlements settlements, WorldTiles tiles,
            LivingLand livingLand, GiftContext gifts, TileGuards guards,
            Godland.Game.Fate.Catastrophes.CatastropheContext world, MilestoneLawKeeper laws)
        {
            World = world;
            Laws = laws;
            Shrines = shrines;
            Settlements = settlements;
            Tiles = tiles;
            LivingLand = livingLand;
            Gifts = gifts;
            Guards = guards;
            Pantheon = pantheon;
            Heroes = heroes;
            Record = record;
        }

        public Godland.Game.Fate.Catastrophes.CatastropheContext World { get; }
        public MilestoneLawKeeper Laws { get; }
        public Pantheon Pantheon { get; }
        public HeroSaga Heroes { get; }
        public ProphecyRecord Record { get; }
        public Shrines Shrines { get; }
        public Settlements Settlements { get; }
        public WorldTiles Tiles { get; }
        public LivingLand LivingLand { get; }
        public GiftContext Gifts { get; }
        public TileGuards Guards { get; }
    }
}
