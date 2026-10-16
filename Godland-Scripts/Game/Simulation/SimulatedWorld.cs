using Godland.Game.Fate.Anomalies;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Powers.Babel;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Prophecies;
using Godland.Core.Life.Heroes;
using Godland.Core;
using Godland.Core.World.Biomes;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Cells;
using Godland.Core.Life.Cities;
using Godland.Core.World.Climate;
using Godland.Core.World.Corruption;
using Godland.Core.World.Creation;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Features;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Kingdoms;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Core.World.Volcanoes;
using Godland.Game.Writer.Actions;
using Godland.Game.Fate.Epochs;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.Writer.Gifts;
using Godland.Game.Presentation.Journal;
using Godland.Game.Writer.Run;
using Godland.Game.Presentation.Settings;
using Godland.Game.World.Tiles;
using Godland.Game.Fate.WorldEvents;
using VContainer;

namespace Godland.Game.Simulation
{
    public sealed class SimulatedWorld
    {
        private readonly IObjectResolver _resolver;

        public SimulatedWorld(GameSettings settings, int seed)
        {
            var builder = new ContainerBuilder();
            new SimulationInstaller(settings, seed).Install(builder);
            builder.RegisterInstance(ProgressionUnlocks.Everything);
            builder.Register<NoPastWorld>(Lifetime.Singleton).As<IPastWorld>();

            _resolver = builder.Build();

            Simulation = Get<WorldSimulation>();
            Get<Godland.Game.Writer.Dilemmas.DilemmaQueue>().AutoChoose = true;
            Get<Godland.Game.Writer.Commandments.CommandmentRewrite>().AutoProclaim = true;
            Get<FertilityOverflow>().SetEnabled(true);
            Get<WorldTiles>().Place(Get<WorldGrid>().Center, settings.Run.StartingTile.Id);
        }

        public WorldSimulation Simulation { get; }

        public T Get<T>() => _resolver.Resolve<T>();
    }
}
