using System;
using System.Collections.Generic;
using Godland.Core.World.Creation;
using Godland.Core.Fate.Emergence;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Writer.Commands
{
    public sealed class EntityCommand
    {
        public EntityCommand(CommandDefinition definition, string entity, string title, Func<GridCoord, bool> target, Func<GridCoord?, GridCoord?> run)
        {
            Definition = definition;
            Entity = entity;
            Title = title;
            Target = target;
            Run = run;
        }

        public CommandDefinition Definition { get; }
        public string Entity { get; }
        public string Title { get; }
        public Func<GridCoord, bool> Target { get; }
        public Func<GridCoord?, GridCoord?> Run { get; }
        public bool NeedsTarget => Target != null;
    }

    public sealed class EntityCommands
    {
        private readonly CommandConfig _config;
        private readonly CreationPoints _creation;
        private readonly FaithPoints _faith;
        private readonly WorldEpoch _epoch;
        private readonly WorldClock _clock;
        private readonly WorldMemory _memory;
        private readonly WorldGrid _grid;
        private readonly TileHistory _history;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;
        private readonly CommandTargeting _targeting;
        private readonly Godland.Core.Flow.FlowBias _flow;
        private readonly CommandDiscount _discount;
        private readonly Dictionary<string, int> _readyYear = new();

        public EntityCommands(
            CommandConfig config, CreationPoints creation, FaithPoints faith, WorldEpoch epoch, WorldClock clock, WorldMemory memory, WorldGrid grid,
            TileHistory history, EventJournal journal, Chronicle chronicle, CommandTargeting targeting, Godland.Core.Flow.FlowBias flow, CommandDiscount discount)
        {
            _discount = discount;
            _flow = flow;
            _config = config;
            _creation = creation;
            _faith = faith;
            _epoch = epoch;
            _clock = clock;
            _memory = memory;
            _grid = grid;
            _history = history;
            _journal = journal;
            _chronicle = chronicle;
            _targeting = targeting;
        }

        public event Action<EntityCommand, GridCoord?> Issued;

        public CommandConfig Config => _config;

        public EntityCommand Make(string id, string entity, Func<GridCoord?> run, string title = null) =>
            Build(id, entity, title, null, _ => run());

        public EntityCommand MakeTargeted(string id, string entity, Func<GridCoord, bool> target, Func<GridCoord, GridCoord?> run, string title = null) =>
            Build(id, entity, title, target, cell => cell is { } chosen ? run(chosen) : null);

        public (int Creation, int Faith) CostOf(EntityCommand command)
        {
            var scale = (100 + _config.CostPerEpochPercent * _epoch.Index) * (100 - _discount.Percent) / 100;
            return (command.Definition.CreationCost * scale / 100, command.Definition.FaithCost * scale / 100);
        }

        public int CooldownLeft(EntityCommand command) =>
            Math.Max(0, _readyYear.GetValueOrDefault(command.Entity) - _clock.Year);

        public string Label(EntityCommand command)
        {
            var (creation, faith) = CostOf(command);
            var cost = string.Join(", ", Costs(creation, faith));
            var cooldown = CooldownLeft(command);
            return cooldown > 0 ? $"{command.Title} · ждать {cooldown} лет" : $"{command.Title} · {cost}";
        }

        public bool CanIssue(EntityCommand command)
        {
            var (creation, faith) = CostOf(command);
            return CooldownLeft(command) == 0 && _creation.CanAfford(creation) && _faith.CanAfford(faith);
        }

        public void Issue(EntityCommand command)
        {
            if (!CanIssue(command)) return;

            if (!command.NeedsTarget)
            {
                Execute(command, null);
                return;
            }

            _journal.Write($"Писатель выбирает место: {command.Title}. Клик по клетке, Esc — отмена");
            _targeting.Begin(command.Target, cell => Execute(command, cell));
        }

        private EntityCommand Build(string id, string entity, string title, Func<GridCoord, bool> target, Func<GridCoord?, GridCoord?> run)
        {
            var definition = _config.Find(id);
            return definition == null ? null : new EntityCommand(definition, entity, title ?? definition.Title, target, run);
        }

        private void Execute(EntityCommand command, GridCoord? target)
        {
            if (!CanIssue(command)) return;

            var place = command.Run(target);
            if (place is not { } site)
            {
                _journal.Write($"Приказ «{command.Title}» не нашёл отклика");
                return;
            }

            var (creation, faith) = CostOf(command);
            _creation.TrySpend(creation);
            _faith.TrySpend(faith);
            _readyYear[command.Entity] = _clock.Year + _config.CooldownYears;

            _memory.Add(site, $"command:{command.Definition.Id}");
            _memory.Add(_grid.Center, "commands");
            if (!string.IsNullOrEmpty(command.Definition.MemoryKey)) _memory.Add(site, command.Definition.MemoryKey);
            _history.Note(site, $"Воля писателя: {command.Title}");
            _journal.Write($"Воля писателя: {command.Title}", JournalTone.Good);
            _chronicle.Add($"Воля писателя: {command.Title}", site);
            _flow.Note($"player:{command.Definition.Id}");
            Issued?.Invoke(command, place);
        }

        private static IEnumerable<string> Costs(int creation, int faith)
        {
            if (creation > 0) yield return $"{creation} созид.";
            if (faith > 0) yield return $"{faith} веры";
            if (creation == 0 && faith == 0) yield return "даром";
        }
    }

    public sealed class CommandTargeting
    {
        private Func<GridCoord, bool> _valid;
        private Action<GridCoord> _pick;

        public bool IsActive => _pick != null;

        public void Begin(Func<GridCoord, bool> valid, Action<GridCoord> pick)
        {
            _valid = valid;
            _pick = pick;
        }

        public bool IsValid(GridCoord cell) => IsActive && _valid(cell);

        public void Cancel()
        {
            _valid = null;
            _pick = null;
        }

        public bool TryPick(GridCoord cell)
        {
            if (!IsValid(cell)) return false;

            var pick = _pick;
            Cancel();
            pick(cell);
            return true;
        }
    }
}
