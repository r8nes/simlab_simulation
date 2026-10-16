using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using Godland.Game.Presentation.Timing;
using VContainer.Unity;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 8f;

        private readonly CommandmentDraft _draft;
        private readonly CommandmentKeeper _keeper;
        private readonly CommandmentView _view;
        private readonly TimeFlow _time;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;
        private readonly CommandmentRewrite _rewrite;
        private readonly Dictionary<CommandmentSlot, CommandmentWord> _chosen = new();

        public CommandmentPresenter(
            CommandmentDraft draft, CommandmentKeeper keeper, CommandmentView view, TimeFlow time, EventJournal journal, EpochView banner, Chronicle chronicle, CommandmentRewrite rewrite)
        {
            _rewrite = rewrite;
            _draft = draft;
            _keeper = keeper;
            _view = view;
            _time = time;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _keeper.Proclaimed += OnProclaimed;
            _keeper.Fired += OnFired;
            _view.Confirmed += OnConfirmed;
            _rewrite.Requested += Open;
            _rewrite.Rewritten += OnRewritten;
            Open();
        }

        private void Open()
        {
            _draft.Offer();
            _chosen[CommandmentSlot.When] = _draft.When.Count > 0 ? _draft.When[0] : null;
            _chosen[CommandmentSlot.Then] = _draft.Then.Count > 0 ? _draft.Then[0] : null;
            _chosen[CommandmentSlot.Where] = _draft.Where.Count > 0 ? _draft.Where[0] : null;
            _chosen[CommandmentSlot.Amplifier] = null;
            _time.Block();
            _view.Show(true);
            Refresh();
        }

        public void Dispose()
        {
            _keeper.Proclaimed -= OnProclaimed;
            _keeper.Fired -= OnFired;
            _view.Confirmed -= OnConfirmed;
            _rewrite.Requested -= Open;
            _rewrite.Rewritten -= OnRewritten;
        }

        private void OnRewritten(Commandment commandment, int keepers)
        {
            var text = keepers > 0
                ? $"Писатель переписал заповедь. Веры остыли, боги оскорблены, {keepers} поселений хранят старый завет"
                : "Писатель переписал заповедь. Веры остыли, боги оскорблены";
            _journal.Write(text, JournalTone.Threat);
            _chronicle.Add($"<b>{text}</b>");
        }

        private void Refresh()
        {
            _view.ClearOptions();
            AddOptions(CommandmentSlot.When, _draft.When);
            AddOptions(CommandmentSlot.Then, _draft.Then);
            AddOptions(CommandmentSlot.Where, _draft.Where);
            AddOptions(CommandmentSlot.Amplifier, _draft.Amplifiers);
            if (_draft.Amplifiers.Count > 0) AddOption(CommandmentSlot.Amplifier, null, "без усилителя");
            _view.ShowSentence(Compose()?.Text ?? "Выбери слова заповеди");
        }

        private void AddOptions(CommandmentSlot slot, IReadOnlyList<CommandmentWord> words)
        {
            foreach (var word in words) AddOption(slot, word, $"{word.Text}{RarityMark(word.Rarity)}");
        }

        private void AddOption(CommandmentSlot slot, CommandmentWord word, string text)
        {
            var selected = _chosen.GetValueOrDefault(slot) == word;
            _view.AddOption(slot, selected ? $"<color=#ffd27a><b>» {text} «</b></color>" : text, () => Choose(slot, word));
        }

        private void Choose(CommandmentSlot slot, CommandmentWord word)
        {
            _chosen[slot] = word;
            Refresh();
        }

        private Commandment Compose()
        {
            var when = _chosen.GetValueOrDefault(CommandmentSlot.When);
            var then = _chosen.GetValueOrDefault(CommandmentSlot.Then);
            var where = _chosen.GetValueOrDefault(CommandmentSlot.Where);
            return when == null || then == null || where == null ? null : new Commandment(when, then, where, _chosen.GetValueOrDefault(CommandmentSlot.Amplifier));
        }

        private void OnConfirmed()
        {
            var commandment = Compose() ?? _draft.PickRandom();
            _view.Show(false);
            _view.ClearOptions();
            _time.Unblock();
            _keeper.Proclaim(commandment);
        }

        private static string RarityMark(CommandmentRarity rarity) => rarity switch
        {
            CommandmentRarity.Rare => " <color=#7fb2ff>(редкое)</color>",
            CommandmentRarity.Legendary => " <color=#ffb347>(легенда)</color>",
            _ => string.Empty
        };

        private void OnProclaimed(Commandment commandment)
        {
            _banner.Announce("Заповедь", $"<size=70%>{commandment.Text}</size>", BannerSeconds);
            _journal.Write($"Заповедь мира: {commandment.Text}", JournalTone.Good);
            _chronicle.Add($"<b>Заповедь: {commandment.Text}</b>");
        }

        private void OnFired(Commandment commandment, GridCoord origin) =>
            _journal.Write($"Заповедь исполнилась: {commandment.Then.Text} {commandment.Where.Text}", JournalTone.Threat);
    }
}
