using System.Collections.Generic;
using Godland.Core.World.Timing;
using Godland.Game.Writer.Commandments;

namespace Godland.Game.Presentation.Overview
{
    public sealed class CommandmentReport : IWorldReport
    {
        private readonly CommandmentKeeper _keeper;
        private readonly WorldClock _clock;
        private readonly Godland.Game.Writer.Commands.EntityCommands _commands;
        private readonly CommandmentRewrite _rewrite;

        public CommandmentReport(CommandmentKeeper keeper, WorldClock clock, Godland.Game.Writer.Commands.EntityCommands commands, CommandmentRewrite rewrite)
        {
            _commands = commands;
            _rewrite = rewrite;
            _keeper = keeper;
            _clock = clock;
        }

        public string Title => "Заповедь";

        public IEnumerable<WorldReportLine> Lines()
        {
            if (_keeper.Current is not { } commandment)
            {
                yield return new WorldReportLine("Заповеди нет", "Заповедь даётся в начале забега.");
                yield break;
            }

            var cooldown = _keeper.ReadyYear - _clock.Year;
            yield return new WorldReportLine(
                commandment.Text,
                $"Сработала {_keeper.TimesFired} раз. " + (cooldown > 0 ? $"Снова сможет через {cooldown} лет." : "Готова сработать при следующем событии.") +
                $"\nКогда: {commandment.When.Text}. То: {commandment.Then.Text}. Где: {commandment.Where.Text}." +
                (commandment.Amplifier != null ? $" Усилитель: {commandment.Amplifier.Text}." : string.Empty) +
                (_rewrite.Times > 0 ? $"\nПереписана {_rewrite.Times} раз." : string.Empty),
                null,
                new[] { _commands.Make("commandment-rewrite", "commandment", _rewrite.Request) });

            foreach (var (year, place) in _keeper.Recent)
                yield return new WorldReportLine($"{year} год", $"Сработала: {commandment.Then.Text}", place);
        }
    }
}
