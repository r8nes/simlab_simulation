using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.World.Timing;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class RealmsReport : IWorldReport
    {
        private readonly Realms _realms;
        private readonly Settlements _settlements;
        private readonly FaithNames _names;
        private readonly WorldClock _clock;

        private readonly EntityCommands _commands;

        private readonly Godland.Game.Life.Kingdoms.Dynasties _dynasties;

        private string Cities(Kingdom kingdom)
        {
            var cities = kingdom.Members.Where(cell => _dynasties.TryGetSpecialty(cell, out _))
                .Select(cell => _dynasties.TryGetSpecialty(cell, out var specialty) ? $"{Godland.Game.Life.Kingdoms.Dynasties.NameOf(specialty)} ({cell.X}, {cell.Y})" : null)
                .ToList();
            return cities.Count == 0 ? null : "Города: " + string.Join(", ", cities);
        }

        public RealmsReport(Realms realms, Settlements settlements, FaithNames names, WorldClock clock, EntityCommands commands, Godland.Game.Life.Kingdoms.Dynasties dynasties)
        {
            _dynasties = dynasties;
            _commands = commands;
            _realms = realms;
            _settlements = settlements;
            _names = names;
            _clock = clock;
        }

        public string Title => "Царства";

        public IEnumerable<WorldReportLine> Lines()
        {
            if (_realms.All.Count == 0)
            {
                yield return new WorldReportLine("Царств нет", "Царство основывает город, вокруг которого живут поселения одной с ним веры.");
                yield break;
            }

            foreach (var war in _realms.Wars)
                yield return new WorldReportLine(
                    $"Война: {war.First.Name} против {war.Second.Name}",
                    $"Сильнее: {(Strength(war.First) >= Strength(war.Second) ? war.First.Name : war.Second.Name)} ({Strength(war.First)} против {Strength(war.Second)} жителей)\nКончится не позже {war.EndYear} года ({war.EndYear - _clock.Year} лет).",
                    war.First.Capital);

            foreach (var kingdom in _realms.All)
            {
                var enemies = _realms.Wars.Where(war => war.Involves(kingdom)).Select(war => war.EnemyOf(kingdom).Name).ToList();
                var parts = new List<string>
                {
                    $"Поселений: {kingdom.Members.Count}, жителей: {Strength(kingdom)}",
                    $"Вера: {_names.Of(kingdom.Religion)}",
                    enemies.Count > 0 ? "Воюет с: " + string.Join(", ", enemies) : "В мире со всеми",
                    kingdom.Strikes > 0 ? $"Слабеет: {kingdom.Strikes} проверок подряд мало поселений" : null,
                    _dynasties.RulerOf(kingdom) is { } ruler ? $"Правит {ruler.Title}, {Godland.Game.Life.Kingdoms.Dynasties.NameOf(ruler.Trait)}, на троне {_clock.Year - ruler.CrownedYear} лет" : null,
                    _dynasties.WondersOf(kingdom).Count > 0 ? "Чудеса света: " + string.Join(", ", _dynasties.WondersOf(kingdom).Select(wonder => wonder.Name)) : null,
                    Cities(kingdom)
                };
                var key = $"realm:{kingdom.Name}";
                yield return new WorldReportLine($"Царство {kingdom.Name}", string.Join("\n", parts.Where(part => part != null)), kingdom.Capital, new[]
                {
                    _realms.IsAtWar(kingdom) ? null : _commands.MakeTargeted("realm-war", key,
                        cell => _realms.TryGetAt(cell, out var other) && other != kingdom,
                        cell => _realms.TryGetAt(cell, out var other) && _realms.TryDeclareWar(kingdom, other) ? cell : null),
                    _realms.IsAtWar(kingdom) ? _commands.Make("realm-peace", key, () => _realms.TryMakePeace(kingdom) ? kingdom.Capital : null) : null
                });
            }
        }

        private int Strength(Kingdom kingdom) => kingdom.Members.Sum(cell => _settlements.TryGet(cell, out var population) ? population : 0);
    }
}
