using Godland.Core.Powers.Titans;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Powers.Babel;
using Godland.Core.World.Tiles;
using Godland.Core.Life.Heroes;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Biomes;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Cells;
using Godland.Core.Life.Cities;
using Godland.Core.World.Corruption;
using Godland.Core.World.Creation;
using Godland.Core.Life.Features;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.World.Timing;
using Godland.Core.World.Volcanoes;
using Godland.Core.World.Climate;

namespace Godland.Core
{
    public sealed class WorldSimulation
    {
        private readonly WorldClock _clock;
        private readonly CellFertility _fertility;
        private readonly ProsperityRewards _rewards;
        private readonly CreationYield _yield;
        private readonly FeatureInteractions _interactions;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly FeatureDepletion _depletion;
        private readonly FeatureSpread _spread;
        private readonly FeatureEvolutions _featureEvolutions;
        private readonly VolcanoActivity _volcanoes;
        private readonly Feuds _feuds;
        private readonly IdolCataclysms _cataclysms;
        private readonly IReadOnlyList<IWorldStep> _extraSteps;
        private readonly Plague _plague;
        private readonly CorruptionSpread _corruption;
        private readonly Godfighters _godfighters;
        private readonly Monsters _monsters;
        private readonly SuperBiomes _biomes;
        private readonly DelayedWorld _delayed;
        private readonly WorldClimate _climate;
        private readonly Pantheon _pantheon;
        private readonly Urbanization _cities;
        private readonly Realms _realms;
        private readonly Temples _temples;
        private readonly HeroSaga _heroes;
        private readonly LivingLand _livingLand;
        private readonly TowerOfBabel _babel;
        private readonly WorldSleep _sleep;
        private readonly TitanHost _titans;

        public WorldSimulation(WorldClock clock, CellFertility fertility, ProsperityRewards rewards, CreationYield creationYield, FeatureInteractions interactions, Settlements settlements, Shrines shrines, FeatureDepletion depletion,
            FeatureSpread spread, FeatureEvolutions featureEvolutions,
            VolcanoActivity volcanoes, Feuds feuds, IdolCataclysms cataclysms,
            WorldSteps steps, Plague plague, CorruptionSpread corruption, Godfighters godfighters,
            Monsters monsters, SuperBiomes biomes, DelayedWorld delayed,
            WorldClimate climate, Pantheon pantheon, Urbanization cities, Realms realms, Temples temples, HeroSaga heroes, LivingLand livingLand, TowerOfBabel babel, WorldSleep sleep, TitanHost titans)
        {
            _clock = clock;
            _fertility = fertility;
            _rewards = rewards;
            _yield = creationYield;
            _interactions = interactions;
            _settlements = settlements;
            _shrines = shrines;
            _depletion = depletion;
            _spread = spread;
            _featureEvolutions = featureEvolutions;
            _volcanoes = volcanoes;
            _feuds = feuds;
            _cataclysms = cataclysms;
            _extraSteps = steps.Ordered;
            _plague = plague;
            _corruption = corruption;
            _godfighters = godfighters;
            _monsters = monsters;
            _biomes = biomes;
            _delayed = delayed;
            _climate = climate;
            _pantheon = pantheon;
            _cities = cities;
            _realms = realms;
            _temples = temples;
            _heroes = heroes;
            _livingLand = livingLand;
            _babel = babel;
            _sleep = sleep;
            _titans = titans;
        }

        public void AdvanceYear()
        {
            if (_sleep.IsAsleep(_clock.Year + 1))
            {
                _clock.Advance();
                if (!_sleep.IsAsleep(_clock.Year + 1)) _sleep.Wake();
                return;
            }

            _depletion.Advance();
            _interactions.Resolve();
            _spread.Update(_clock.Year + 1);
            _featureEvolutions.Update(_clock.Year + 1);
            _fertility.Grow(_clock.Year + 1);
            _climate.Update(_clock.Year + 1);
            _settlements.Update(_clock.Year + 1);
            _cities.Update(_clock.Year + 1);
            _babel.Update(_clock.Year + 1);
            _shrines.Update(_clock.Year + 1);
            _pantheon.Update(_clock.Year + 1);
            _temples.Update(_clock.Year + 1);
            _realms.Update(_clock.Year + 1);
            _feuds.Update(_clock.Year + 1);
            _plague.Update(_clock.Year + 1);
            _godfighters.Update(_clock.Year + 1);
            _corruption.Update(_clock.Year + 1);
            _monsters.Update(_clock.Year + 1);
            _heroes.Update(_clock.Year + 1);
            _titans.Update(_clock.Year + 1);
            _volcanoes.Update(_clock.Year + 1);
            _biomes.Update(_clock.Year + 1);
            _livingLand.Update(_clock.Year + 1);
            _cataclysms.Update();
            _delayed.Update(_clock.Year + 1);
            foreach (var step in _extraSteps) step.Advance(_clock.Year + 1);
            _rewards.Collect();
            _yield.Collect();
            _clock.Advance();
        }
    }
}
