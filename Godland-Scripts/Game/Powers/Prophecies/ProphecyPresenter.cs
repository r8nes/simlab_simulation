using System;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Powers.Prophecies
{
    public sealed class ProphecyPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 6f;

        private readonly ProphecyKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly LivingLand _livingLand;
        private readonly Settlements _settlements;

        public ProphecyPresenter(ProphecyKeeper keeper, EventJournal journal, EpochView banner, LivingLand livingLand, Settlements settlements)
        {
            _livingLand = livingLand;
            _settlements = settlements;
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _keeper.Announced += OnAnnounced;
            _keeper.Resolved += OnResolved;
            _livingLand.Spread += OnLandSpread;
            _settlements.Exodus += OnExodus;
        }

        public void Dispose()
        {
            _keeper.Announced -= OnAnnounced;
            _keeper.Resolved -= OnResolved;
            _livingLand.Spread -= OnLandSpread;
            _settlements.Exodus -= OnExodus;
        }

        private void OnLandSpread(GridCoord source, GridCoord target, string tileId) =>
            _journal.Write("Живая земля: биом поглотил соседа", JournalTone.Info);

        private void OnExodus(GridCoord from, GridCoord to) =>
            _journal.Write("Исход: люди ушли за край мира и открыли новую землю", JournalTone.Good);

        private void OnAnnounced(ProphecyDefinition prophecy, int deadline)
        {
            _banner.Announce("Пророчество", $"«{prophecy.Title}»\n<size=70%>до {deadline} года</size>", BannerSeconds);
            _journal.Write($"Пророчество: «{prophecy.Title}» (до {deadline} года)", JournalTone.Threat);
        }

        private void OnResolved(ProphecyDefinition prophecy, MilestoneDefinition milestone, bool fulfilled)
        {
            var outcome = fulfilled ? "Пророчество сбылось" : "Пророчество не сбылось";
            _banner.Announce(outcome, $"{milestone.DisplayName}\n<size=70%>{milestone.Rule}</size>", BannerSeconds);
            _journal.Write($"{outcome}: «{prophecy.Title}». Новый закон мира — {milestone.DisplayName}: {milestone.Rule}", JournalTone.Good);
        }
    }
}
