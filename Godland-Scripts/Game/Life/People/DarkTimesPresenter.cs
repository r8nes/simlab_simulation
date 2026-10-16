using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.People
{
    public sealed class DarkTimesPresenter : IStartable, IDisposable
    {
        private readonly Plague _plague;
        private readonly Godfighters _godfighters;
        private readonly Settlements _settlements;
        private readonly SettlementConfig _config;
        private readonly FloatingTextSpawner _spawner;
        private readonly EventJournal _journal;

        public DarkTimesPresenter(
            Plague plague, Godfighters godfighters, Settlements settlements, SettlementConfig config,
            FloatingTextSpawner spawner, EventJournal journal)
        {
            _plague = plague;
            _godfighters = godfighters;
            _settlements = settlements;
            _config = config;
            _spawner = spawner;
            _journal = journal;
        }

        public void Start()
        {
            _plague.Outbreak += OnOutbreak;
            _plague.Spread += OnSpread;
            _plague.Recovered += OnRecovered;
            _godfighters.Rebelled += OnRebelled;
            _godfighters.ShrineDestroyed += OnShrineDestroyed;
        }

        public void Dispose()
        {
            _plague.Outbreak -= OnOutbreak;
            _plague.Spread -= OnSpread;
            _plague.Recovered -= OnRecovered;
            _godfighters.Rebelled -= OnRebelled;
            _godfighters.ShrineDestroyed -= OnShrineDestroyed;
        }

        private string PeopleAt(GridCoord coord)
        {
            var culture = _settlements.CultureOf(coord);
            return culture != null ? _config.PeopleName(culture) : "Жители";
        }

        private void OnOutbreak(GridCoord settlement, IReadOnlyList<string> causes)
        {
            _spawner.Spawn(settlement, "Мор!", FloatingTextStyle.Threat);
            _journal.Write($"Мор у поселения ({PeopleAt(settlement)}): {string.Join(", ", causes)}", JournalTone.Threat);
        }

        private void OnSpread(GridCoord from, GridCoord to) =>
            _journal.Write($"Мор перекинулся на соседей ({PeopleAt(to)})", JournalTone.Threat);

        private void OnRecovered(GridCoord settlement) =>
            _journal.Write($"{PeopleAt(settlement)} пережили мор", JournalTone.Good);

        private void OnRebelled(GridCoord settlement)
        {
            _spawner.Spawn(settlement, "Богоборцы!", FloatingTextStyle.Threat);
            _journal.Write($"{PeopleAt(settlement)} восстали против богов — богоборцы", JournalTone.Threat);
        }

        private void OnShrineDestroyed(GridCoord rebel, GridCoord shrine) =>
            _journal.Write("Богоборцы разрушили святыню", JournalTone.Threat);
    }
}
