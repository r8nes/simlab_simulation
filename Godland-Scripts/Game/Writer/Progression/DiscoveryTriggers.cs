using System;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Fate.Epochs;
using Godland.Game.Life.Races;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Progression
{
    public interface IDiscoveryTrigger
    {
        string Key { get; }
    }

    public static class DiscoveryKeys
    {
        public static string Of(string kind, string id) => $"{kind}:{id}";
    }

    [Serializable]
    public sealed class CatastropheDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private CatastropheDefinition _catastrophe;
        public string Key => DiscoveryKeys.Of("catastrophe", _catastrophe.name);
    }

    [Serializable]
    public sealed class EpochDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private EpochDefinition _epoch;
        public string Key => DiscoveryKeys.Of("epoch", _epoch.name);
    }

    [Serializable]
    public sealed class EmergenceDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private EmergenceDefinition _emergence;
        public string Key => DiscoveryKeys.Of("emergence", _emergence.name);
    }

    [Serializable]
    public sealed class RaceDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private RaceDefinition _race;
        public string Key => DiscoveryKeys.Of("race", _race.name);
    }

    [Serializable]
    public sealed class TitanDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private TitanDefinition _titan;
        public string Key => DiscoveryKeys.Of("titan", _titan.Id);
    }

    [Serializable]
    public sealed class AnomalyDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private AnomalyDefinition _anomaly;
        public string Key => DiscoveryKeys.Of("anomaly", _anomaly.name);
    }

    [Serializable]
    public sealed class SovereignDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private SovereignDefinition _sovereign;
        public string Key => DiscoveryKeys.Of("sovereign", _sovereign.name);
    }

    [Serializable]
    public sealed class SagaActDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private Godland.Game.Fate.Sagas.SagaActDefinition _act;
        public string Key => DiscoveryKeys.Of("saga-act", _act.name);
    }

    [Serializable]
    public sealed class SignalDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, InfoBox("ridge — встал хребет супергор; wonder-age — Эпоха чудес.")] private string _signal;
        public string Key => DiscoveryKeys.Of("signal", _signal);
    }

    [Serializable]
    public sealed class PantheonDiscovery : IDiscoveryTrigger
    {
        [SerializeField, Required, AssetsOnly] private Godland.Game.Powers.Pantheons.PantheonDefinition _pantheon;
        public string Key => DiscoveryKeys.Of("pantheon", _pantheon.Id);
    }
}
