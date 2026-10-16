using System;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Fate.Emergence
{
    public sealed class EmergencePresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 6f;

        private readonly EmergenceKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;

        public EmergencePresenter(EmergenceKeeper keeper, EventJournal journal, EpochView banner, Chronicle chronicle)
        {
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start() => _keeper.Emerged += OnEmerged;

        public void Dispose() => _keeper.Emerged -= OnEmerged;

        private void OnEmerged(EmergenceDefinition emergence, GridCoord site)
        {
            _banner.Announce("Мир породил новое", $"{emergence.Title}\n<size=60%>{emergence.Text}</size>", BannerSeconds);
            _journal.Write($"{emergence.Title}: {emergence.Text}", JournalTone.Threat);
            _chronicle.Add($"<b>Возникло: {emergence.Title}</b>", site);
        }
    }
}
