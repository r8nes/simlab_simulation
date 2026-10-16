using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Commandments
{
    [CreateAssetMenu(menuName = "Godland/Commandment Config", fileName = "CommandmentConfig")]
    public sealed class CommandmentConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), InfoBox("В начале забега игрок собирает одну заповедь из фрагментов: столько вариантов на каждый слот.")]
        public int OptionsPerSlot { get; private set; } = 3;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что среди вариантов будет усилитель.")]
        public int AmplifierChancePercent { get; private set; } = 50;

        [field: SerializeField, MinValue(1)] public int CommonWeight { get; private set; } = 6;
        [field: SerializeField, MinValue(1)] public int RareWeight { get; private set; } = 3;
        [field: SerializeField, MinValue(1)] public int LegendaryWeight { get; private set; } = 1;

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Как часто народ «молится».")]
        public int PrayerPeriod { get; private set; } = 25;

        [field: SerializeField, MinValue(1), InfoBox("Усилитель «во тьме»: во сколько раз сильнее.")]
        public int DarkMultiplier { get; private set; } = 3;

        [SerializeField, AssetsOnly] private List<CommandmentWord> _words = new();

        public IReadOnlyList<CommandmentWord> Words => _words;

        public int WeightOf(CommandmentRarity rarity) => rarity switch
        {
            CommandmentRarity.Rare => RareWeight,
            CommandmentRarity.Legendary => LegendaryWeight,
            _ => CommonWeight
        };
    }
}
