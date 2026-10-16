using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Dilemmas
{
    [CreateAssetMenu(menuName = "Godland/Dilemma Config", fileName = "DilemmaConfig")]
    public sealed class DilemmaConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Узлы"), InfoBox("Узловые моменты: гражданская война, предвестие катастрофы, священная война — время встаёт, писатель выбирает исход.")]
        public bool CrossroadsEnabled { get; private set; } = true;

        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int BackSideCreation { get; private set; } = 40;
        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int ReconcileFaith { get; private set; } = 50;
        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int DelayCreation { get; private set; } = 90;
        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int PrepareFaith { get; private set; } = 60;
        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int TakeSideFaith { get; private set; } = 40;
        [field: SerializeField, BoxGroup("Узлы"), MinValue(0)] public int PeaceCreation { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Сделки"), InfoBox("Силы мира (пантеоны, титаны, правитель мира) сами предлагают писателю сделку.")]
        public bool DealsEnabled { get; private set; } = true;

        [field: SerializeField, BoxGroup("Сделки"), MinValue(1), SuffixLabel("years")] public int DealPeriod { get; private set; } = 100;
        [field: SerializeField, BoxGroup("Сделки"), Range(0, 100), SuffixLabel("%")] public int DealChancePercent { get; private set; } = 50;
        [field: SerializeField, BoxGroup("Сделки"), MinValue(0)] public int DealFromEpoch { get; private set; } = 3;
        [field: SerializeField, BoxGroup("Сделки"), MinValue(0)] public int OathFaith { get; private set; } = 80;
        [field: SerializeField, BoxGroup("Сделки"), MinValue(0)] public int CityPayment { get; private set; } = 60;
        [field: SerializeField, BoxGroup("Сделки"), MinValue(0)] public int FeedTitanCreation { get; private set; } = 70;
        [field: SerializeField, BoxGroup("Сделки"), MinValue(0)] public int TitheFaith { get; private set; } = 80;
    }
}
