using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Timing;
using Godland.Core.Powers.Titans;
using Godland.Game.Powers.Gods;
using Godland.Game.Powers.Titans;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class TitansReport : IWorldReport
    {
        private readonly TitanHost _titans;
        private readonly TitanConfig _config;
        private readonly GodsConfig _gods;
        private readonly WorldClock _clock;

        private readonly EntityCommands _commands;
        private readonly Godland.Core.World.Tiles.WorldTiles _tiles;

        public TitansReport(TitanHost titans, TitanConfig config, GodsConfig gods, WorldClock clock, EntityCommands commands, Godland.Core.World.Tiles.WorldTiles tiles)
        {
            _commands = commands;
            _tiles = tiles;
            _titans = titans;
            _config = config;
            _gods = gods;
            _clock = clock;
        }

        public string Title => "Титаны";

        public IEnumerable<WorldReportLine> Lines()
        {
            if (_titans.All.Count == 0)
            {
                yield return new WorldReportLine("Титанов нет", "Мир спокоен. Титаны рождаются из памяти мира, заповеди и некоторых эпох.");
                yield break;
            }

            foreach (var titan in _titans.All)
            {
                var name = _config.DefinitionOf(titan.Rule.Id).DisplayName;
                var parts = new List<string>
                {
                    State(titan),
                    $"Цель: {GoalOf(titan.Rule.Target)}",
                    titan.IsAwake && !titan.IsTamed ? $"Осталось жить: {titan.Rule.Lifespan - (_clock.Year - titan.BornYear)} лет" : null,
                    titan.Pacts.Count > 0 ? "Договор с богами: " + string.Join(", ", titan.Pacts.Select(_gods.NameOf)) : null,
                    titan.Rule.Infects ? "Несёт мор" : null,
                    $"Пробудился в {titan.BornYear} году" + (string.IsNullOrEmpty(titan.Origin) ? string.Empty : $": {titan.Origin}")
                };
                var key = $"titan:{titan.Rule.Id}:{titan.BornYear}";
                var free = !titan.IsTamed;
                yield return new WorldReportLine(name, string.Join("\n", parts.Where(part => part != null)), titan.Position, new[]
                {
                    free && titan.IsAwake ? _commands.MakeTargeted("titan-direct", key, cell => _tiles.All.ContainsKey(cell), cell =>
                    {
                        _titans.Direct(titan, cell);
                        return cell;
                    }) : null,
                    free && titan.IsAwake ? _commands.Make("titan-lull", key, () =>
                    {
                        _titans.Lull(titan, _commands.Config.TitanSleepYears);
                        return titan.Position;
                    }) : null,
                    free && !titan.IsAwake ? _commands.Make("titan-rouse", key, () =>
                    {
                        _titans.Rouse(titan);
                        return titan.Position;
                    }) : null
                });
            }
        }

        private static string State(Titan titan) =>
            titan.IsTamed ? "Усмирён: стоит и охраняет землю вокруг" : titan.IsAwake ? "Бодрствует и идёт по миру" : "Спит. Проснётся от чудес и войн рядом";

        private static string GoalOf(TitanTarget target) => target switch
        {
            TitanTarget.Settlements => "поселения",
            TitanTarget.GodSeats => "престолы богов",
            TitanTarget.Edges => "край мира",
            TitanTarget.Sleep => "спит на месте",
            _ => "бродит без цели"
        };
    }
}
