using System;
using System.Linq;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Peaks;
using Godland.Core.Powers.Titans;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Epochs;
using Godland.Game.Life.Races;
using Godland.Game.Writer.Run;
using Godland.Game.Powers.Sovereigns;
using VContainer.Unity;

namespace Godland.Game.Writer.Progression
{
    public sealed class DiscoveryKeeper : IStartable, IDisposable
    {
        private readonly ProgressionConfig _config;
        private readonly ProfileStore _store;
        private readonly CatastropheKeeper _catastrophes;
        private readonly WorldEpoch _epoch;
        private readonly EpochProgression _epochs;
        private readonly EmergenceKeeper _emergences;
        private readonly RaceKeeper _races;
        private readonly WonderAge _wonders;
        private readonly TitanHost _titans;
        private readonly AnomalyKeeper _anomalies;
        private readonly SovereignKeeper _sovereigns;
        private readonly SuperPeaks _peaks;
        private readonly Godland.Game.Fate.Sagas.SagaKeeper _sagas;
        private readonly Godland.Game.Powers.Pantheons.PantheonKeeper _pantheons;

        public DiscoveryKeeper(
            ProgressionConfig config, ProfileStore store, CatastropheKeeper catastrophes, WorldEpoch epoch, EpochProgression epochs,
            EmergenceKeeper emergences, RaceKeeper races, WonderAge wonders, TitanHost titans, AnomalyKeeper anomalies,
            SovereignKeeper sovereigns, SuperPeaks peaks, Godland.Game.Fate.Sagas.SagaKeeper sagas, Godland.Game.Powers.Pantheons.PantheonKeeper pantheons)
        {
            _pantheons = pantheons;
            _sagas = sagas;
            _config = config;
            _store = store;
            _catastrophes = catastrophes;
            _epoch = epoch;
            _epochs = epochs;
            _emergences = emergences;
            _races = races;
            _wonders = wonders;
            _titans = titans;
            _anomalies = anomalies;
            _sovereigns = sovereigns;
            _peaks = peaks;
        }

        public event Action<DiscoveryDefinition, int> Discovered;

        public void Start()
        {
            _catastrophes.Struck += OnCatastrophe;
            _epoch.Advanced += OnEpoch;
            _emergences.Emerged += OnEmergence;
            _races.Born += OnRace;
            _wonders.Began += OnWonders;
            _titans.Awoke += OnTitan;
            _anomalies.Began += OnAnomaly;
            _sovereigns.Rose += OnSovereign;
            _peaks.Raised += OnRidge;
            _sagas.ActBegan += OnSagaAct;
            _pantheons.Arrived += OnPantheon;
        }

        public void Dispose()
        {
            _catastrophes.Struck -= OnCatastrophe;
            _epoch.Advanced -= OnEpoch;
            _emergences.Emerged -= OnEmergence;
            _races.Born -= OnRace;
            _wonders.Began -= OnWonders;
            _titans.Awoke -= OnTitan;
            _anomalies.Began -= OnAnomaly;
            _sovereigns.Rose -= OnSovereign;
            _peaks.Raised -= OnRidge;
            _sagas.ActBegan -= OnSagaAct;
            _pantheons.Arrived -= OnPantheon;
        }

        private void OnCatastrophe(CatastropheDefinition catastrophe) => Witness(DiscoveryKeys.Of("catastrophe", catastrophe.name));
        private void OnEpoch(int _) => Witness(DiscoveryKeys.Of("epoch", _epochs.Current.name));
        private void OnEmergence(EmergenceDefinition emergence, Godland.Core.World.Grid.GridCoord _) => Witness(DiscoveryKeys.Of("emergence", emergence.name));
        private void OnRace(Race race, Godland.Core.World.Grid.GridCoord _) => Witness(DiscoveryKeys.Of("race", race.Definition.name));
        private void OnWonders() => Witness(DiscoveryKeys.Of("signal", "wonder-age"));
        private void OnTitan(Titan titan) => Witness(DiscoveryKeys.Of("titan", titan.Rule.Id));
        private void OnAnomaly(AnomalyDefinition anomaly) => Witness(DiscoveryKeys.Of("anomaly", anomaly.name));
        private void OnSovereign(SovereignDefinition sovereign) => Witness(DiscoveryKeys.Of("sovereign", sovereign.name));
        private void OnRidge(System.Collections.Generic.IReadOnlyList<Godland.Core.World.Grid.GridCoord> _) => Witness(DiscoveryKeys.Of("signal", "ridge"));

        private void OnPantheon(Godland.Game.Powers.Pantheons.PantheonHouse house, string _) => Witness(DiscoveryKeys.Of("pantheon", house.Definition.Id));

        private void OnSagaAct(Godland.Game.Fate.Sagas.SagaActDefinition act) => Witness(DiscoveryKeys.Of("saga-act", act.name));

        private void Witness(string key)
        {
            var profile = _store.Profile;
            foreach (var discovery in _config.Discoveries.Where(candidate => candidate.Key == key && !profile.HasDiscovered(candidate.name)))
            {
                profile.Discover(discovery.name);
                if (discovery.UnlockFeature != ProgressionFeature.None) profile.Unlock(ProgressionUnlocks.KeyOf(discovery.UnlockFeature));
                if (discovery.UnlockWord != null) profile.Unlock(ProgressionUnlocks.KeyOf(discovery.UnlockWord));
                _store.Save();
                Discovered?.Invoke(discovery, _config.DiscoveryReward);
            }
        }
    }
}
