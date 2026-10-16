using System;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Powers.Gods;
using Godland.Game.World.Tiles;
using VContainer.Unity;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Life.People
{
    public sealed class ShrinesPresenter : IStartable, IDisposable
    {
        private readonly Shrines _shrines;
        private readonly WorldTiles _tiles;
        private readonly TileCatalog _catalog;
        private readonly SettlementConfig _config;
        private readonly FloatingTextSpawner _spawner;
        private readonly Settlements _settlements;
        private readonly EventJournal _journal;
        private readonly GodsConfig _gods;

        public ShrinesPresenter(
            Shrines shrines, WorldTiles tiles, TileCatalog catalog, SettlementConfig config, FloatingTextSpawner spawner,
            Settlements settlements, EventJournal journal, GodsConfig gods)
        {
            _gods = gods;
            _shrines = shrines;
            _tiles = tiles;
            _catalog = catalog;
            _config = config;
            _spawner = spawner;
            _settlements = settlements;
            _journal = journal;
        }

        public void Start()
        {
            _shrines.BeliefChanged += OnBeliefChanged;
            _shrines.IdolRevealed += OnIdolRevealed;
        }

        public void Dispose()
        {
            _shrines.BeliefChanged -= OnBeliefChanged;
            _shrines.IdolRevealed -= OnIdolRevealed;
        }

        private void OnIdolRevealed(Religion religion, GridCoord settlement)
        {
            var idol = _config.IdolById(religion.IdolId);
            _spawner.Spawn(religion.Shrine ?? settlement, $"Бог обрёл имя: {idol.FullName}", FloatingTextStyle.Evolution);
            _journal.Write($"Бог обрёл имя: {idol.FullName}", JournalTone.Good);
        }

        private void OnBeliefChanged(GridCoord settlement, Religion previous, Religion current, BeliefChange reason)
        {
            var text = reason switch
            {
                BeliefChange.Revelation => $"Жители обрели бога: {GodName(current)}",
                BeliefChange.Conversion => $"Приняли веру соседей: {GodName(current)}",
                BeliefChange.Conquest => $"Обращены силой: {GodName(current)}",
                BeliefChange.Calling => $"Призваны богом: {GodName(current)}",
                BeliefChange.Schism => $"Раскол! Ересь: {_config.DogmaById(current.Dogma.Id).DisplayName}",
                BeliefChange.Desecration => "Святыня осквернена!",
                _ => null
            };
            if (text == null) return;

            WriteToJournal(settlement, previous, current, reason);

            var style = reason == BeliefChange.Desecration ? FloatingTextStyle.Threat : FloatingTextStyle.Evolution;
            _spawner.Spawn(reason == BeliefChange.Revelation && current.Shrine is { } shrine ? shrine : settlement, text, style);
        }

        private void WriteToJournal(GridCoord settlement, Religion previous, Religion current, BeliefChange reason)
        {
            var culture = _settlements.CultureOf(settlement);
            var people = culture != null ? _config.PeopleName(culture) : "Жители";
            var entry = reason switch
            {
                BeliefChange.Revelation => ($"{people} обрели бога: {GodName(current)}", JournalTone.Good),
                BeliefChange.Schism => ($"Раскол веры {GodName(current)}: ересь «{_config.DogmaById(current.Dogma.Id).DisplayName}»", JournalTone.Threat),
                BeliefChange.Conquest => ($"{people} обращены силой в веру {GodName(current)}", JournalTone.Threat),
                BeliefChange.Calling => ($"{people} услышали зов бога: {GodName(current)}", JournalTone.Info),
                BeliefChange.Desecration => ($"Святыня {GodName(previous)} осквернена", JournalTone.Threat),
                _ => (null, JournalTone.Info)
            };
            if (entry.Item1 != null) _journal.Write(entry.Item1, entry.Item2);
        }

        private string GodName(Religion religion) =>
            religion.GodId != null ? _gods.DefinitionOf(religion.GodId).DisplayName
            : religion.IdolId != null ? _config.IdolById(religion.IdolId).DisplayName
            : religion.Shrine is { } shrine && _tiles.TryGet(shrine, out var tileId) ? _catalog.Get(tileId).DisplayName
            : "без имени";
    }
}
