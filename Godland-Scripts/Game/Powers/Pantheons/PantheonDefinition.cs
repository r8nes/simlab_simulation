using System;
using System.Collections.Generic;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    public enum PantheonDeedSite
    {
        Seat,
        Follower,
        Rival
    }

    [Serializable]
    public sealed class PantheonGodDefinition
    {
        [SerializeField, Required] private string _name;
        [SerializeField] private string _epithet;
        [SerializeField, Required, InfoBox("Id тела бога в GodsConfig (нестихийное): storm, harvest, sea, death, war, sun, chaos, wisdom, love, underworld, winter, wild.")] private string _body;
        [SerializeReference, InfoBox("Когда мир рождает этого бога (у главы пусто).")] private ICatastropheTrigger _birth;

        public string Name => _name;
        public string FullName => string.IsNullOrEmpty(_epithet) ? _name : $"{_name}, {_epithet}";
        public string Body => _body;
        public ICatastropheTrigger Birth => _birth;
    }

    [Serializable]
    public sealed class PantheonDeed
    {
        [SerializeField, Required] private string _name;
        [SerializeField, Range(1, 10), InfoBox("Нрав: насколько пантеону по душе это деяние.")] private int _nature = 3;
        [SerializeField] private PantheonDeedSite _site;
        [SerializeReference] private List<IEmergenceOutcome> _outcomes = new();

        public string Name => _name;
        public int Nature => _nature;
        public PantheonDeedSite Site => _site;
        public IReadOnlyList<IEmergenceOutcome> Outcomes => _outcomes;
    }

    [Serializable]
    public sealed class GodScheme
    {
        [SerializeField, InfoBox("Чей это замысел (имя бога пантеона). Пусто — любой бог пантеона.")] private string _god;
        [SerializeField, Required, InfoBox("Факт, к которому бог стремится.")] private string _goal;
        [SerializeField, Range(1, 10)] private int _weight = 3;
        [SerializeField, InfoBox("Можно стремиться снова, когда цель достигнута.")] private bool _repeatable;

        public string God => _god;
        public string Goal => _goal;
        public int Weight => _weight;
        public bool Repeatable => _repeatable;
    }

    [Serializable]
    public sealed class GodAction
    {
        [SerializeField, Required] private string _name;
        [SerializeField, InfoBox("Кто может это сделать (имя бога). Пусто — любой бог пантеона.")] private string _god;
        [SerializeField, TextArea, InfoBox("Что видит мир: журнал и летопись.")] private string _text;
        [SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Бог должен прожить столько лет.")] private int _minGodAge;
        [SerializeReference, InfoBox("Состояние мира, без которого действие невозможно (пусто — всегда).")] private ICatastropheTrigger _when;
        [SerializeField, InfoBox("Факты пантеона, которые нужны.")] private List<string> _needs = new();
        [SerializeField, InfoBox("Факты, которые появятся.")] private List<string> _adds = new();
        [SerializeField] private List<string> _removes = new();
        [SerializeField, MinValue(1)] private int _cost = 1;
        [SerializeField] private PantheonDeedSite _site = PantheonDeedSite.Follower;
        [SerializeReference] private List<IEmergenceOutcome> _outcomes = new();
        [SerializeField, InfoBox("Полубог рождается от этого действия (имя).")] private string _demigod;
        [SerializeField, AssetsOnly, InfoBox("Действие открывает сагу.")] private Godland.Game.Fate.Sagas.SagaDefinition _saga;

        public string Name => _name;
        public string God => _god;
        public string Text => _text;
        public int MinGodAge => _minGodAge;
        public ICatastropheTrigger When => _when;
        public IReadOnlyList<string> Needs => _needs;
        public IReadOnlyList<string> Adds => _adds;
        public IReadOnlyList<string> Removes => _removes;
        public int Cost => _cost;
        public PantheonDeedSite Site => _site;
        public IReadOnlyList<IEmergenceOutcome> Outcomes => _outcomes;
        public string Demigod => _demigod;
        public Godland.Game.Fate.Sagas.SagaDefinition Saga => _saga;
    }

    [CreateAssetMenu(menuName = "Godland/Pantheon", fileName = "Pantheon")]
    public sealed class PantheonDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string Id { get; private set; }
        [field: SerializeField, Required] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Text { get; private set; }
        [field: SerializeField] public Color Color { get; private set; } = Color.white;
        [field: SerializeField, MinValue(0)] public int MinEpoch { get; private set; } = 3;
        [field: SerializeField, Range(1, 100), SuffixLabel("%")] public int ArrivalChancePercent { get; private set; } = 25;
        [field: SerializeField, Range(0, 100), InfoBox("Воинственность: чем выше, тем быстрее портятся отношения с соседями.")] public int Aggression { get; private set; } = 50;

        [SerializeReference, Required, InfoBox("Когда земля зовёт этот пантеон.")] private ICatastropheTrigger _arrival;
        [SerializeField, AssetsOnly, InfoBox("Раскрытый идол из этого списка сразу приводит пантеон: его верующие переходят к главе.")] private List<IdolDefinition> _idols = new();
        [SerializeField] private PantheonGodDefinition _head = new();
        [SerializeField] private List<PantheonGodDefinition> _children = new();
        [SerializeField] private List<PantheonDeed> _deeds = new();
        [SerializeField, BoxGroup("Замыслы"), InfoBox("Цели богов: планировщик строит цепочку действий к факту-цели.")] private List<GodScheme> _schemes = new();
        [SerializeField, BoxGroup("Замыслы")] private List<GodAction> _actions = new();

        public ICatastropheTrigger Arrival => _arrival;
        public IReadOnlyList<IdolDefinition> Idols => _idols;
        public PantheonGodDefinition Head => _head;
        public IReadOnlyList<PantheonGodDefinition> Children => _children;
        public IReadOnlyList<PantheonDeed> Deeds => _deeds;
        public IReadOnlyList<GodScheme> Schemes => _schemes;
        public IReadOnlyList<GodAction> Actions => _actions;
    }
}
