using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Fate.Catastrophes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    public interface ISagaCondition
    {
        bool Holds(SagaKeeper saga);
        string Describe(SagaKeeper saga);
        SagaFaction Favors(SagaKeeper saga) => null;
    }

    [Serializable]
    public sealed class FactionPowerCondition : ISagaCondition
    {
        [SerializeField, Required, AssetsOnly] private SagaFactionDefinition _faction;
        [SerializeField, Range(0, 100)] private int _atLeast = 75;

        public bool Holds(SagaKeeper saga) => saga.PowerOf(_faction) >= _atLeast;
        public string Describe(SagaKeeper saga) => $"сила стороны «{_faction.DisplayName}» {saga.PowerOf(_faction)} из {_atLeast}";
        public SagaFaction Favors(SagaKeeper saga) => saga.FactionOf(_faction);
        public int Threshold => _atLeast;
    }

    [Serializable]
    public sealed class SidePowerCondition : ISagaCondition
    {
        [SerializeField, MinValue(0), InfoBox("0 — первая сторона акта, 1 — вторая.")] private int _side;
        [SerializeField, Range(0, 100)] private int _atLeast = 80;

        public bool Holds(SagaKeeper saga) => saga.Factions.Count > _side && saga.Factions[_side].Power >= _atLeast;
        public string Describe(SagaKeeper saga) =>
            saga.Factions.Count > _side ? $"сила стороны «{saga.Factions[_side].Definition.DisplayName}» {saga.Factions[_side].Power} из {_atLeast}" : "стороны нет";
        public SagaFaction Favors(SagaKeeper saga) => saga.Factions.Count > _side ? saga.Factions[_side] : null;
        public int Threshold => _atLeast;
    }

    [Serializable]
    public sealed class ArtifactCondition : ISagaCondition
    {
        [SerializeField, Required, AssetsOnly] private SagaFactionDefinition _faction;

        public bool Holds(SagaKeeper saga) => saga.HasArtifact(_faction);
        public string Describe(SagaKeeper saga) => $"артефакт у стороны «{_faction.DisplayName}»: {(saga.HasArtifact(_faction) ? "найден" : "нет")}";
        public SagaFaction Favors(SagaKeeper saga) => saga.FactionOf(_faction);
    }

    [Serializable]
    public sealed class ActAgeCondition : ISagaCondition
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 150;

        public bool Holds(SagaKeeper saga) => saga.ActAge >= _years;
        public string Describe(SagaKeeper saga) => $"акт длится {saga.ActAge} из {_years} лет";
    }

    [Serializable]
    public sealed class WorldCondition : ISagaCondition
    {
        [SerializeField, InfoBox("Состояние мира, как у катастроф и веток эпох.")] private string _description;
        [SerializeReference, Required] private ICatastropheTrigger _condition;
        [SerializeField, InfoBox("Выполнено, когда условие НЕ держится.")] private bool _negate;

        public bool Holds(SagaKeeper saga) => _condition.IsReady(saga.World) != _negate;
        public string Describe(SagaKeeper saga) => $"{_description}: {(Holds(saga) ? "да" : "нет")}";
    }

    [Serializable]
    public sealed class AllCondition : ISagaCondition
    {
        [SerializeReference] private List<ISagaCondition> _all = new();

        public bool Holds(SagaKeeper saga) => _all.All(condition => condition.Holds(saga));
        public string Describe(SagaKeeper saga) => string.Join("; ", _all.Select(condition => condition.Describe(saga)));
        public SagaFaction Favors(SagaKeeper saga) => _all.Select(condition => condition.Favors(saga)).FirstOrDefault(faction => faction != null);
    }
}
