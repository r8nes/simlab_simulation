using System;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.Kingdoms
{
    public sealed class KingdomsPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 4f;

        private readonly Realms _realms;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public KingdomsPresenter(Realms realms, EventJournal journal, EpochView banner)
        {
            _realms = realms;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _realms.Founded += OnFounded;
            _realms.Dissolved += OnDissolved;
            _realms.CapitalMoved += OnCapitalMoved;
            _realms.WarDeclared += OnWarDeclared;
            _realms.PeaceMade += OnPeaceMade;
            _realms.Conquered += OnConquered;
        }

        public void Dispose()
        {
            _realms.Founded -= OnFounded;
            _realms.Dissolved -= OnDissolved;
            _realms.CapitalMoved -= OnCapitalMoved;
            _realms.WarDeclared -= OnWarDeclared;
            _realms.PeaceMade -= OnPeaceMade;
            _realms.Conquered -= OnConquered;
        }

        private void OnFounded(Kingdom kingdom) =>
            _journal.Write($"Основано царство {kingdom.Name}: {kingdom.Members.Count} поселений", JournalTone.Good);

        private void OnCapitalMoved(Kingdom kingdom, Godland.Core.World.Grid.GridCoord capital) =>
            _journal.Write($"Царство {kingdom.Name} перенесло столицу в ({capital.X}, {capital.Y})", JournalTone.Info);

        private void OnDissolved(Kingdom kingdom) => _journal.Write($"Царство {kingdom.Name} распалось", JournalTone.Threat);

        private void OnWarDeclared(War war)
        {
            _journal.Write($"Война: {war.First.Name} против {war.Second.Name}", JournalTone.Threat);
            _banner.Announce("Война", $"{war.First.Name} против {war.Second.Name}", BannerSeconds);
        }

        private void OnPeaceMade(War war) => _journal.Write($"Мир между {war.First.Name} и {war.Second.Name}", JournalTone.Good);

        private void OnConquered(Kingdom attacker, Kingdom defender, GridCoord _) =>
            _journal.Write($"{attacker.Name} захватывает поселение царства {defender.Name}", JournalTone.Threat);
    }
}
