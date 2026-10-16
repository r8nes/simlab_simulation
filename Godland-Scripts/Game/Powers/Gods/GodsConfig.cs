using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Gods;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Gods
{
    [CreateAssetMenu(menuName = "Godland/Gods Config", fileName = "GodsConfig")]
    public sealed class GodsConfig : ScriptableObject, IGodRules
    {
        [field: SerializeField, MinValue(0), InfoBox("С какой эпохи (индекс) рождаются боги и угасают идолы.")]
        public int GodsFromEpoch { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Birth"), MinValue(1), SuffixLabel("years")]
        public int BirthPeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Birth"), MinValue(1), InfoBox("Пантеон выбирается из давления мира: у каждой стихии давление = её тайлы + жители вокруг них. Раз в период рождается бог самой давящей стихии, пока богов меньше этого числа.")]
        public int MaxGods { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Birth"), MinValue(0), InfoBox("Сколько давления добавляет каждое поселение на тайле стихии или рядом с ним.")]
        public int PressurePerSettlement { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Birth"), MinValue(100), SuffixLabel("%"), InfoBox("Если пантеон полон, а давление новой стихии больше давления слабейшего бога на столько процентов — слабейший забыт и уступает место.")]
        public int DisplacePercent { get; private set; } = 200;

        [field: SerializeField, BoxGroup("Calling"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период бог призывает поселения вокруг престола.")]
        public int CallPeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Calling"), MinValue(1)]
        public int CallRadius { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Calling"), Range(0, 100), SuffixLabel("%")]
        public int CallChancePercent { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Idols"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период слабые идолы угасают: теряют пыл и верующих.")]
        public int FadePeriod { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Idols"), MinValue(0)]
        public int FadeFervor { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Idols"), Range(0, 100), SuffixLabel("%")]
        public int FadeChancePercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Idols"), MinValue(1), InfoBox("Идол с таким пылом не угасает, не отдаёт верующих богам и воюет с их верой.")]
        public int ResistFervor { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Temples"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период город, верящий в бога, строит ему храм на соседнем тайле его стихии. Каждый храм усиливает деяния бога: +1 тайл, +1 к радиусу за каждые 2 храма.")]
        public int TemplePeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Temples"), MinValue(1), InfoBox("Сколько периодов подряд бог может прожить без храмов, пока в мире есть города; потом он забыт.")]
        public int NeglectLimit { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Temples"), MinValue(0), SuffixLabel("years"), InfoBox("Забытый или павший бог не рождается снова столько лет.")]
        public int RebirthCooldown { get; private set; } = 300;

        [field: SerializeField, BoxGroup("Temples"), MinValue(0), InfoBox("Сколько жителей теряет город, когда его храм осквернён.")]
        public int TempleDesecrationLoss { get; private set; } = 1;

        [field: SerializeField, MinValue(1), InfoBox("Сколько храмов может быть у одного бога.")]
        public int MaxTemplesPerGod { get; private set; } = 6;

        [field: SerializeField, BoxGroup("View"), Required, AssetsOnly]
        public GodAvatarView AvatarPrefab { get; private set; }

        [SerializeField]
        private List<GodDefinition> _gods = new();

        private IReadOnlyList<GodRule> _rules;

        public IReadOnlyList<GodRule> Gods => _rules ??= _gods.Select(god => god.ToCore()).ToList();

        public GodDefinition DefinitionOf(string id) => _gods.First(god => god.Id == id);

        public string NameOf(God god) => god.PantheonId != null ? god.HeroName : god.HeroName != null ? $"{god.HeroName}, вознесённый герой" : DefinitionOf(god.Rule.Id).FullName;
    }
}
