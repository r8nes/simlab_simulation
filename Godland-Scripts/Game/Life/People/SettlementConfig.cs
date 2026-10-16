using System.Collections.Generic;
using System.Linq;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Godland.Game.Life.People
{
    [CreateAssetMenu(menuName = "Godland/Settlement Config", fileName = "SettlementConfig")]
    public sealed class SettlementConfig : ScriptableObject, ISettlementRules, IShrineRules, IFeudRules, ICataclysmRules, IPlagueRules, IGodfighterRules
    {
        [SerializeField, BoxGroup("Where"), AssetsOnly, InfoBox("Жители появляются на любом тайле, кроме этих.")]
        private List<TileDefinition> _uninhabitableTiles = new();

        [SerializeField, BoxGroup("Food"), AssetsOnly, InfoBox("Тайлы, которые кормят жителей любой культуры рядом (и на самом тайле), например Житница.")]
        private List<TileDefinition> _sharedFoodTiles = new();

        [field: SerializeField, BoxGroup("Food"), MinValue(1), InfoBox("Сколько еды считается за каждый такой тайл.")]
        public int SharedFoodValue { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Where"), MinValue(0), InfoBox("Только для тайлов с плодородием.")]
        public int MinFertility { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Timing"), MinValue(1), SuffixLabel("years")]
        public int SpawnPeriod { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Timing"), MinValue(0), InfoBox("Номер эпохи в EpochConfig (0 — первая), с которой появляются жители. 1 — Каменный век.")]
        public int SpawnFromEpoch { get; private set; } = 1;

        [field: SerializeField, BoxGroup("Timing"), MinValue(1), SuffixLabel("years")]
        public int GrowthPeriod { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Timing"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в этот период каждое поселение даёт веру: стоянка 1, деревня 2 (× множитель культуры).")]
        public int FaithPeriod { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Population"), MinValue(1)]
        public int StartPopulation { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Population"), MinValue(1)]
        public int BaseCapacity { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Population"), MinValue(0)]
        public int CapacityPerFood { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Energy"), MinValue(1), InfoBox("Бюджет энергии (DEB): за период поселение получает энергию от еды (вместимость × это), тратит на жителей; излишек рождает людей, долг морит голодом. Так бумы и голод возникают сами.")]
        public int EnergyPerCapacity { get; private set; } = 10;
        [field: SerializeField, BoxGroup("Energy"), MinValue(1)] public int UpkeepPerPerson { get; private set; } = 10;
        [field: SerializeField, BoxGroup("Energy"), MinValue(1)] public int BirthCost { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Energy"), MinValue(1)] public int StarveDebt { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Energy"), MinValue(0), InfoBox("Сколько запаса поселение может скопить: чем больше, тем сильнее бум и тем больнее голод после.")]
        public int MaxReserve { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Population"), MinValue(1)]
        public int VillagePopulation { get; private set; } = 6;

        [field: SerializeField, BoxGroup("Migration"), MinValue(2)]
        public int MigrationMinPopulation { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Migration"), MinValue(1)]
        public int MigrationSize { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Hunting"), MinValue(1), SuffixLabel("years")]
        public int HuntPeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Migration"), MinValue(2), InfoBox("Если поселение должно опустеть от удара, сосед с таким числом жителей отдаёт ему одного — поселение выживает.")]
        public int RescueMinPopulation { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Migration"), Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что сосед действительно придёт на помощь.")]
        public int RescueChancePercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Migration"), MinValue(1), SuffixLabel("years"), InfoBox("В «Мире без смерти»: не чаще раза в этот период люди уходят за край мира и открывают новую клетку.")]
        public int ExodusPeriod { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Hunting"), MinValue(1)]
        public int HuntMinPopulation { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Hunting"), MinValue(1), SuffixLabel("years")]
        public int HuntYears { get; private set; } = 20;

        [field: SerializeField, BoxGroup("Shrines"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период поселение рядом со священным для его культуры тайлом может объявить его своим богом.")]
        public int ShrinePeriod { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Shrines"), Range(0, 100), SuffixLabel("%")]
        public int ShrineChancePercent { get; private set; } = 30;

        [field: SerializeField, FormerlySerializedAs("<ShrineFaithPerPerson>k__BackingField"), BoxGroup("Shrines"), MinValue(0), InfoBox("Дополнительная вера за период с каждого поселения, у которого есть бог.")]
        public int ShrineFaith { get; private set; } = 1;

        [field: SerializeField, BoxGroup("Shrines"), MinValue(0), InfoBox("Сколько жителей теряет поселение, если его святыню изменили или убрали.")]
        public int DesecrationPopulationLoss { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Shrines"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период поселение без бога может принять бога соседнего поселения. Переселенцы всегда уносят бога с собой.")]
        public int ConversionPeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Shrines"), Range(0, 100), SuffixLabel("%")]
        public int ConversionChancePercent { get; private set; } = 30;

        [SerializeField, BoxGroup("Religions"), AssetsOnly, InfoBox("Догматы богов-тайлов. Тайл без догмата получает догмат по умолчанию.")]
        private List<DogmaDefinition> _dogmas = new();

        [SerializeField, BoxGroup("Religions"), Required, AssetsOnly]
        private DogmaDefinition _defaultDogma;


        [field: SerializeField, BoxGroup("Religions"), MinValue(1), SuffixLabel("years"), InfoBox("Как часто большие религии бросают кубик раскола.")]
        public int SchismPeriod { get; private set; } = 100;

        [field: SerializeField, BoxGroup("Religions"), MinValue(2)]
        public int SchismMinFollowers { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Religions"), Range(0, 100), SuffixLabel("%")]
        public int SchismChancePercent { get; private set; } = 25;

        [SerializeField, BoxGroup("Idols"), AssetsOnly, InfoBox("Конкретные идолы. Когда пыл религии достигает порога, из подходящих по обстоятельствам бросается кубик по весам.")]
        private List<IdolDefinition> _idols = new();

        [field: SerializeField, BoxGroup("Idols"), MinValue(1), InfoBox("Пыл копится раз в период веры: число поселений + бонус догмата.")]
        public int IdolRevealFervor { get; private set; } = 15;

        [field: SerializeField, BoxGroup("Idols"), MinValue(0), InfoBox("Сколько пыла теряет религия идола, когда Творец творит чудо на её поселении.")]
        public int MiracleFervorLoss { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Feuds"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период соседние поселения с разными богами копят вражду (+1).")]
        public int FeudPeriod { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Feuds"), MinValue(2), InfoBox("На этой вражде случается набег; с половины — значок вражды.")]
        public int RaidTension { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Feuds"), MinValue(0)]
        public int RaidPopulationLoss { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Feuds"), Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что побеждённые примут бога победителей.")]
        public int ConquestChancePercent { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Cataclysms"), MinValue(1), InfoBox("Сколько поселений в мире должно верить хоть во что-то, чтобы идол мог призывать.")]
        public int MinWorldBelievers { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Cataclysms"), Range(1, 100), SuffixLabel("%"), InfoBox("Какую долю мировой веры должен держать идол.")]
        public int DominancePercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Cataclysms"), MinValue(1), InfoBox("Пыл для призыва. Если пыл падает ниже половины или доля теряется — призыв срывается.")]
        public int SummonFervor { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Cataclysms"), MinValue(1), SuffixLabel("years")]
        public int WarningYears { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Cataclysms"), MinValue(0), InfoBox("Сколько жителей теряет поселение на тайле, изменённом катаклизмом.")]
        public int PopulationLoss { get; private set; } = 3;

        [SerializeField, BoxGroup("Cataclysms"), AssetsOnly, InfoBox("Эти тайлы катаклизмы не трогают.")]
        private List<TileDefinition> _cataclysmImmune = new();

        [field: SerializeField, BoxGroup("Plague"), MinValue(1), SuffixLabel("years"), InfoBox("Мор не появляется просто так: раз в период бросается кубик только у поселений с факторами риска (теснота, голод, гнилые земли рядом, недавняя война).")]
        public int PlagueCheckPeriod { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Plague"), MinValue(1)]
        public int PlagueMinRiskFactors { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Plague"), Range(0, 100), SuffixLabel("%")]
        public int PlagueChancePerFactorPercent { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Plague"), MinValue(1), SuffixLabel("years")]
        public int PlagueDuration { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Plague"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период больное поселение теряет жителя и заражает соседей.")]
        public int PlagueTickPeriod { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Plague"), Range(0, 100), SuffixLabel("%")]
        public int PlagueSpreadChancePercent { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Plague"), MinValue(0), SuffixLabel("years")]
        public int PlagueImmunityYears { get; private set; } = 100;

        [field: SerializeField, BoxGroup("Plague"), MinValue(0), SuffixLabel("years")]
        public int RecentRaidYears { get; private set; } = 30;

        [SerializeField, BoxGroup("Plague"), AssetsOnly, InfoBox("Тайлы, рядом с которыми зарождается мор.")]
        private List<TileDefinition> _diseaseTiles = new();

        public bool BreedsDisease(string tileId) => _diseaseTiles.Any(tile => tile.Id == tileId);

        [field: SerializeField, BoxGroup("Godfighters"), MinValue(1), InfoBox("Сколько недавних чудес на поселении злят его жителей до бунта.")]
        public int IreThreshold { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Godfighters"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период гнев спадает на 1.")]
        public int IreDecayPeriod { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Godfighters"), Range(0, 100), SuffixLabel("%")]
        public int RevoltChancePercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Godfighters"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период богоборцы рушат соседние святыни и подбивают соседей.")]
        public int GodfighterPeriod { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Godfighters"), Range(0, 100), SuffixLabel("%")]
        public int GodfighterSpreadChancePercent { get; private set; } = 20;

        public bool IsImmune(string tileId) => _cataclysmImmune.Any(tile => tile.Id == tileId);

        public Cataclysm CataclysmOf(string idolId) => IdolById(idolId).ToCataclysm();

        public IReadOnlyList<IdolRule> Idols => _idols.Select(idol => idol.ToCore()).ToList();

        public IdolDefinition IdolById(string id) => _idols.First(idol => idol.Id == id);

        public Dogma DogmaFor(string godTileId) => DogmaDefinitionFor(godTileId).ToCore();

        public Dogma DefaultDogma => _defaultDogma.ToCore();

        public IReadOnlyList<Dogma> HeresyDogmas => _dogmas.Append(_defaultDogma).Select(dogma => dogma.ToCore()).ToList();

        public DogmaDefinition DogmaDefinitionFor(string godTileId) =>
            _dogmas.FirstOrDefault(dogma => dogma.BelongsTo(godTileId)) ?? _defaultDogma;

        public DogmaDefinition DogmaById(string id) =>
            _dogmas.Append(_defaultDogma).First(dogma => dogma.Id == id);

        [field: SerializeField, BoxGroup("Cultures"), Required, AssetsOnly]
        public CultureDefinition DefaultCulture { get; private set; }

        [SerializeField, BoxGroup("Cultures"), AssetsOnly]
        private List<CultureDefinition> _cultures = new();

        [field: SerializeField, BoxGroup("View"), Required, AssetsOnly]
        public SettlementView SettlementPrefab { get; private set; }

        public bool CanHost(string tileId) => _uninhabitableTiles.All(tile => tile.Id != tileId);

        public SettlementCulture GetCulture(string tileId) => CultureFor(tileId).ToCore();

        int ISettlementRules.SharedFoodValue(string tileId) => _sharedFoodTiles.Any(tile => tile.Id == tileId) ? SharedFoodValue : 0;

        public CultureDefinition CultureFor(string tileId) =>
            _cultures.FirstOrDefault(culture => culture.LivesOn(tileId)) ?? DefaultCulture;

        public string PeopleName(SettlementCulture culture)
        {
            var name = CultureById(culture.Id).DisplayName;
            return string.IsNullOrEmpty(name) ? name : char.ToUpper(name[0]) + name.Substring(1);
        }

        public CultureDefinition CultureById(string id) =>
            _cultures.Append(DefaultCulture).First(culture => culture.Id == id);
    }
}
