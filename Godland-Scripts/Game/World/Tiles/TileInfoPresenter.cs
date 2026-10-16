using System;
using System.Text;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Cells;
using Godland.Core.Life.Features;
using Godland.Core.Life.People;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Life.Features;
using Godland.Game.Powers.Gods;
using Godland.Game.Life.People;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TileInfoPresenter : IStartable, IDisposable
    {
        private readonly TileSelection _selection;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly TileCatalog _catalog;
        private readonly WorldClock _clock;
        private readonly TileInfoView _view;
        private readonly TileFeatures _features;
        private readonly FeatureCatalog _featureCatalog;
        private readonly FeatureInteractions _interactions;
        private readonly Settlements _settlements;
        private readonly Godland.Game.Life.Races.RaceKeeper _races;
        private readonly SettlementConfig _settlementConfig;
        private readonly Shrines _shrines;
        private readonly FeatureDepletion _depletion;
        private readonly IdolCataclysms _cataclysms;
        private readonly Plague _plague;
        private readonly Godfighters _godfighters;
        private readonly GodsConfig _gods;
        private readonly Pantheon _pantheon;
        private readonly TileHistory _history;
        private readonly CreatureGenes _creatures;

        public TileInfoPresenter(
            TileSelection selection, WorldTiles tiles, CellFertility fertility, TileCatalog catalog, WorldClock clock, TileInfoView view,
            TileFeatures features, FeatureCatalog featureCatalog, FeatureInteractions interactions, Settlements settlements, SettlementConfig settlementConfig, Shrines shrines,
            FeatureDepletion depletion, IdolCataclysms cataclysms, Plague plague, Godfighters godfighters,
            GodsConfig gods, Pantheon pantheon, TileHistory history, Godland.Game.Life.Races.RaceKeeper races, CreatureGenes creatures)
        {
            _creatures = creatures;
            _races = races;
            _history = history;
            _selection = selection;
            _tiles = tiles;
            _fertility = fertility;
            _catalog = catalog;
            _clock = clock;
            _view = view;
            _features = features;
            _featureCatalog = featureCatalog;
            _interactions = interactions;
            _settlements = settlements;
            _settlementConfig = settlementConfig;
            _shrines = shrines;
            _depletion = depletion;
            _cataclysms = cataclysms;
            _plague = plague;
            _godfighters = godfighters;
            _gods = gods;
            _pantheon = pantheon;
        }

        public void Start()
        {
            Refresh();
            _selection.Changed += Refresh;
            _tiles.Placed += OnPlaced;
            _tiles.Removed += OnRemoved;
            _clock.Ticked += OnTicked;
            _features.Added += OnPlaced;
            _features.Removed += OnPlaced;
        }

        public void Dispose()
        {
            _selection.Changed -= Refresh;
            _tiles.Placed -= OnPlaced;
            _tiles.Removed -= OnRemoved;
            _clock.Ticked -= OnTicked;
            _features.Added -= OnPlaced;
            _features.Removed -= OnPlaced;
        }

        private void OnPlaced(GridCoord coord, string _)
        {
            if (_selection.Selected == coord) Refresh();
        }

        private void OnTicked(int _) => Refresh();

        private void OnRemoved(GridCoord _) => Refresh();

        private const int HistoryShown = 4;

        private static void Section(StringBuilder details, string title) =>
            details.Append(details.Length == 0 ? string.Empty : "\n").AppendLine($"<size=78%><color=#c9a86a><b>{title.ToUpperInvariant()}</b></color></size>");

        private static void Row(StringBuilder details, string label, string value) =>
            details.AppendLine($"<color=#9a948a>{label}</color>  {value}");

        private static void Note(StringBuilder details, string text, string color = "#d8d2c4") =>
            details.AppendLine($"<size=88%><color={color}>{text}</color></size>");

        private void DescribeReligion(Religion religion, StringBuilder details)
        {
            var tile = religion.Shrine is { } shrine && _tiles.TryGet(shrine, out var godId) ? _catalog.Get(godId).DisplayName : null;
            var god = tile == null ? "без святыни"
                : religion.GodId != null ? _gods.DefinitionOf(religion.GodId).FullName
                : religion.IdolId != null ? _settlementConfig.IdolById(religion.IdolId).FullName
                : $"святыня «{tile}»";
            var dogma = _settlementConfig.DogmaById(religion.Dogma.Id);
            Section(details, religion.IsHeresy ? "Вера · ересь" : "Вера");
            Row(details, "Бог", god);
            Row(details, "Догма", dogma.DisplayName);
            var rules = dogma.Describe();
            if (!string.IsNullOrEmpty(rules)) Note(details, rules, "#a9a394");
            Row(details, "Пыл", $"{religion.Fervor}   <color=#9a948a>доля мира</color>  {_cataclysms.SharePercent(religion)}%");
            if (_cataclysms.Current is { } summoning && summoning.Religion == religion)
                Note(details, $"Призыв «{_settlementConfig.IdolById(religion.IdolId).CataclysmName}» через {summoning.YearsLeft} лет", "#ff7766");
        }

        private void AppendHistory(GridCoord coord, StringBuilder details)
        {
            var records = _history.Of(coord);
            if (records.Count == 0) return;

            Section(details, "История");
            for (var i = records.Count - 1; i >= Math.Max(0, records.Count - HistoryShown); i--)
            {
                var record = records[i];
                var text = record.TileId == null ? record.Text
                    : record.TileId.Length == 0 ? $"земля исчезла — {record.Text}"
                    : string.IsNullOrEmpty(record.Text) ? $"стал «{_catalog.Get(record.TileId).DisplayName}»"
                    : $"стал «{_catalog.Get(record.TileId).DisplayName}» — {record.Text}";
                Note(details, $"<color=#9a948a>{record.Year}</color>  {text}");
            }
        }

        private void Refresh()
        {
            if (_selection.Selected is not { } coord || !_tiles.TryGet(coord, out var tileId))
            {
                _view.Hide();
                return;
            }

            var tile = _catalog.Get(tileId);
            var details = new StringBuilder();
            var threats = new StringBuilder();

            if (tile.HasFertility || tile.HasCreationYield || tile.NeighborFertility > 0)
            {
                Section(details, "Земля");
                if (tile.HasFertility) Row(details, "Плодородие", $"{_fertility.Get(coord)} / {_fertility.Max}");
                if (tile.HasCreationYield) Row(details, "Созидание", $"+{tile.CreationYield} раз в {tile.CreationYieldPeriod} лет");
                if (tile.NeighborFertility > 0) Row(details, "Соседям", $"+{tile.NeighborFertility} плодородия");
            }

            if (_pantheon.TryGetAt(coord, out var seatGod))
            {
                var definition = _gods.DefinitionOf(seatGod.Rule.Id);
                Section(details, "Престол бога");
                Row(details, "Бог", $"<color=#ffd27a>{_gods.NameOf(seatGod)}</color>");
                Row(details, "Деяние", $"«{definition.DeedName}» через {seatGod.YearsToAct} лет");
            }

            if (_features.TryGet(coord, out var featureId))
            {
                var feature = _featureCatalog.Get(featureId);
                var wild = _features.IsWild(coord);
                Section(details, "Обитатели");
                _creatures.TryGet(coord, out var creature);
                Row(details, CreatureNames.NameOf(creature, feature.DisplayName), wild ? "<color=#a9a394>дикие</color>" : string.Empty);
                if (creature != null)
                {
                    Note(details, CreatureNames.Describe(creature), "#a9a394");
                    if (creature.Lineage is { } lineage) Note(details, $"Вид: {CreatureNames.Effect(lineage)}", "#e0b060");
                }
                var effect = feature.Describe();
                if (wild) Note(details, "Без бонусов; подсадка заменит их", "#a9a394");
                else if (!string.IsNullOrEmpty(effect)) Note(details, effect, "#a9a394");
                if (_depletion.TryGet(coord, out var recovery)) Note(details, $"Истощены, восстановятся через {recovery} лет", "#e0b060");
            }

            if (_settlements.TryGet(coord, out var population))
            {
                var culture = _settlementConfig.CultureById(_settlements.CultureOf(coord).Id);
                var stage = population >= _settlementConfig.VillagePopulation ? "Деревня" : "Стоянка";
                var capacity = _settlements.CapacityOf(coord);
                var energy = _settlements.EnergyOf(coord);
                var landFood = _settlements.LandFood(coord);
                Section(details, $"{stage} · {culture.DisplayName}");
                Row(details, "Жители", $"{population} / {capacity}");
                Row(details, "Еда", energy >= 0
                    ? $"запас {energy}{(landFood > 0 ? $", земля +{landFood}" : string.Empty)}"
                    : $"<color=#ff7766>голод, долг {-energy}</color>");
                if (energy >= 0 && population > capacity) Note(details, "Бум — скоро голод", "#e0b060");
                if (_races.RaceAt(coord) is { } race)
                {
                    Row(details, "Народ", race.Name);
                    Note(details, race.Genome.Describe(), "#a9a394");
                }

                var rules = culture.Describe();
                if (!string.IsNullOrEmpty(rules)) Note(details, rules, "#a9a394");
                if (_settlements.TryGetHunt(coord, out var hunt))
                    Row(details, "Охота", $"«{_featureCatalog.Get(hunt.PreyFeatureId).DisplayName}», {hunt.YearsLeft} лет");

                if (_plague.IsSick(coord)) Note(threats, "Мор", "#ff7766");
                else if (_plague.RiskFactors(coord) is { Count: > 0 } risks) Note(threats, $"Риск мора: {string.Join(", ", risks)}", "#e0b060");
                if (_godfighters.IsRebel(coord)) Note(threats, "Богоборцы: отвергают чудеса и рушат святыни", "#ff7766");
                else if (_godfighters.IreOf(coord) > 0) Note(threats, $"Гнев на чудеса: {_godfighters.IreOf(coord)}", "#e0b060");

                if (_shrines.TryGet(coord, out var religion)) DescribeReligion(religion, details);
            }

            if (_interactions.TryGetThreat(coord, out var threat))
            {
                var definition = _featureCatalog.GetInteraction(threat.Interaction);
                Note(threats, $"{definition.Predator.DisplayName} {definition.ThreatText}, {threat.YearsLeft} лет", "#ff7766");
            }

            if (threats.Length > 0)
            {
                Section(details, "Угрозы");
                details.Append(threats);
            }

            AppendHistory(coord, details);
            _view.Show(tile.DisplayName, details.ToString().TrimEnd());
        }
    }
}
