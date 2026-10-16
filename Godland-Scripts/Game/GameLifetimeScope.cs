using Godland.Game.Writer.Actions;
using Godland.Game.World.Biomes;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Climate;
using Godland.Game.World.Creation;
using Godland.Game.Life.Features;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Writer.Gifts;
using Godland.Game.World.Grid;
using Godland.Game.Life.People;
using Godland.Game.Writer.Run;
using Godland.Game.Presentation.Settings;
using Godland.Game.World.Tiles;
using Godland.Game.Presentation.Timing;
using Godland.Game.Life.Cities;
using Godland.Game.Powers.Titans;
using Godland.Game.Fate.Emergence;
using Godland.Game.Presentation.History;
using Godland.Game.Fate.Anomalies;
using Godland.Game.World.Overflow;
using Godland.Game.Powers.Babel;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Prophecies;
using Godland.Game.Life.Heroes;
using Godland.Game.Life.Kingdoms;
using Godland.Game.World.Corruption;
using Godland.Game.World.Volcanoes;
using Godland.Game.Fate.Cataclysms;
using Godland.Game.Presentation.Inspection;
using Godland.Game.Powers.Gods;
using Godland.Game.Presentation.Journal;
using Godland.Game.Fate.WorldEvents;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            new Godland.Game.Simulation.SimulationInstaller(_settings, RunInstaller.ResolveSeed(_settings)).Install(builder);
            new GridInstaller(_settings).Install(builder);
            new TilesInstaller().Install(builder);
            new FeaturesInstaller().Install(builder);
            new InteractionInstaller().Install(builder);
            new PeopleInstaller().Install(builder);
            new VolcanoesInstaller().Install(builder);
            new BiomesInstaller().Install(builder);
            new ClimateInstaller().Install(builder);
            new CorruptionInstaller().Install(builder);
            new CitiesInstaller().Install(builder);
            new OverflowInstaller().Install(builder);
            new KingdomsInstaller().Install(builder);
            new HeroesInstaller().Install(builder);
            new BabelInstaller().Install(builder);
            new ProphecyInstaller().Install(builder);
            new SovereignInstaller().Install(builder);
            new AnomaliesInstaller().Install(builder);
            new HistoryInstaller().Install(builder);
            new EmergenceInstaller().Install(builder);
            new TitansInstaller().Install(builder);
            new Godland.Game.Life.Races.RacesInstaller().Install(builder);
            new Godland.Game.Fate.Catastrophes.CatastrophesInstaller().Install(builder);
            new Godland.Game.Writer.Commandments.CommandmentsInstaller().Install(builder);
            new Godland.Game.Fate.Sagas.SagasInstaller().Install(builder);
            new Godland.Game.Powers.Pantheons.PantheonsInstaller().Install(builder);
            new Godland.Game.Writer.Dilemmas.DilemmasInstaller().Install(builder);
            new Godland.Game.Writer.Threads.WriterInstaller().Install(builder);
            new Godland.Game.Writer.Progression.ProgressionInstaller().Install(builder);
            new Godland.Game.Presentation.Overview.OverviewInstaller().Install(builder);
            new CataclysmsInstaller().Install(builder);
            new PantheonInstaller().Install(builder);
            new JournalInstaller().Install(builder);
            new InspectionInstaller().Install(builder);
            new EpochsInstaller().Install(builder);
            new CreationInstaller(_settings).Install(builder);
            new TimeInstaller().Install(builder);
            new RunInstaller(_settings).Install(builder);
            new GiftsInstaller().Install(builder);
            new FeedbackInstaller().Install(builder);
            new CameraInstaller().Install(builder);
        }
    }
}
