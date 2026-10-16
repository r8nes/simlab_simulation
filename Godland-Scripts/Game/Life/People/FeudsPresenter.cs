using System;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.People
{
    public sealed class FeudsPresenter : IStartable, IDisposable
    {
        private readonly Feuds _feuds;
        private readonly Settlements _settlements;
        private readonly SettlementConfig _config;
        private readonly FloatingTextSpawner _spawner;
        private readonly EventJournal _journal;

        public FeudsPresenter(Feuds feuds, Settlements settlements, SettlementConfig config, FloatingTextSpawner spawner, EventJournal journal)
        {
            _feuds = feuds;
            _settlements = settlements;
            _config = config;
            _spawner = spawner;
            _journal = journal;
        }

        public void Start() => _feuds.Raided += OnRaided;

        public void Dispose() => _feuds.Raided -= OnRaided;

        private void OnRaided(GridCoord attacker, GridCoord defender, bool conquered)
        {
            var raiders = PeopleAt(attacker);
            var victims = PeopleAt(defender);
            _spawner.Spawn(defender, conquered ? "Набег! Обращены в чужую веру" : "Набег!", FloatingTextStyle.Threat);
            _journal.Write(conquered
                ? $"Священная война: {raiders} разорили и обратили соседей ({victims})"
                : $"Набег: {raiders} напали на соседей ({victims}) из-за веры", JournalTone.Threat);
        }

        private string PeopleAt(GridCoord coord)
        {
            var culture = _settlements.CultureOf(coord);
            return culture != null ? _config.PeopleName(culture) : "жители";
        }
    }
}
