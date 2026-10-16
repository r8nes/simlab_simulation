using Godland.Game.World.Biomes;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Climate;
using Godland.Game.Fate.Epochs;
using Godland.Game.Life.Features;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Powers.Gods;
using Godland.Game.World.Grid;
using Godland.Game.Life.People;
using Godland.Game.World.Rules;
using Godland.Game.Writer.Run;
using Godland.Game.World.Tiles;
using Godland.Game.Presentation.Timing;
using Godland.Game.Life.Cities;
using Godland.Game.Powers.Titans;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Anomalies;
using Godland.Game.World.Overflow;
using Godland.Game.Powers.Babel;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Prophecies;
using Godland.Game.Life.Heroes;
using Godland.Game.Life.Kingdoms;
using Godland.Game.World.Corruption;
using Godland.Game.World.Volcanoes;
using Godland.Game.Fate.WorldEvents;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Settings
{
    [CreateAssetMenu(menuName = "Godland/Game Settings", fileName = "GameSettings")]
    public sealed class GameSettings : ScriptableObject
    {
        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public GridConfig Grid { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public WorldRulesConfig WorldRules { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public TimeConfig Time { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public SettlementConfig Settlements { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public VolcanoConfig Volcanoes { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public CorruptionConfig Corruption { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public CityConfig Cities { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public OverflowConfig Overflow { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public KingdomConfig Kingdoms { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public DynastyConfig Dynasties { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public HeroConfig Heroes { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public BabelConfig Babel { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public SuperBiomeConfig SuperBiomes { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public ClimateConfig Climate { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public RunConfig Run { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public ProphecyConfig Prophecies { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public SovereignConfig Sovereigns { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public AnomalyConfig Anomalies { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public EmergenceConfig Emergence { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public TitanConfig Titans { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.Life.Races.RacesConfig Races { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.Life.Minds.MindConfig Minds { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.Fate.Catastrophes.CatastropheConfig Catastrophes { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.Life.Reclaim.ReclaimConfig Reclaim { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.World.Peaks.PeaksConfig Peaks { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Writer.Commandments.CommandmentConfig Commandments { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Writer.Progression.ProgressionConfig Progression { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.Fate.Sagas.SagaConfig Sagas { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Writer.Commands.CommandConfig Commands { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Powers.Pantheons.PantheonConfig Pantheons { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Writer.Dilemmas.DilemmaConfig Dilemmas { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public Godland.Game.Writer.Threads.WriterConfig Writer { get; private set; }

        [field: SerializeField, FoldoutGroup("Мир"), Required, InlineEditor]
        public Godland.Game.World.Tiles.WildLandConfig WildLand { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public EpochConfig Epochs { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public WorldEventsConfig WorldEvents { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required, InlineEditor]
        public GodsConfig Gods { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required]
        public TileCatalog TileCatalog { get; private set; }

        [field: SerializeField, FoldoutGroup("Забег"), Required]
        public FeatureCatalog FeatureCatalog { get; private set; }

        [field: SerializeField, FoldoutGroup("Визуал"), Required, InlineEditor]
        public CameraConfig Camera { get; private set; }

        [field: SerializeField, FoldoutGroup("Визуал"), Required, InlineEditor]
        public FeedbackConfig Feedback { get; private set; }

        [field: SerializeField, FoldoutGroup("Визуал"), Required, InlineEditor]
        public TileAnimationConfig TileAnimation { get; private set; }
    }
}
