using System;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Writer.Progression
{
    public sealed class DiscoveryPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 5f;

        private readonly DiscoveryKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;

        public DiscoveryPresenter(DiscoveryKeeper keeper, EventJournal journal, EpochView banner, Chronicle chronicle)
        {
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start() => _keeper.Discovered += OnDiscovered;

        public void Dispose() => _keeper.Discovered -= OnDiscovered;

        private void OnDiscovered(DiscoveryDefinition discovery, int reward)
        {
            _banner.Announce("Открытие", $"{discovery.Title}\n<size=60%>+{reward} Памяти миров ждут в Летописи</size>", BannerSeconds);
            _journal.Write($"Открытие: {discovery.Title} (награда +{reward} Памяти миров ждёт в Летописи меню)", JournalTone.Good);
            _chronicle.Add($"<b>Открытие: {discovery.Title}</b>");
        }
    }
}
