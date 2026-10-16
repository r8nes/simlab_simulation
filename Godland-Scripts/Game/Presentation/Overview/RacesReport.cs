using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.Life.Races;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class RacesReport : IWorldReport
    {
        private readonly RaceKeeper _races;
        private readonly WonderAge _wonders;
        private readonly Settlements _settlements;

        private readonly EntityCommands _commands;
        private readonly Godland.Core.World.Tiles.WorldTiles _tiles;
        private readonly WorldGrid _grid;

        public RacesReport(RaceKeeper races, WonderAge wonders, Settlements settlements, EntityCommands commands, Godland.Core.World.Tiles.WorldTiles tiles, WorldGrid grid)
        {
            _commands = commands;
            _tiles = tiles;
            _grid = grid;
            _races = races;
            _wonders = wonders;
            _settlements = settlements;
        }

        public string Title => "Народы";

        public IEnumerable<WorldReportLine> Lines()
        {
            var living = _races.Living.ToList();
            yield return new WorldReportLine(
                _wonders.IsActive ? "Идёт Эпоха чудес" : _wonders.HasHappened ? "Эпоха чудес прошла" : "Эпоха чудес впереди",
                _wonders.IsActive ? "Народы воюют и дружат, мир рождает невиданное вдвое чаще."
                    : _wonders.HasHappened ? $"Была один раз за забег. Живых народов: {living.Count}."
                    : $"Наступит, когда живых народов станет 3 (сейчас {living.Count}).");

            foreach (var race in _races.All)
            {
                var definition = race.Definition;
                if (!race.IsAlive)
                {
                    yield return new WorldReportLine($"{race.Name} (исчезли)", $"Жили с {race.BornYear} года. {race.Origin}");
                    continue;
                }

                var population = race.Cells.Sum(cell => _settlements.TryGet(cell, out var count) ? count : 0);
                var parts = new List<string>
                {
                    $"Земель: {race.Cells.Count}, жителей: {population}" + (race.IsVeteran ? ", ветераны Эпохи чудес" : string.Empty),
                    $"Появились в {race.BornYear} году: {race.Origin}",
                    $"Разум: {race.Mind.Describe(action => action)}",
                    $"Геном: {race.Genome.Describe()}" + (race.Parent != null ? $". Откололись от народа «{race.Parent.Name}»" : string.Empty),
                    race.LastDeed != null ? $"Последнее деяние: {race.LastDeed}" : null,
                    definition.Rivals.Count > 0 ? "Враги: " + string.Join(", ", definition.Rivals.Select(rival => rival.DisplayName)) : null,
                    definition.Friends.Count > 0 ? "Друзья: " + string.Join(", ", definition.Friends.Select(friend => friend.DisplayName)) : null
                };
                var home = race.Cells.OrderByDescending(cell => _settlements.TryGet(cell, out var count) ? count : 0).Cast<Godland.Core.World.Grid.GridCoord?>().FirstOrDefault();
                var key = $"race:{race.Name}";
                var commands = new List<EntityCommand>
                {
                    _commands.MakeTargeted("race-migrate", key,
                        cell => _tiles.All.ContainsKey(cell) && !_settlements.TryGet(cell, out _) && _grid.Neighbors(cell).Any(race.Cells.Contains),
                        cell => _races.Migrate(race, cell) ? cell : null)
                };
                commands.AddRange(definition.Actions.Select(action => _commands.Make("race-suggest", key, () =>
                {
                    _races.Suggest(race, action.Name);
                    return home;
                }, $"Подсказать: {action.Name}")));
                yield return new WorldReportLine(race.Name, string.Join("\n", parts.Where(part => part != null)), home, commands);
            }

            if (_races.All.Count == 0)
                yield return new WorldReportLine("Народов пока нет", "Народ создаёт бог, проживший 150 лет с храмом, или рождают обстоятельства: что люди едят, что переживают.");
        }
    }
}
