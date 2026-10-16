using Godland.Core;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.Fate.Emergence;
using Godland.Core.Fate.Epochs;
using Godland.Core.Flow;
using Godland.Core.Life.Cities;
using Godland.Core.Life.Features;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.Powers.Babel;
using Godland.Core.Powers.Gods;
using Godland.Core.Powers.Titans;
using Godland.Core.Randomness;
using Godland.Core.World.Biomes;
using Godland.Core.World.Cells;
using Godland.Core.World.Climate;
using Godland.Core.World.Corruption;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.World.Patterns;
using Godland.Core.World.Peaks;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Core.World.Volcanoes;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Epochs;
using Godland.Game.Fate.Sagas;
using Godland.Game.Fate.WorldEvents;
using Godland.Game.Life.Features;
using Godland.Game.Life.Kingdoms;
using Godland.Game.Life.Races;
using Godland.Game.Life.Reclaim;
using Godland.Game.Powers.Pantheons;
using Godland.Game.Powers.Prophecies;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using Godland.Game.Presentation.Overview;
using Godland.Game.Presentation.Settings;
using Godland.Game.World.Evolution;
using Godland.Game.World.Tiles;
using Godland.Game.Writer.Actions;
using Godland.Game.Writer.Commandments;
using Godland.Game.Writer.Commands;
using Godland.Game.Writer.Dilemmas;
using Godland.Game.Writer.Gifts;
using Godland.Game.Writer.Run;
using Godland.Game.Writer.Threads;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Simulation
{
    public sealed class SimulationInstaller : IInstaller
    {
        private readonly GameSettings _settings;
        private readonly int _seed;

        public SimulationInstaller(GameSettings settings, int seed)
        {
            _settings = settings;
            _seed = seed;
        }

        public void Install(IContainerBuilder builder)
        {
            new SettingsInstaller(_settings).Install(builder);
            InstallKernel(builder);
            InstallWorld(builder);
            InstallLife(builder);
            InstallPowers(builder);
            InstallFate(builder);
            InstallWriter(builder);
            InstallChronicle(builder);
            builder.Register(WorldStepOrder.Build, Lifetime.Singleton);
            builder.RegisterBuildCallback(WakeUp);
        }

        private void InstallKernel(IContainerBuilder builder)
        {
            var grid = _settings.Grid;
            builder.RegisterInstance(new WorldGrid(grid.Width, grid.Height, grid.InitialUnlockedSize, grid.WorldSize));
            builder.RegisterInstance(new SeededRandom(_seed));
            builder.Register<WorldClock>(Lifetime.Singleton);
            builder.Register<WorldSimulation>(Lifetime.Singleton);
            builder.Register<FlowBias>(Lifetime.Singleton);
            builder.Register<WorldModifiers>(Lifetime.Singleton);
            builder.Register<DelayedWorld>(Lifetime.Singleton);
            builder.Register<TileGuards>(Lifetime.Singleton);
            builder.Register<PlayerLocks>(Lifetime.Singleton);
            builder.Register<GrowthPatterns>(Lifetime.Singleton);
            builder.Register<RunUnlocks>(Lifetime.Singleton);
        }

        private void InstallWorld(IContainerBuilder builder)
        {
            var rules = _settings.WorldRules;
            builder.RegisterInstance(new CreationPoints(rules.StartingCreationPoints));
            builder.Register<CellFertility>(Lifetime.Singleton)
                .WithParameter("maxFertility", rules.MaxFertility)
                .WithParameter("growthPeriod", rules.FertilityGrowthPeriod);
            builder.Register<CellPurchase>(Lifetime.Singleton)
                .WithParameter("baseCost", rules.CellBaseCost)
                .WithParameter("costStep", rules.CellCostStep);
            builder.Register<ProsperityRewards>(Lifetime.Singleton).WithParameter(rules.MaxFertilityReward);
            builder.Register<CreationYield>(Lifetime.Singleton);
            builder.Register<WorldTiles>(Lifetime.Singleton);
            builder.Register<TileHistory>(Lifetime.Singleton);
            builder.Register<LivingLand>(Lifetime.Singleton);
            builder.Register<TileBrush>(Lifetime.Singleton);
            builder.Register<TilePlacement>(Lifetime.Singleton);
            builder.Register<WildLand>(Lifetime.Singleton);
            builder.Register<VolcanoActivity>(Lifetime.Singleton);
            builder.Register<SuperBiomes>(Lifetime.Singleton);
            builder.Register<WorldClimate>(Lifetime.Singleton);
            builder.Register<CorruptionSpread>(Lifetime.Singleton);
            builder.Register<FertilityOverflow>(Lifetime.Singleton);
            builder.Register<SuperPeaks>(Lifetime.Singleton);
        }

        private static void InstallLife(IContainerBuilder builder)
        {
            builder.Register<TileFeatures>(Lifetime.Singleton);
            builder.Register<InteractionModifiers>(Lifetime.Singleton);
            builder.Register<FeatureDepletion>(Lifetime.Singleton);
            builder.Register<FeatureSpread>(Lifetime.Singleton);
            builder.Register<CreatureGenes>(Lifetime.Singleton);
            builder.Register<FeatureEvolutions>(Lifetime.Singleton);
            builder.Register<Monsters>(Lifetime.Singleton);
            builder.Register<FeatureInteractions>(Lifetime.Singleton);
            builder.Register<FaithPoints>(Lifetime.Singleton);
            builder.Register<Settlements>(Lifetime.Singleton);
            builder.Register<IdolOracle>(Lifetime.Singleton);
            builder.Register<Shrines>(Lifetime.Singleton);
            builder.Register<Feuds>(Lifetime.Singleton);
            builder.Register<Plague>(Lifetime.Singleton);
            builder.Register<Godfighters>(Lifetime.Singleton);
            builder.Register<Urbanization>(Lifetime.Singleton);
            builder.Register<Realms>(Lifetime.Singleton);
            builder.Register<Dynasties>(Lifetime.Singleton);
            builder.Register<HeroSaga>(Lifetime.Singleton);
            builder.Register<RaceKeeper>(Lifetime.Singleton);
            builder.Register<WonderAge>(Lifetime.Singleton);
            builder.Register<ReclaimKeeper>(Lifetime.Singleton);
        }

        private static void InstallPowers(IContainerBuilder builder)
        {
            builder.Register<Pantheon>(Lifetime.Singleton);
            builder.Register<Temples>(Lifetime.Singleton);
            builder.Register<TitanHost>(Lifetime.Singleton);
            builder.Register<TowerOfBabel>(Lifetime.Singleton);
            builder.Register<PantheonRegistry>(Lifetime.Singleton);
            builder.Register<PantheonKeeper>(Lifetime.Singleton);
            builder.Register<WorldSchemes>(Lifetime.Singleton);
            builder.Register<ProphecyRecord>(Lifetime.Singleton);
            builder.Register<ProphecyContext>(Lifetime.Singleton);
            builder.Register<ProphecyKeeper>(Lifetime.Singleton);
            builder.Register<MilestoneLawKeeper>(Lifetime.Singleton);
            builder.Register<SovereignContext>(Lifetime.Singleton);
            builder.Register<SovereignKeeper>(Lifetime.Singleton);
        }

        private static void InstallFate(IContainerBuilder builder)
        {
            builder.Register<WorldEpoch>(Lifetime.Singleton);
            builder.Register<EpochProgression>(Lifetime.Singleton);
            builder.Register<EpochLawKeeper>(Lifetime.Singleton);
            builder.Register<IdolCataclysms>(Lifetime.Singleton);
            builder.Register<WorldEventContext>(Lifetime.Singleton);
            builder.Register<WorldEventRoller>(Lifetime.Singleton);
            builder.Register<WorldSleep>(Lifetime.Singleton);
            builder.Register<Devourer>(Lifetime.Singleton);
            builder.Register<LandDrift>(Lifetime.Singleton);
            builder.Register<WorldHeart>(Lifetime.Singleton);
            builder.Register<SkyFolk>(Lifetime.Singleton);
            builder.Register<AnomalyContext>(Lifetime.Singleton);
            builder.Register<AnomalyKeeper>(Lifetime.Singleton);
            builder.Register<WorldMemory>(Lifetime.Singleton);
            builder.Register<MonsterWards>(Lifetime.Singleton);
            builder.Register<LastingSpreads>(Lifetime.Singleton);
            builder.Register<EmergenceContext>(Lifetime.Singleton);
            builder.Register<EmergenceKeeper>(Lifetime.Singleton);
            builder.Register<MemoryRecorder>(Lifetime.Singleton);
            builder.Register<CatastropheContext>(Lifetime.Singleton);
            builder.Register<CatastropheKeeper>(Lifetime.Singleton);
            builder.Register<SagaKeeper>(Lifetime.Singleton);
        }

        private static void InstallWriter(IContainerBuilder builder)
        {
            builder.Register<TileUpgrades>(Lifetime.Singleton);
            builder.Register<TileActions>(Lifetime.Singleton);
            builder.Register<TileEvolutions>(Lifetime.Singleton);
            builder.Register<GiftContext>(Lifetime.Singleton);
            builder.Register<GiftOffers>(Lifetime.Singleton);
            builder.Register<CommandmentContext>(Lifetime.Singleton);
            builder.Register<CommandmentDraft>(Lifetime.Singleton);
            builder.Register<CommandmentKeeper>(Lifetime.Singleton);
            builder.Register<CommandmentRewrite>(Lifetime.Singleton);
            builder.Register<CommandTargeting>(Lifetime.Singleton);
            builder.Register<EntityCommands>(Lifetime.Singleton);
            builder.Register<CommandDiscount>(Lifetime.Singleton);
            builder.Register<DilemmaQueue>(Lifetime.Singleton);
            builder.Register<WorldDilemmas>(Lifetime.Singleton);
            builder.Register<WriterThreads>(Lifetime.Singleton);
        }

        private static void InstallChronicle(IContainerBuilder builder)
        {
            builder.Register<EventJournal>(Lifetime.Singleton);
            builder.Register<Chronicle>(Lifetime.Singleton);
            builder.Register<FaithNames>(Lifetime.Singleton);
            builder.Register<PantheonsReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<EpochReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<ThreatsReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<TitansReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<RacesReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<GodsReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<FaithsReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<RealmsReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<HeroesReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<CommandmentReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<EmergenceReport>(Lifetime.Singleton).As<IWorldReport>();
            builder.Register<SagaReport>(Lifetime.Singleton).As<IWorldReport>();
        }

        private static void WakeUp(IObjectResolver resolver)
        {
            resolver.Resolve<WildLand>();
            resolver.Resolve<MemoryRecorder>();
            resolver.Resolve<EpochProgression>();
            resolver.Resolve<SuperPeaks>();
            resolver.Resolve<WorldSimulation>();
        }
    }
}
