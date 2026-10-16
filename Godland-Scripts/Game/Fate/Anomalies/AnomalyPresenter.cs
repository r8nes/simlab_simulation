using System;
using Godland.Core.Fate.Anomalies;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class AnomalyPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 7f;

        private readonly AnomalyKeeper _keeper;
        private readonly AnomalyContext _context;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public AnomalyPresenter(AnomalyKeeper keeper, AnomalyContext context, EventJournal journal, EpochView banner)
        {
            _keeper = keeper;
            _context = context;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _keeper.Foretold += OnForetold;
            _keeper.Began += OnBegan;
            _context.Devourer.Ate += OnAte;
            _context.Devourer.Fed += OnFed;
            _context.Devourer.Sated += OnSated;
            _context.Drift.Drifted += OnDrifted;
            _context.Heart.Beat += OnBeat;
            _context.Heart.Died += OnHeartDied;
            _context.SkyFolk.Converted += OnSkyConverted;
            _context.Sleep.Woke += OnWoke;
        }

        public void Dispose()
        {
            _keeper.Foretold -= OnForetold;
            _keeper.Began -= OnBegan;
            _context.Devourer.Ate -= OnAte;
            _context.Devourer.Fed -= OnFed;
            _context.Devourer.Sated -= OnSated;
            _context.Drift.Drifted -= OnDrifted;
            _context.Heart.Beat -= OnBeat;
            _context.Heart.Died -= OnHeartDied;
            _context.SkyFolk.Converted -= OnSkyConverted;
            _context.Sleep.Woke -= OnWoke;
        }

        private void OnForetold(AnomalyDefinition anomaly, int year)
        {
            _banner.Announce("Знамение", $"{anomaly.Omen}\n<size=60%>в {year} году мир изменится</size>", BannerSeconds);
            _journal.Write($"Знамение: {anomaly.Omen}", JournalTone.Threat);
        }

        private void OnBegan(AnomalyDefinition anomaly)
        {
            _banner.Announce(anomaly.Title, $"<size=60%>{anomaly.Description}</size>", BannerSeconds);
            _journal.Write($"{anomaly.Title}! {anomaly.Description}", JournalTone.Threat);
        }

        private void OnAte(GridCoord _) => _journal.Write("Пожиратель поглотил землю", JournalTone.Threat);

        private void OnFed(int left) => _journal.Write($"Пожиратель принял веру. Осталось кормлений: {left}", JournalTone.Info);

        private void OnSated()
        {
            _banner.Announce("Пожиратель насытился", "Бездна закрылась", BannerSeconds);
            _journal.Write("Пожиратель насытился верой, и бездна закрылась", JournalTone.Good);
        }

        private void OnDrifted(int line, bool vertical) => _journal.Write("Земли сдвинулись: целая полоса мира уплыла на шаг", JournalTone.Threat);

        private void OnBeat(int reborn) => _journal.Write($"Сердце мира ударило: переродилось тайлов — {reborn}", JournalTone.Info);

        private void OnHeartDied()
        {
            _banner.Announce("Сердце мира остановилось", "Мир умирает", BannerSeconds);
            _journal.Write("Сердце мира остановилось: люди слабеют, земля иссякла", JournalTone.Threat);
        }

        private void OnSkyConverted(GridCoord _) => _journal.Write("Небесный народ обратил соседей в свою веру", JournalTone.Threat);

        private void OnWoke()
        {
            _banner.Announce("Мир проснулся", "Всё, что ты сделал, пока он спал, ожило разом", BannerSeconds);
            _journal.Write("Мир проснулся", JournalTone.Good);
        }
    }
}
