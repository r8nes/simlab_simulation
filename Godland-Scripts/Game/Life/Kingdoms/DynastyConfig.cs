using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Kingdoms
{
    [CreateAssetMenu(menuName = "Godland/Dynasty Config", fileName = "DynastyConfig")]
    public sealed class DynastyConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int RulePeriod { get; private set; } = 20;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int MinLifespan { get; private set; } = 40;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int MaxLifespan { get; private set; } = 90;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс кризиса престолонаследия: царство раскалывается гражданской войной.")] public int CrisisPercent { get; private set; } = 20;
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int TyrantCrisisBonus { get; private set; } = 25;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Мудрый правитель оставляет спокойный трон: шанс спора меньше на столько.")] public int WiseCalm { get; private set; } = 10;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Отречение законнее смерти: шанс спора меньше на столько.")] public int AbdicationCalm { get; private set; } = 5;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Если наследника не признали — с этим шансом трон захватывает претендент из другого дома, иначе мятежники откалываются.")] public int UsurpPercent { get; private set; } = 50;
        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("С какой доли отпущенной жизни правитель может отречься.")] public int OldAgePercent { get; private set; } = 70;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс отречения за период для старого правителя (тиран не отрекается).")] public int AbdicatePercent { get; private set; } = 10;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Мирные нравы (не завоеватель) отрекаются охотнее.")] public int CalmAbdicateBonus { get; private set; } = 15;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс за период, что Строитель возводит чудо.")] public int WonderPercent { get; private set; } = 25;
        [field: SerializeField, MinValue(1)] public int WonderCapitalPopulation { get; private set; } = 10;
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int TyrantRevoltPercent { get; private set; } = 30;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int SpecialtyPeriod { get; private set; } = 30;
        [SerializeField] private List<string> _names = new();
        [SerializeField] private List<string> _dynasties = new();
        [SerializeField] private List<WonderDefinition> _wonders = new();

        public IReadOnlyList<string> Names => _names;
        public IReadOnlyList<string> DynastyNames => _dynasties;
        public IReadOnlyList<WonderDefinition> Wonders => _wonders;
    }
}
