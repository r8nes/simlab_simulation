using System;
using System.Collections.Generic;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using Godland.Game.Life.People;
using VContainer.Unity;

namespace Godland.Game.Fate.Cataclysms
{
    public sealed class CataclysmPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 4f;

        private readonly IdolCataclysms _cataclysms;
        private readonly SettlementConfig _config;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public CataclysmPresenter(IdolCataclysms cataclysms, SettlementConfig config, EventJournal journal, EpochView banner)
        {
            _cataclysms = cataclysms;
            _config = config;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _cataclysms.Announced += OnAnnounced;
            _cataclysms.Averted += OnAverted;
            _cataclysms.Struck += OnStruck;
        }

        public void Dispose()
        {
            _cataclysms.Announced -= OnAnnounced;
            _cataclysms.Averted -= OnAverted;
            _cataclysms.Struck -= OnStruck;
        }

        private IdolDefinition IdolOf(Summoning summoning) => _config.IdolById(summoning.Religion.IdolId);

        private void OnAnnounced(Summoning summoning)
        {
            var idol = IdolOf(summoning);
            _journal.Write($"{idol.DisplayName} держит мир и призывает «{idol.CataclysmName}» — через {summoning.YearsLeft} лет", JournalTone.Threat);
            _banner.Announce("Призыв", $"{idol.FullName}\n<size=70%>{idol.CataclysmName} через {summoning.YearsLeft} лет</size>", BannerSeconds);
        }

        private void OnAverted(Summoning summoning)
        {
            var idol = IdolOf(summoning);
            _journal.Write($"Призыв «{idol.CataclysmName}» сорвался: {idol.DisplayName} потерял силу", JournalTone.Good);
        }

        private void OnStruck(Summoning summoning, IReadOnlyList<GridCoord> cells)
        {
            var idol = IdolOf(summoning);
            var scope = cells.Count > 0 ? $": изменено тайлов — {cells.Count}" : string.Empty;
            _journal.Write($"{idol.CataclysmName}! {idol.DisplayName} исполнил призыв{scope}", JournalTone.Threat);
            _banner.Announce(idol.CataclysmName, idol.FullName, BannerSeconds);
        }
    }
}
