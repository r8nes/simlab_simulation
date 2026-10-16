using System;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Fate.Catastrophes
{
    public sealed class CatastrophesPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 7f;

        private readonly CatastropheKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;

        public CatastrophesPresenter(CatastropheKeeper keeper, EventJournal journal, EpochView banner, Chronicle chronicle)
        {
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _keeper.Foretold += OnForetold;
            _keeper.Averted += OnAverted;
            _keeper.Struck += OnStruck;
            _keeper.AgeBegan += OnAgeBegan;
            _keeper.AgeEnded += OnAgeEnded;
        }

        public void Dispose()
        {
            _keeper.Foretold -= OnForetold;
            _keeper.Averted -= OnAverted;
            _keeper.Struck -= OnStruck;
            _keeper.AgeBegan -= OnAgeBegan;
            _keeper.AgeEnded -= OnAgeEnded;
        }

        private void OnForetold(CatastropheDefinition catastrophe, int year)
        {
            _banner.Announce("Грядёт катастрофа", $"{catastrophe.Title}\n<size=60%>{catastrophe.Omen} ({year} год)</size>", BannerSeconds);
            _journal.Write($"Предвестие: {catastrophe.Omen}", JournalTone.Threat);
        }

        private void OnAverted(CatastropheDefinition catastrophe) =>
            _journal.Write($"Катастрофа миновала: {catastrophe.Title}", JournalTone.Good);

        private void OnStruck(CatastropheDefinition catastrophe)
        {
            _banner.Announce(catastrophe.Title, $"<size=60%>{catastrophe.StrikeText}</size>", BannerSeconds);
            _journal.Write($"{catastrophe.Title}: {catastrophe.StrikeText}", JournalTone.Threat);
            _chronicle.Add($"<b>Катастрофа: {catastrophe.Title}</b>");
        }

        private void OnAgeBegan(AgeDefinition age)
        {
            _journal.Write($"Наступила эпоха: {age.DisplayName}", JournalTone.Info);
            _chronicle.Add($"<b>Наступила эпоха: {age.DisplayName}</b>");
        }

        private void OnAgeEnded(AgeDefinition age)
        {
            _journal.Write($"Эпоха «{age.DisplayName}» окончена: мир оправился", JournalTone.Good);
            _chronicle.Add($"Окончена эпоха: {age.DisplayName}");
        }
    }
}
