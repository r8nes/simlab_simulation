using System.Collections.Generic;
using Godland.Core.Life.Heroes;
using Godland.Game.Powers.Gods;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Heroes
{
    [CreateAssetMenu(menuName = "Godland/Hero Config", fileName = "HeroConfig")]
    public sealed class HeroConfig : ScriptableObject, IHeroRules
    {
        [field: SerializeField, BoxGroup("Birth"), MinValue(0), InfoBox("Номер эпохи, с которой рождаются герои. 4 — Античность.")]
        public int HeroesFromEpoch { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Birth"), MinValue(1), SuffixLabel("years")]
        public int BirthMinYears { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Birth"), MinValue(1), SuffixLabel("years")]
        public int BirthMaxYears { get; private set; } = 80;

        [field: SerializeField, BoxGroup("Birth"), MinValue(1), InfoBox("Герой рождается в самом большом городе, если в нём не меньше стольких жителей.")]
        public int HeroMinPopulation { get; private set; } = 6;

        [field: SerializeField, BoxGroup("Quest"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период герой делает шаг к ближайшему чудовищу, Источнику порчи или чужому богу.")]
        public int MoveYears { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Quest"), Range(0, 100), SuffixLabel("%")]
        public int SuccessChancePercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Quest"), Range(0, 100), SuffixLabel("%"), InfoBox("Прибавка к удаче героя, благословлённого свыше. Только благословлённый идёт на бога чужой веры.")]
        public int BlessedBonusPercent { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Quest"), Range(0, 100), SuffixLabel("%"), InfoBox("Черта «Отважный»: прибавка к удаче (за каждого отважного в отряде) и к дуэлям.")]
        public int BraveBonusPercent { get; private set; } = 15;

        [field: SerializeField, BoxGroup("Quest"), Range(0, 100), SuffixLabel("%"), InfoBox("Прибавка к удаче за каждого союзника в отряде.")]
        public int AllyBonusPercent { get; private set; } = 20;

        [field: SerializeField, BoxGroup("Quest"), MinValue(0), InfoBox("Слава за победу в дуэли — прибавка к удаче в процентах.")]
        public int GloryPerVictory { get; private set; } = 10;


        [field: SerializeField, BoxGroup("Fiasco"), Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что поражение станет грандиозным провалом: чудовище заражает округу, Источник порчи взрывается, бог впадает в ярость.")]
        public int FiascoChancePercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Fiasco"), Range(0, 100), SuffixLabel("%"), InfoBox("Черта «Безрассудный»: прибавка к шансу провала. Безрассудные идут на самые сильные цели.")]
        public int RecklessFiascoPercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Fiasco"), Range(0, 100), SuffixLabel("%"), InfoBox("Черта «Мудрый»: снижение шанса провала.")]
        public int WiseFiascoPercent { get; private set; } = 15;

        [field: SerializeField, BoxGroup("Fiasco"), MinValue(1), InfoBox("Сколько клеток вокруг цели задевает провал.")]
        public int FiascoSpread { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Meetings"), Range(0, 100), SuffixLabel("%"), InfoBox("Встретив героя чужой веры, герой может договориться (а если их царства воюют — заключить мир). Гордые не договариваются и ищут дуэлей; герои одной веры или одного царства объединяются (кроме гордых).")]
        public int TreatyChancePercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Meetings"), Range(0, 100), SuffixLabel("%"), InfoBox("Прибавка к договору, если среди них есть мудрый, и ещё столько же, если есть благословлённый.")]
        public int WiseTreatyPercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Meetings"), MinValue(1), InfoBox("На каком расстоянии (в клетках) герои встречаются. Герой без цели идёт навстречу ближайшему герою.")]
        public int MeetDistance { get; private set; } = 3;



        [field: SerializeField, BoxGroup("Meetings"), Range(0, 100), SuffixLabel("%")]
        public int DuelChancePercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Outcome"), MinValue(0), InfoBox("Легенда: столько жителей прибавляется в родном городе.")]
        public int LegendPopulation { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Outcome"), MinValue(0), InfoBox("Трагедия: столько жителей теряет родной город.")]
        public int TragedyLoss { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Outcome"), MinValue(1), SuffixLabel("years"), InfoBox("Герой без цели уходит на покой в этом возрасте.")]
        public int MaxAgeYears { get; private set; } = 150;

        [field: SerializeField, BoxGroup("View"), Required, AssetsOnly]
        public GodAvatarView AvatarPrefab { get; private set; }

        [field: SerializeField, BoxGroup("View")]
        public Color Color { get; private set; } = new(1f, 0.85f, 0.35f);

        [field: SerializeField, BoxGroup("View"), MinValue(0.1f)]
        public float AvatarScale { get; private set; } = 0.6f;

        [SerializeField, BoxGroup("Archetypes"), InfoBox("Герои своей эпохи: рождаются только в ней и уходят в легенды, когда она кончается.")]
        private List<HeroArchetypeDefinition> _archetypes = new();

        public IReadOnlyList<IHeroArchetype> Archetypes => _archetypes;

        [field: SerializeField, BoxGroup("Demigods"), Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что герой после легенды станет полубогом (+3% за каждую славу): он не стареет, остаётся в мире и ищет новые подвиги.")]
        public int DemigodChancePercent { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Demigods"), Range(0, 100), SuffixLabel("%")] public int DemigodBonusPercent { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Quest"), Range(0, 100), SuffixLabel("%"), InfoBox("Насколько труднее герою победить бога пантеона: за него стоит весь пантеон.")] public int PantheonGuardPercent { get; private set; } = 25;
    }
}
