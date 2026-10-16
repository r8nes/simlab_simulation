using Godland.Core.Randomness;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Powers.Babel;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Biomes;
using Godland.Core.World.Cells;
using Godland.Core.Life.Features;
using Godland.Core.World.Volcanoes;
using Godland.Core.World.Creation;
using Godland.Core.Life.People;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Game.Writer.Actions
{
    public sealed class TileEffectContext
    {
        public TileEffectContext(WorldGrid grid, WorldTiles tiles, CellFertility fertility, Settlements settlements, Shrines shrines, CreationPoints creation,
            SuperBiomes superBiomes, TileFeatures features, Monsters monsters, VolcanoActivity volcanoes, Temples temples, Realms realms, HeroSaga heroes, TowerOfBabel babel, Devourer devourer, SeededRandom random, Pantheon pantheon,
            Godland.Game.Fate.Anomalies.AnomalyKeeper anomalies, Godland.Core.Powers.Titans.TitanHost titans, Godland.Game.Fate.Sagas.SagaKeeper sagas)
        {
            Sagas = sagas;
            Grid = grid;
            Tiles = tiles;
            Fertility = fertility;
            Settlements = settlements;
            Shrines = shrines;
            Creation = creation;
            SuperBiomes = superBiomes;
            Features = features;
            Monsters = monsters;
            Volcanoes = volcanoes;
            Temples = temples;
            Realms = realms;
            Heroes = heroes;
            Babel = babel;
            Devourer = devourer;
            Random = random;
            Pantheon = pantheon;
            Anomalies = anomalies;
            Titans = titans;
        }

        public WorldGrid Grid { get; }
        public WorldTiles Tiles { get; }
        public CellFertility Fertility { get; }
        public Settlements Settlements { get; }
        public Shrines Shrines { get; }
        public CreationPoints Creation { get; }
        public SuperBiomes SuperBiomes { get; }
        public TileFeatures Features { get; }
        public Monsters Monsters { get; }
        public VolcanoActivity Volcanoes { get; }
        public Temples Temples { get; }
        public Realms Realms { get; }
        public HeroSaga Heroes { get; }
        public TowerOfBabel Babel { get; }
        public Devourer Devourer { get; }
        public SeededRandom Random { get; }
        public Pantheon Pantheon { get; }
        public Godland.Game.Fate.Anomalies.AnomalyKeeper Anomalies { get; }
        public Godland.Core.Powers.Titans.TitanHost Titans { get; }
        public Godland.Game.Fate.Sagas.SagaKeeper Sagas { get; }
    }
}
