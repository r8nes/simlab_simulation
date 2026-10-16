using System;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Powers.Sovereigns
{
    public sealed class SovereignPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 7f;

        private readonly SovereignKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public SovereignPresenter(SovereignKeeper keeper, EventJournal journal, EpochView banner)
        {
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _keeper.Rose += OnRose;
            _keeper.Fell += OnFell;
            _keeper.Acted += OnActed;
        }

        public void Dispose()
        {
            _keeper.Rose -= OnRose;
            _keeper.Fell -= OnFell;
            _keeper.Acted -= OnActed;
        }

        private void OnActed(SovereignDefinition sovereign, string deed) =>
            _journal.Write($"{sovereign.DisplayName} {deed}", JournalTone.Threat);

        private void OnRose(SovereignDefinition sovereign)
        {
            _banner.Announce(sovereign.Slaughter, $"{sovereign.DisplayName} — правитель мира\n<size=60%>{sovereign.Reign}</size>", BannerSeconds);
            _journal.Write($"{sovereign.Slaughter}! {sovereign.DisplayName} стал правителем мира ({sovereign.Temperament.Describe()}). {sovereign.Reign}", JournalTone.Threat);
            _journal.Write($"Чтобы свергнуть его: {sovereign.OverthrowText}", JournalTone.Info);
        }

        private void OnFell(SovereignDefinition sovereign)
        {
            _banner.Announce("Правитель свергнут", sovereign.DisplayName, BannerSeconds);
            _journal.Write($"{sovereign.DisplayName} свергнут, мир снова свободен", JournalTone.Good);
        }
    }
}
