using System;
using System.Collections.Generic;
using Godland.Core.Life.Heroes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Heroes
{
    [Serializable]
    public sealed class HeroArchetypeDefinition : IHeroArchetype
    {
        [SerializeField, Required] private string _title;
        [SerializeField, MinValue(0), InfoBox("Номер эпохи в EpochConfig — только в ней рождаются такие герои.")] private int _epoch;
        [SerializeField] private bool _needsCity;
        [SerializeField, MinValue(1)] private int _minPopulation = 4;
        [SerializeField] private QuestKind _favoredQuest;
        [SerializeField, Range(0, 100), SuffixLabel("%")] private int _favoredBonusPercent = 20;
        [SerializeField, InfoBox("Идёт на чужих богов и без благословения.")] private bool _huntsGods;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private List<string> _names = new();

        public string Title => _title;
        public int Epoch => _epoch;
        public bool NeedsCity => _needsCity;
        public int MinPopulation => _minPopulation;
        public QuestKind FavoredQuest => _favoredQuest;
        public int FavoredBonusPercent => _favoredBonusPercent;
        public bool HuntsGods => _huntsGods;
        public Color Color => _color;
        public IReadOnlyList<string> Names => _names;
    }
}
