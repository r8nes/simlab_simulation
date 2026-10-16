using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.People;
using Godland.Core.World.Timing;
using Godland.Core.Powers.Titans;
using Godland.Game.Powers.Gods;
using Godland.Game.Powers.Titans;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class GodsReport : IWorldReport
    {
        private readonly Pantheon _pantheon;
        private readonly Temples _temples;
        private readonly Shrines _shrines;
        private readonly GodsConfig _config;
        private readonly TitanHost _titans;
        private readonly TitanConfig _titanConfig;
        private readonly HeroSaga _heroes;
        private readonly WorldClock _clock;

        private readonly EntityCommands _commands;
        private readonly Godland.Core.World.Tiles.WorldTiles _tiles;

        public GodsReport(Pantheon pantheon, Temples temples, Shrines shrines, GodsConfig config, TitanHost titans, TitanConfig titanConfig, HeroSaga heroes, WorldClock clock, EntityCommands commands, Godland.Core.World.Tiles.WorldTiles tiles)
        {
            _commands = commands;
            _tiles = tiles;
            _pantheon = pantheon;
            _temples = temples;
            _shrines = shrines;
            _config = config;
            _titans = titans;
            _titanConfig = titanConfig;
            _heroes = heroes;
            _clock = clock;
        }

        public string Title => "Боги";

        public IEnumerable<WorldReportLine> Lines()
        {
            if (_pantheon.IsSuppressed)
                yield return new WorldReportLine("Боги не рождаются", "Их рождение подавлено: правитель мира, Век титанов, Эпоха духов или Сумерки богов.");

            if (_pantheon.Gods.Count == 0)
            {
                yield return new WorldReportLine("Богов нет", $"Боги рождаются с эпохи {_config.GodsFromEpoch} из стихий: чем больше их тайлов и поселений рядом, тем сильнее давление.");
                yield break;
            }

            foreach (var god in _pantheon.Gods)
            {
                var definition = god.HeroName == null ? _config.DefinitionOf(god.Rule.Id) : null;
                var threats = _titans.All.Where(titan => System.Math.Abs(titan.Position.X - god.Seat.X) + System.Math.Abs(titan.Position.Y - god.Seat.Y) <= 3)
                    .Select(titan => $"титан {_titanConfig.DefinitionOf(titan.Rule.Id).DisplayName} рядом")
                    .Concat(_heroes.All.Where(hero => hero.Quest == QuestKind.God && hero.Target == god.Seat).Select(hero => $"герой {hero.Name} идёт убить его"))
                    .ToList();
                var parts = new List<string>
                {
                    $"Родился в {god.BornYear} году ({_clock.Year - god.BornYear} лет назад)",
                    $"Верующих поселений: {_shrines.FollowersOf(god.Religion).Count}, храмов: {_temples.CountOf(god)}",
                    definition != null ? $"Деяние: {definition.DeedName}, следующее через {god.YearsToAct} лет" : $"Следующее деяние через {god.YearsToAct} лет",
                    $"Пыл веры: {god.Religion.Fervor}",
                    threats.Count > 0 ? "Угрозы: " + string.Join(", ", threats) : null
                };
                var key = $"god:{god.Seat}";
                yield return new WorldReportLine(_config.NameOf(god), string.Join("\n", parts.Where(part => part != null)), god.Seat, new[]
                {
                    _commands.MakeTargeted("god-deed", key, cell => _tiles.All.ContainsKey(cell), cell => _pantheon.Command(god, cell) ? cell : null),
                    god.SilentUntil > _clock.Year ? null : _commands.Make("god-silence", key, () =>
                    {
                        _pantheon.Silence(god, _commands.Config.GodSilenceYears);
                        return god.Seat;
                    })
                });
            }
        }
    }
}
