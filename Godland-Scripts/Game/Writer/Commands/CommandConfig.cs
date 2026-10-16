using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Commands
{
    [Serializable]
    public sealed class CommandDefinition
    {
        [SerializeField, Required] private string _id;
        [SerializeField, Required] private string _title;
        [SerializeField, TextArea] private string _description;
        [SerializeField, MinValue(0)] private int _creationCost;
        [SerializeField, MinValue(0)] private int _faithCost;
        [SerializeField, InfoBox("Что мир запоминает в месте приказа (кроме command:<id>). Пусто — ничего.")] private string _memoryKey;

        public string Id => _id;
        public string Title => _title;
        public string Description => _description;
        public int CreationCost => _creationCost;
        public int FaithCost => _faithCost;
        public string MemoryKey => _memoryKey;
    }

    [CreateAssetMenu(menuName = "Godland/Command Config", fileName = "CommandConfig")]
    public sealed class CommandConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(0), SuffixLabel("%"), InfoBox("Насколько дорожает приказ с каждой эпохой.")]
        public int CostPerEpochPercent { get; private set; } = 20;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Перезарядка: одной сущности можно приказывать раз в столько лет.")]
        public int CooldownYears { get; private set; } = 50;

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int GodSilenceYears { get; private set; } = 100;

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int TitanSleepYears { get; private set; } = 150;

        [field: SerializeField, MinValue(1)]
        public int FervorStep { get; private set; } = 20;

        [field: SerializeField, MinValue(1)]
        public int SagaPush { get; private set; } = 5;

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int CatastropheDelayYears { get; private set; } = 100;

        [SerializeField, ListDrawerSettings(ShowFoldout = true)] private List<CommandDefinition> _commands = new();

        public CommandDefinition Find(string id) => _commands.FirstOrDefault(command => command.Id == id);
    }
}
