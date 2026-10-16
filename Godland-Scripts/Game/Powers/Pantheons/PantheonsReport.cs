using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Timing;
using Godland.Game.Writer.Commands;
using Godland.Game.Powers.Gods;
using Godland.Game.Powers.Pantheons;

namespace Godland.Game.Presentation.Overview
{
    public sealed class PantheonsReport : IWorldReport
    {
        private readonly PantheonKeeper _keeper;
        private readonly PantheonConfig _config;
        private readonly GodsConfig _gods;
        private readonly Shrines _shrines;
        private readonly WorldClock _clock;
        private readonly EntityCommands _commands;

        public PantheonsReport(PantheonKeeper keeper, PantheonConfig config, GodsConfig gods, Shrines shrines, WorldClock clock, EntityCommands commands)
        {
            _keeper = keeper;
            _config = config;
            _gods = gods;
            _shrines = shrines;
            _clock = clock;
            _commands = commands;
        }

        public string Title => "Пантеоны";

        public IEnumerable<WorldReportLine> Lines()
        {
            var active = _keeper.Registry.Active.ToList();
            if (active.Count == 0)
                yield return new WorldReportLine("Пантеонов нет", "Пантеоны приходят с Эпохи богов: их зовёт земля (снег и войны — асы, реки и пустыни — Египет, леса — славяне, моря — греки…) или раскрытый идол.");

            foreach (var house in active)
            {
                var parts = new List<string>
                {
                    house.Definition.Text,
                    $"Глава: {_gods.NameOf(house.Head)}",
                    "Боги: " + string.Join(", ", house.Gods.Select(_gods.NameOf)),
                    $"Верующих поселений: {_shrines.FollowersOf(house.Faith).Count} ({_keeper.SharePercent(house)}% мира), с {house.ArrivedYear} года",
                    house.LastDeed != null ? $"Последнее деяние: {house.LastDeed}" : null,
                    Relations(house),
                    house.Facts.Count > 0 ? "Свершилось: " + string.Join(", ", house.Facts) : null,
                    Plans(house)
                };
                var key = $"pantheon:{house.Definition.Id}";
                yield return new WorldReportLine(house.Definition.DisplayName, string.Join("\n", parts.Where(part => part != null)), house.Head?.Seat, new[]
                {
                    _commands.Make("pantheon-invoke", key, () => _keeper.Invoke(house) ? house.Head?.Seat : null),
                    active.Count > 1 ? _commands.MakeTargeted("pantheon-court", key, cell => OtherAt(house, cell) != null, cell => Shift(house, cell, _config.CommandRelation)) : null,
                    active.Count > 1 ? _commands.MakeTargeted("pantheon-strife", key, cell => OtherAt(house, cell) != null, cell => Shift(house, cell, -_config.CommandRelation)) : null
                });
            }

            var waiting = _config.Pantheons.Where(definition => definition != null && _keeper.Registry.Find(definition.Id) == null).Select(definition => definition.DisplayName).ToList();
            if (waiting.Count > 0)
                yield return new WorldReportLine("Ещё не пришли", string.Join(", ", waiting));
        }

        private string Plans(PantheonHouse house)
        {
            var plans = house.Schemes.Where(pair => pair.Value.Plan.Count > 0)
                .Select(pair => $"{pair.Key.HeroName?.Split(',')[0]} задумал: {string.Join(" → ", pair.Value.Plan.Select(action => action.Name))}")
                .ToList();
            return plans.Count == 0 ? null : string.Join("\n", plans);
        }

        private string Relations(PantheonHouse house)
        {
            var others = _keeper.Registry.Active.Where(other => other != house).ToList();
            if (others.Count == 0) return null;

            return "Отношения: " + string.Join(", ", others.Select(other => $"{other.Definition.DisplayName} {BondName(house.BondWith(other))} ({house.RelationWith(other)})"));
        }

        private static string BondName(PantheonBond bond) => bond switch
        {
            PantheonBond.War => "— священная война",
            PantheonBond.Alliance => "— союз",
            _ => "— настороженно"
        };

        private PantheonHouse OtherAt(PantheonHouse house, GridCoord cell) =>
            _shrines.TryGet(cell, out var faith) ? _keeper.Registry.Active.FirstOrDefault(other => other != house && other.Faith == faith) : null;

        private GridCoord? Shift(PantheonHouse house, GridCoord cell, int amount)
        {
            var other = OtherAt(house, cell);
            if (other == null) return null;

            _keeper.Shift(house, other, amount);
            return cell;
        }
    }
}
