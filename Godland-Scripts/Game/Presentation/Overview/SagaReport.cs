using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Timing;
using Godland.Game.Fate.Sagas;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class SagaReport : IWorldReport
    {
        private readonly SagaKeeper _keeper;
        private readonly SagaConfig _config;
        private readonly WorldClock _clock;

        private readonly EntityCommands _commands;

        public SagaReport(SagaKeeper keeper, SagaConfig config, WorldClock clock, EntityCommands commands)
        {
            _commands = commands;
            _keeper = keeper;
            _config = config;
            _clock = clock;
        }

        public string Title => "Сага";

        private static string Bar(int power, int threshold, int side)
        {
            const int Width = 30;
            var filled = Math.Clamp(power * Width / Math.Max(1, threshold), 0, Width);
            var color = side == 0 ? "#e07a6a" : "#7fb2ff";
            return $"<b><color={color}>{new string('|', filled)}</color><color=#555555>{new string('|', Width - filled)}</color></b>  {power} / {threshold}";
        }

        public IEnumerable<WorldReportLine> Lines()
        {
            if (_keeper.Current is not { } saga)
            {
                yield return new WorldReportLine("Мир в обычной полярности", "Сага начнётся, когда сила сдвинет мир.");
                foreach (var candidate in _config.Sagas.Where(candidate => !candidate.OnlyFromSaga))
                {
                    var told = _keeper.Told.Contains(candidate);
                    var ready = candidate.Start != null && candidate.Start.IsReady(_keeper.World);
                    yield return new WorldReportLine(
                        candidate.Title,
                        told ? "Уже случалась в этом мире." : ready ? $"Условие выполнено: шанс {candidate.ChancePercent}% каждые {_config.CheckPeriod} лет." : "Условие пока не выполнено.");
                }

                yield break;
            }

            var act = _keeper.Act;
            yield return new WorldReportLine($"{saga.Title}: {act.Title}",
                $"{act.Text}\nАкт длится {_keeper.ActAge} лет.\n<color=#a9a394>Сага — это перетягивание: у каждой стороны своя сила (земли + бонусы + артефакт). Кто первым дойдёт до порога, тот повернёт сагу. Толкать сторону можно действием «Сага» на её земле или кнопкой ниже.</color>");

            if (_keeper.IsSplit)
                yield return new WorldReportLine(
                    "Мир расколот",
                    _keeper.BridgeOpen ? "Мост между мирами открыт: идут битвы." : $"Мост закрыт. Откроется около {_keeper.NextBridgeYear} года.",
                    _keeper.Bridge);

            foreach (var faction in _keeper.Factions)
            {
                var parts = new List<string>
                {
                    Bar(faction.Power, _keeper.ThresholdOf(faction), faction.Side),
                    $"<color=#ffd27a>{_keeper.GoalText(faction)}</color>",
                    $"Земель: {faction.Lands.Count}" + (faction.HasArtifact ? ", владеет артефактом" : string.Empty),
                    $"Ведёт: {faction.Leader}",
                    string.IsNullOrEmpty(faction.Definition.Temperament) ? null : faction.Definition.Temperament,
                    $"Разум: {faction.Mind.Describe(name => name)}",
                    faction.LastDeed != null ? $"Последнее деяние: {faction.LastDeed}" : null
                };
                var land = faction.Lands.Cast<GridCoord?>().FirstOrDefault();
                yield return new WorldReportLine(faction.Definition.DisplayName, string.Join("\n", parts.Where(part => part != null)), land, new[]
                {
                    _commands.Make("saga-empower", "saga", () =>
                    {
                        _keeper.Empower(faction, _commands.Config.SagaPush);
                        return land ?? _keeper.World.World.Grid.Center;
                    }, $"Склонить чашу: {faction.Definition.DisplayName}")
                });
            }

            foreach (var transition in act.Transitions)
                yield return new WorldReportLine(
                    $"  Возможный поворот: {transition.Text}",
                    $"{transition.Condition?.Describe(_keeper)}. Шанс {transition.ChancePercent}%." + (transition.EndsSaga ? " Сага кончится." : string.Empty));
        }
    }
}
