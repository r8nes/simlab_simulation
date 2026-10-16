using System.Collections.Generic;
using Godland.Core.World.Timing;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Writer.Commandments;
using Godland.Game.Fate.Epochs;
using Godland.Game.Powers.Sovereigns;

namespace Godland.Game.Presentation.Overview
{
    public sealed class EpochReport : IWorldReport
    {
        private readonly EpochProgression _epochs;
        private readonly CatastropheKeeper _catastrophes;
        private readonly CatastropheContext _context;
        private readonly CommandmentKeeper _commandment;
        private readonly SovereignKeeper _sovereigns;
        private readonly WorldClock _clock;
        private readonly Godland.Game.Writer.Threads.WriterThreads _writer;
        private readonly Godland.Game.Writer.Threads.WriterConfig _writerConfig;
        private readonly Godland.Game.Writer.Commands.EntityCommands _commands;

        public EpochReport(
            EpochProgression epochs, CatastropheKeeper catastrophes, CatastropheContext context, CommandmentKeeper commandment,
            SovereignKeeper sovereigns, WorldClock clock, Godland.Game.Writer.Threads.WriterThreads writer, Godland.Game.Writer.Threads.WriterConfig writerConfig,
            Godland.Game.Writer.Commands.EntityCommands commands)
        {
            _commands = commands;
            _writer = writer;
            _writerConfig = writerConfig;
            _epochs = epochs;
            _catastrophes = catastrophes;
            _context = context;
            _commandment = commandment;
            _sovereigns = sovereigns;
            _clock = clock;
        }

        public string Title => "Эпоха и мир";

        public IEnumerable<WorldReportLine> Lines()
        {
            var current = _epochs.Current;
            yield return new WorldReportLine(
                $"Эпоха: {current.DisplayName}" + (_epochs.IsBranch ? " (ветка)" : string.Empty),
                $"С {_epochs.StartYear} года. {current.Text}");

            if (_catastrophes.Age is { } age)
                yield return new WorldReportLine($"Последствия: {age.DisplayName}", $"{age.Text}\nЕщё {_catastrophes.AgeYearsLeft(_clock.Year)} лет.");

            if (_epochs.Next is { } next)
            {
                yield return new WorldReportLine($"Дальше: {next.DisplayName}", _epochs.GoalText);
                foreach (var branch in next.Alternatives)
                {
                    var ready = branch.BranchCondition != null && branch.BranchCondition.IsReady(_context);
                    yield return new WorldReportLine(
                        $"  Возможная ветка: {branch.DisplayName}",
                        ready ? "Условие выполнено: если так останется, наступит вместо основной." : "Условие пока не выполнено.");
                }
            }

            if (_commandment.Current is { } commandment)
                yield return new WorldReportLine("Заповедь", $"{commandment.Text}\nСработала {_commandment.TimesFired} раз.");

            if (_sovereigns.Ruler is { } ruler)
                yield return new WorldReportLine(
                    $"Правитель мира: {ruler.DisplayName}",
                    $"{ruler.Temperament.Describe()}; {_sovereigns.MindOf(ruler).Describe(name => name)}\nСвергнуть: {ruler.OverthrowText}");

            var center = _context.World.Grid.Center;
            yield return new WorldReportLine(
                "Великие приказы",
                "Дорогие приказы, меняющие ход всего мира. Для накопленных веры и созидания.",
                null,
                new[]
                {
                    _commands.Make("great-hasten", "great-hasten", () =>
                    {
                        _epochs.Hasten(100);
                        return center;
                    }),
                    _commands.Make("great-blessing", "great-blessing", () =>
                    {
                        foreach (var cell in System.Linq.Enumerable.ToList(_context.World.Settlements.All.Keys)) _context.World.Settlements.Bless(cell, 2);
                        return center;
                    })
                });

            if (_writerConfig.PathsEnabled)
                yield return new WorldReportLine(
                    $"Путь писателя: {Godland.Game.Writer.Threads.WriterThreads.NameOf(_writer.Path)}",
                    "Каждый век стиль игры засчитывается одному пути: много тайлов — Творец, много приказов и чудес — Режиссёр, почти без вмешательства — Наблюдатель.");

            if (_writer.Prophet is { } prophet)
                yield return new WorldReportLine(
                    $"Пророк {prophet.Name}",
                    $"Говорит от имени писателя с {prophet.Born} года (правда: {prophet.Truths}, ложь: {prophet.Lies}). Пока он жив, приказы дешевле на {_writerConfig.ProphetDiscountPercent}%.",
                    prophet.Home);
        }
    }
}
