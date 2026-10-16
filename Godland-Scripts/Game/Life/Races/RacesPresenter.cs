using System;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.Races
{
    public sealed class RacesPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 6f;

        private readonly RaceKeeper _races;
        private readonly WonderAge _wonders;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;

        public RacesPresenter(RaceKeeper races, WonderAge wonders, EventJournal journal, EpochView banner, Chronicle chronicle)
        {
            _races = races;
            _wonders = wonders;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _races.Born += OnBorn;
            _races.Vanished += OnVanished;
            _races.Diverged += OnDiverged;
            _wonders.Began += OnBegan;
            _wonders.Ended += OnEnded;
            _wonders.Clashed += OnClashed;
            _wonders.Befriended += OnBefriended;
            _wonders.Miracle += OnMiracle;
        }

        public void Dispose()
        {
            _races.Born -= OnBorn;
            _races.Vanished -= OnVanished;
            _races.Diverged -= OnDiverged;
            _wonders.Began -= OnBegan;
            _wonders.Ended -= OnEnded;
            _wonders.Clashed -= OnClashed;
            _wonders.Befriended -= OnBefriended;
            _wonders.Miracle -= OnMiracle;
        }

        private void OnBorn(Race race, GridCoord cell)
        {
            var header = race.GodName == null ? "Рождается новый народ" : $"{race.GodName} создаёт народ";
            _banner.Announce(header, $"{race.Name}\n<size=60%>{race.Origin}</size>", BannerSeconds);
            _journal.Write($"{race.Name}: {race.Origin}", JournalTone.Good);
            _chronicle.Add($"<b>Родился народ: {race.Name}</b> — {race.Origin}", cell);
        }

        private void OnDiverged(Race parent, Race child)
        {
            _banner.Announce("Народ раскололся", $"{child.Name}\n<size=60%>{child.Origin}</size>", BannerSeconds);
            _journal.Write($"Народ «{parent.Name}» раскололся: появились {child.Name}", JournalTone.Info);
            _chronicle.Add($"<b>Раскол народа:</b> {child.Name} откололись от «{parent.Name}»");
        }

        private void OnVanished(Race race)
        {
            _journal.Write($"Народ «{race.Name}» исчез с лица мира", JournalTone.Threat);
            _chronicle.Add($"Исчез народ: {race.Name}");
        }

        private void OnBegan()
        {
            _banner.Announce("Эпоха сменилась", "Эпоха чудес\n<size=60%>Народы богов делят мир; мир рождает невиданное вдвое чаще</size>", BannerSeconds);
            _journal.Write("Наступила Эпоха чудес", JournalTone.Good);
            _chronicle.Add("<b>Наступила Эпоха чудес</b>");
        }

        private void OnEnded(Race dominant)
        {
            var text = dominant == null ? "Эпоха чудес угасла" : $"Эпоха чудес окончена: мир принадлежит народу «{dominant.Name}»";
            _banner.Announce("Конец эпохи", text, BannerSeconds);
            _journal.Write(text, JournalTone.Info);
            _chronicle.Add($"<b>{text}</b>");
        }

        private void OnClashed(Race race, Race other, GridCoord victim) =>
            _journal.Write($"Война народов: {race.Name} против {other.Name}", JournalTone.Threat);

        private void OnBefriended(Race race, Race other, GridCoord cell) =>
            _journal.Write($"Дружба народов: {race.Name} и {other.Name}", JournalTone.Good);

        private void OnMiracle(AnomalyDefinition anomaly)
        {
            _journal.Write("Чудо эпохи: мир порождает невиданное", JournalTone.Threat);
            _chronicle.Add("<b>Чудо Эпохи чудес</b>");
        }
    }
}
