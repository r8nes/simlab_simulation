using System;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Powers.Babel;
using Godland.Core.World.Biomes;
using Godland.Core.Fate.Epochs;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Game.Fate.Anomalies;
using Godland.Game.World.Biomes;
using Godland.Game.Fate.Epochs;
using Godland.Game.Powers.Gods;
using Godland.Game.Powers.Prophecies;
using Godland.Game.Powers.Sovereigns;
using VContainer.Unity;

namespace Godland.Game.Presentation.History
{
    public sealed class ChronicleRecorder : IStartable, IDisposable
    {
        private readonly Chronicle _chronicle;
        private readonly WorldEpoch _epoch;
        private readonly EpochConfig _epochs;
        private readonly Pantheon _pantheon;
        private readonly GodsConfig _gods;
        private readonly SovereignKeeper _sovereigns;
        private readonly AnomalyKeeper _anomalies;
        private readonly ProphecyKeeper _prophecies;
        private readonly TowerOfBabel _tower;
        private readonly Realms _realms;
        private readonly HeroSaga _heroes;
        private readonly SuperBiomes _biomes;
        private readonly SuperBiomeConfig _biomeConfig;
        private readonly Devourer _devourer;
        private readonly WorldHeart _heart;

        public ChronicleRecorder(
            Chronicle chronicle, WorldEpoch epoch, EpochConfig epochs, Pantheon pantheon, GodsConfig gods, SovereignKeeper sovereigns,
            AnomalyKeeper anomalies, ProphecyKeeper prophecies, TowerOfBabel tower, Realms realms, HeroSaga heroes, SuperBiomes biomes,
            SuperBiomeConfig biomeConfig, Devourer devourer, WorldHeart heart)
        {
            _chronicle = chronicle;
            _epoch = epoch;
            _epochs = epochs;
            _pantheon = pantheon;
            _gods = gods;
            _sovereigns = sovereigns;
            _anomalies = anomalies;
            _prophecies = prophecies;
            _tower = tower;
            _realms = realms;
            _heroes = heroes;
            _biomes = biomes;
            _biomeConfig = biomeConfig;
            _devourer = devourer;
            _heart = heart;
        }

        public void Start()
        {
            _epoch.Advanced += OnEpoch;
            _pantheon.Born += OnGodBorn;
            _pantheon.Fallen += OnGodFallen;
            _pantheon.Died += OnGodDied;
            _pantheon.Forgotten += OnGodForgotten;
            _sovereigns.Rose += OnSovereignRose;
            _sovereigns.Fell += OnSovereignFell;
            _anomalies.Began += OnAnomaly;
            _prophecies.Announced += OnProphecy;
            _prophecies.Resolved += OnProphecyResolved;
            _tower.Founded += OnTowerFounded;
            _tower.Completed += OnTowerCompleted;
            _tower.Toppled += OnTowerToppled;
            _realms.Founded += OnKingdomFounded;
            _realms.Dissolved += OnKingdomDissolved;
            _realms.WarDeclared += OnWar;
            _heroes.Triumphed += OnLegend;
            _heroes.Fiasco += OnFiasco;
            _biomes.Entered += OnSuperBiome;
            _devourer.Sated += OnDevourerSated;
            _heart.Died += OnHeartDied;
        }

        public void Dispose()
        {
            _epoch.Advanced -= OnEpoch;
            _pantheon.Born -= OnGodBorn;
            _pantheon.Fallen -= OnGodFallen;
            _pantheon.Died -= OnGodDied;
            _pantheon.Forgotten -= OnGodForgotten;
            _sovereigns.Rose -= OnSovereignRose;
            _sovereigns.Fell -= OnSovereignFell;
            _anomalies.Began -= OnAnomaly;
            _prophecies.Announced -= OnProphecy;
            _prophecies.Resolved -= OnProphecyResolved;
            _tower.Founded -= OnTowerFounded;
            _tower.Completed -= OnTowerCompleted;
            _tower.Toppled -= OnTowerToppled;
            _realms.Founded -= OnKingdomFounded;
            _realms.Dissolved -= OnKingdomDissolved;
            _realms.WarDeclared -= OnWar;
            _heroes.Triumphed -= OnLegend;
            _heroes.Fiasco -= OnFiasco;
            _biomes.Entered -= OnSuperBiome;
            _devourer.Sated -= OnDevourerSated;
            _heart.Died -= OnHeartDied;
        }

        private void OnEpoch(int index) => _chronicle.Add($"<b>Наступила эпоха: {_epochs.Epochs[index].DisplayName}</b>");

        private void OnGodBorn(God god) => _chronicle.Add($"Родился бог: {_gods.NameOf(god)}", god.Seat);

        private void OnGodFallen(God god) => _chronicle.Add($"Пал бог: {_gods.NameOf(god)}", god.Seat);

        private void OnGodDied(God god) => _chronicle.Add($"Умер от старости бог: {_gods.NameOf(god)}", god.Seat);

        private void OnGodForgotten(God god) => _chronicle.Add($"Забыт бог: {_gods.NameOf(god)}", god.Seat);

        private void OnSovereignRose(SovereignDefinition sovereign) => _chronicle.Add($"<b>{sovereign.Slaughter}: {sovereign.DisplayName} стал правителем мира</b>");

        private void OnSovereignFell(SovereignDefinition sovereign) => _chronicle.Add($"Свергнут правитель мира: {sovereign.DisplayName}");

        private void OnAnomaly(AnomalyDefinition anomaly) => _chronicle.Add($"<b>Явление: {anomaly.Title}</b>");

        private void OnProphecy(ProphecyDefinition prophecy, int deadline) => _chronicle.Add($"Пророчество: «{prophecy.Title}»");

        private void OnProphecyResolved(ProphecyDefinition prophecy, MilestoneDefinition milestone, bool fulfilled) =>
            _chronicle.Add($"<b>Новый закон мира: {milestone.DisplayName}</b> ({(fulfilled ? "пророчество сбылось" : "пророчество не сбылось")})");

        private void OnTowerFounded(Godland.Core.World.Grid.GridCoord site) => _chronicle.Add("Заложена Вавилонская башня", site);

        private void OnTowerCompleted(bool blessed) =>
            _chronicle.Add(blessed ? "<b>Башня достроена: мир принял веру строителей</b>" : "<b>Башня достроена: люди отвергли чудеса</b>", _tower.Site);

        private void OnTowerToppled() => _chronicle.Add("Вавилонская башня рухнула", _tower.Site);

        private void OnKingdomFounded(Kingdom kingdom) => _chronicle.Add($"Основано царство {kingdom.Name}", kingdom.Capital);

        private void OnKingdomDissolved(Kingdom kingdom) => _chronicle.Add($"Распалось царство {kingdom.Name}", kingdom.Capital);

        private void OnWar(War war) => _chronicle.Add($"Война: {war.First.Name} против {war.Second.Name}", war.First.Capital);

        private void OnLegend(Hero hero) => _chronicle.Add($"Легенда героя {hero.Name}", hero.Position);

        private void OnFiasco(Hero hero) => _chronicle.Add($"Грандиозный провал героя {hero.Name}", hero.Position);

        private void OnSuperBiome(SuperBiomeRule rule) => _chronicle.Add($"Мир стал супербиомом: {_biomeConfig.DefinitionOf(rule).DisplayName}");

        private void OnDevourerSated() => _chronicle.Add("Пожиратель насытился, бездна закрылась", _devourer.Maw);

        private void OnHeartDied() => _chronicle.Add("<b>Сердце мира остановилось</b>", _heart.Heart);
    }
}
