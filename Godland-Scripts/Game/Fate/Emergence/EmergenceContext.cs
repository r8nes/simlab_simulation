using Godland.Core;
using Godland.Core.World.Biomes;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Game.Powers.Sovereigns;
using Godland.Core.Fate.Emergence;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;

namespace Godland.Game.Fate.Emergence
{
    public sealed class EmergenceContext
    {
        public EmergenceContext(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, Settlements settlements, Plague plague, WorldMemory memory,
            TileHistory history, TileGuards guards, SeededRandom random, WorldClock clock, SuperBiomes biomes, MonsterWards wards,
            Shrines shrines, Pantheon pantheon, Realms realms, HeroSaga heroes, WorldModifiers modifiers, SovereignKeeper sovereigns,
            LastingSpreads spreads, Godland.Core.Powers.Titans.TitanHost titans, Godland.Core.Fate.Epochs.WorldEpoch epoch)
        {
            Epoch = epoch;
            Titans = titans;
            Shrines = shrines;
            Pantheon = pantheon;
            Realms = realms;
            Heroes = heroes;
            Modifiers = modifiers;
            Sovereigns = sovereigns;
            Spreads = spreads;
            Wards = wards;
            Grid = grid;
            Tiles = tiles;
            Features = features;
            Settlements = settlements;
            Plague = plague;
            Memory = memory;
            History = history;
            Guards = guards;
            Random = random;
            Clock = clock;
            Biomes = biomes;
        }

        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public TileFeatures Features { get; }
        public Settlements Settlements { get; }
        public Plague Plague { get; }
        public WorldMemory Memory { get; }
        public TileHistory History { get; }
        public TileGuards Guards { get; }
        public SeededRandom Random { get; }
        public WorldClock Clock { get; }
        public SuperBiomes Biomes { get; }
        public MonsterWards Wards { get; }
        public Shrines Shrines { get; }
        public Pantheon Pantheon { get; }
        public Realms Realms { get; }
        public HeroSaga Heroes { get; }
        public WorldModifiers Modifiers { get; }
        public SovereignKeeper Sovereigns { get; }
        public LastingSpreads Spreads { get; }
        public Godland.Core.Powers.Titans.TitanHost Titans { get; }
        public Godland.Core.Fate.Epochs.WorldEpoch Epoch { get; }
    }
}
