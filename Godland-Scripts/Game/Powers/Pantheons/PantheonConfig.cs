using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    [CreateAssetMenu(menuName = "Godland/Pantheon Config", fileName = "PantheonConfig")]
    public sealed class PantheonConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int CheckPeriod { get; private set; } = 25;
        [field: SerializeField, MinValue(1)] public int MaxPantheons { get; private set; } = 3;
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период пантеон может родить одного бога, если мир дал для него повод.")] public int BirthPeriod { get; private set; } = 40;
        [field: SerializeField, MinValue(1)] public int MaxGodsPerPantheon { get; private set; } = 5;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int DeedPeriod { get; private set; } = 50;
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период каждый бог делает следующий шаг своего замысла.")] public int SchemePeriod { get; private set; } = 30;
        [field: SerializeField, Range(1, 6), InfoBox("Самая длинная цепочка действий, которую бог способен задумать.")] public int PlanDepth { get; private set; } = 4;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int RelationPeriod { get; private set; } = 40;
        [field: SerializeField, Range(-100, 0), InfoBox("Ниже этого отношения пантеоны ведут священную войну.")] public int WarThreshold { get; private set; } = -40;
        [field: SerializeField, Range(0, 100), InfoBox("Выше этого отношения пантеоны в союзе.")] public int AllianceThreshold { get; private set; } = 40;
        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Союзник слабее этой доли растворяется в сильном (как римляне приняли греческих богов).")] public int AbsorbPercent { get; private set; } = 30;
        [field: SerializeField, MinValue(0)] public int HolyWarLoss { get; private set; } = 1;
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int HolyWarConversionPercent { get; private set; } = 30;
        [field: SerializeField, MinValue(1), InfoBox("На сколько приказ писателя сдвигает отношения пантеонов.")] public int CommandRelation { get; private set; } = 30;
        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Угасший пантеон не возвращается столько лет.")] public int SleepYears { get; private set; } = 300;

        [SerializeField, AssetsOnly] private List<PantheonDefinition> _pantheons = new();

        [field: SerializeField, AssetsOnly, InfoBox("Замыслы стихийных богов, титанов и правителей мира.")] public SchemeBook WorldBook { get; private set; }

        public IReadOnlyList<PantheonDefinition> Pantheons => _pantheons;
    }
}
