using System;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Fate.Sagas
{
    public sealed class SagaPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 8f;

        private readonly SagaKeeper _keeper;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;

        public SagaPresenter(SagaKeeper keeper, EventJournal journal, EpochView banner, Chronicle chronicle)
        {
            _keeper = keeper;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _keeper.Began += OnBegan;
            _keeper.ActBegan += OnActBegan;
            _keeper.Turned += OnTurned;
            _keeper.Ended += OnEnded;
            _keeper.Battled += OnBattled;
            _keeper.Overthrown += OnOverthrown;
            _keeper.Betrayed += OnBetrayed;
            _keeper.ArtifactFound += OnArtifact;
            _keeper.BridgeOpened += OnBridgeOpened;
            _keeper.BridgeClosed += OnBridgeClosed;
        }

        public void Dispose()
        {
            _keeper.Began -= OnBegan;
            _keeper.ActBegan -= OnActBegan;
            _keeper.Turned -= OnTurned;
            _keeper.Ended -= OnEnded;
            _keeper.Battled -= OnBattled;
            _keeper.Overthrown -= OnOverthrown;
            _keeper.Betrayed -= OnBetrayed;
            _keeper.ArtifactFound -= OnArtifact;
            _keeper.BridgeOpened -= OnBridgeOpened;
            _keeper.BridgeClosed -= OnBridgeClosed;
        }

        private void OnBegan(SagaDefinition saga)
        {
            _banner.Announce("Сдвиг полярности", $"{saga.Title}\n<size=60%>{saga.Text}</size>", BannerSeconds);
            _journal.Write($"Сдвиг полярности: {saga.Title}. {saga.Text}", JournalTone.Threat);
            _chronicle.Add($"<b>Сдвиг полярности: {saga.Title}</b>");
        }

        private void OnActBegan(SagaActDefinition act)
        {
            _banner.Announce(_keeper.Current.Title, $"{act.Title}\n<size=60%>{act.Text}</size>", BannerSeconds);
            _journal.Write($"Сага «{_keeper.Current.Title}»: {act.Title}. {act.Text}", JournalTone.Info);
            _chronicle.Add($"<b>{_keeper.Current.Title}: {act.Title}</b>");
        }

        private void OnTurned(SagaTransition transition)
        {
            _journal.Write($"Поворот саги: {transition.Text}", JournalTone.Threat);
            _chronicle.Add($"Поворот: {transition.Text}");
        }

        private void OnEnded(SagaDefinition saga, SagaTransition transition)
        {
            _banner.Announce("Конец саги", $"{saga.Title}\n<size=60%>{transition.Text}</size>", BannerSeconds);
            _journal.Write($"Сага «{saga.Title}» окончена: {transition.Text}", JournalTone.Good);
            _chronicle.Add($"<b>Конец саги «{saga.Title}»</b>: {transition.Text}");
        }

        private void OnBattled(SagaFaction winner, SagaFaction loser, GridCoord cell) =>
            _journal.Write($"Битва: {winner.Definition.DisplayName} одолели {loser.Definition.DisplayName}", JournalTone.Threat);

        private void OnOverthrown(SagaFaction faction, string old)
        {
            _journal.Write($"Заговор: {old} свергнут, сторону «{faction.Definition.DisplayName}» ведёт {faction.Leader}", JournalTone.Threat);
            _chronicle.Add($"Заговор: {old} свергнут, правит {faction.Leader}");
        }

        private void OnBetrayed(SagaFaction from, SagaFaction to, GridCoord cell) =>
            _journal.Write($"Предательство: земля ушла от стороны «{from.Definition.DisplayName}» к «{to.Definition.DisplayName}»", JournalTone.Threat);

        private void OnArtifact(SagaFaction faction)
        {
            _banner.Announce("Артефакт", $"{faction.Definition.DisplayName} нашли артефакт", BannerSeconds);
            _journal.Write($"Артефакт найден: сторона «{faction.Definition.DisplayName}» стала сильнее", JournalTone.Good);
            _chronicle.Add($"<b>Артефакт у стороны «{faction.Definition.DisplayName}»</b>");
        }

        private void OnBridgeOpened(GridCoord cell)
        {
            _journal.Write("Между мирами открылся мост", JournalTone.Threat);
            _chronicle.Add("Между мирами открылся мост", cell);
        }

        private void OnBridgeClosed() => _journal.Write("Мост между мирами закрылся", JournalTone.Info);
    }
}
