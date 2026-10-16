using System.Collections.Generic;
using Godland.Core.Life.Kingdoms;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Kingdoms
{
    [CreateAssetMenu(menuName = "Godland/Kingdom Config", fileName = "KingdomConfig")]
    public sealed class KingdomConfig : ScriptableObject, IKingdomRules
    {
        [field: SerializeField, BoxGroup("Form"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период город и связанные с ним поселения его веры объединяются в царство; царства без города или с расколом веры распадаются.")]
        public int FormPeriod { get; private set; } = 20;

        [field: SerializeField, BoxGroup("Form"), MinValue(1), InfoBox("Сколько поселений (вместе со столицей) нужно царству.")]
        public int MinMembers { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Form"), MinValue(1), InfoBox("Ниже этого числа поселений царство начинает распадаться.")]
        public int SurviveMembers { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Form"), MinValue(1), InfoBox("Сколько проверок подряд царство может быть слишком маленьким, прежде чем распадётся. Потерянная столица переезжает в другой город той же веры; без него — тоже проверка.")]
        public int DissolveStrikes { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Form"), MinValue(1), SuffixLabel("years"), InfoBox("Каждые столько лет жизни царству прощается ещё одна плохая проверка: старые царства устойчивее.")]
        public int StabilityYears { get; private set; } = 60;

        [field: SerializeField, BoxGroup("War"), MinValue(1), SuffixLabel("years"), InfoBox("Соседние царства разной веры воюют вместо мелких набегов.")]
        public int WarCheckPeriod { get; private set; } = 20;

        [field: SerializeField, BoxGroup("War"), Range(0, 100), SuffixLabel("%")]
        public int WarChancePercent { get; private set; } = 40;

        [field: SerializeField, BoxGroup("War"), MinValue(1), SuffixLabel("years")]
        public int WarMinYears { get; private set; } = 20;

        [field: SerializeField, BoxGroup("War"), MinValue(1), SuffixLabel("years")]
        public int WarMaxYears { get; private set; } = 40;

        [field: SerializeField, BoxGroup("War"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период сильнейшая сторона берёт одно приграничное поселение врага (теряет жителей и принимает веру победителя). Взятие столицы уничтожает царство.")]
        public int BattlePeriod { get; private set; } = 10;

        [field: SerializeField, BoxGroup("War"), MinValue(0)]
        public int BattlePopulationLoss { get; private set; } = 1;

        [SerializeField, BoxGroup("View"), InfoBox("Цвета границ царств на сетке, по кругу.")]
        private List<Color> _colors = new()
        {
            new Color(0.95f, 0.35f, 0.3f), new Color(0.3f, 0.6f, 1f), new Color(0.95f, 0.8f, 0.25f), new Color(0.6f, 0.4f, 0.95f),
            new Color(0.3f, 0.85f, 0.5f), new Color(1f, 0.55f, 0.2f), new Color(0.9f, 0.4f, 0.75f), new Color(0.35f, 0.85f, 0.85f)
        };

        [SerializeField, BoxGroup("Names")]
        private List<string> _names = new();

        public IReadOnlyList<string> Names => _names;

        public Color ColorOf(int index) => _colors.Count == 0 ? Color.white : _colors[index % _colors.Count];
    }
}
