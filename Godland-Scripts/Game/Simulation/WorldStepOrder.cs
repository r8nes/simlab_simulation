using Godland.Core;
using Godland.Core.Life.Features;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Epochs;
using Godland.Game.Fate.Sagas;
using Godland.Game.Fate.WorldEvents;
using Godland.Game.Life.Kingdoms;
using Godland.Game.Life.Races;
using Godland.Game.Life.Reclaim;
using Godland.Game.Powers.Pantheons;
using Godland.Game.Powers.Prophecies;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Writer.Commandments;
using Godland.Game.Writer.Dilemmas;
using Godland.Game.Writer.Threads;
using VContainer;

namespace Godland.Game.Simulation
{
    public static class WorldStepOrder
    {
        public static WorldSteps Build(IObjectResolver resolver) => new(new IWorldStep[]
        {
            resolver.Resolve<CreatureGenes>(),
            resolver.Resolve<WorldEventRoller>(),
            resolver.Resolve<MilestoneLawKeeper>(),
            resolver.Resolve<ProphecyKeeper>(),
            resolver.Resolve<SovereignKeeper>(),
            resolver.Resolve<AnomalyKeeper>(),
            resolver.Resolve<LastingSpreads>(),
            resolver.Resolve<EmergenceKeeper>(),
            resolver.Resolve<RaceKeeper>(),
            resolver.Resolve<WonderAge>(),
            resolver.Resolve<CatastropheKeeper>(),
            resolver.Resolve<EpochLawKeeper>(),
            resolver.Resolve<ReclaimKeeper>(),
            resolver.Resolve<SagaKeeper>(),
            resolver.Resolve<CommandmentKeeper>(),
            resolver.Resolve<PantheonKeeper>(),
            resolver.Resolve<WorldSchemes>(),
            resolver.Resolve<Dynasties>(),
            resolver.Resolve<WorldDilemmas>(),
            resolver.Resolve<WriterThreads>()
        });
    }
}
