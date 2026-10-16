using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Heroes;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class HeroesReport : IWorldReport
    {
        private readonly HeroSaga _heroes;
        private readonly EntityCommands _commands;
        private readonly Godland.Core.Life.People.Settlements _settlements;

        public HeroesReport(HeroSaga heroes, EntityCommands commands, Godland.Core.Life.People.Settlements settlements)
        {
            _heroes = heroes;
            _commands = commands;
            _settlements = settlements;
        }

        public string Title => "Герои";

        public IEnumerable<WorldReportLine> Lines()
        {
            yield return new WorldReportLine("Призвать героя", "Писатель может вызвать героя своей эпохи в любом поселении, где героя ещё нет.", null, new[]
            {
                _commands.MakeTargeted("hero-summon", "hero-summon", cell => _settlements.TryGet(cell, out _), cell => _heroes.Summon(cell) ? cell : null)
            });

            if (_heroes.All.Count == 0)
            {
                yield return new WorldReportLine("Героев нет", "Герои рождаются в больших городах с Античности.");
                yield break;
            }

            foreach (var hero in _heroes.All)
            {
                var parts = new List<string>
                {
                    $"{TraitOf(hero.Trait)}, возраст {hero.Age}, слава {hero.Glory}" + (hero.Blessed ? ", благословлён" : string.Empty) + (hero.IsDemigod ? $", полубог, легенд: {hero.Legends}" : string.Empty),
                    $"Цель: {QuestOf(hero.Quest)}" + (hero.Target is { } target ? $" в клетке ({target.X}, {target.Y})" : string.Empty),
                    $"Родной город: ({hero.Home.X}, {hero.Home.Y})",
                    hero.Companions.Count > 0 ? "Отряд: " + string.Join(", ", hero.Companions.Select(companion => companion.Name)) : null
                };
                var key = $"hero:{hero.Name}";
                yield return new WorldReportLine(hero.Name, string.Join("\n", parts.Where(part => part != null)), hero.Position, new[]
                {
                    _commands.MakeTargeted("hero-guide", key, cell => _heroes.CanGuide(hero, cell), cell => _heroes.Guide(hero, cell) ? cell : null),
                    _commands.MakeTargeted("hero-meet", key, cell => _heroes.TryGetAt(cell, out var other) && other != hero,
                        cell => _heroes.TryGetAt(cell, out var other) && _heroes.Introduce(hero, other) ? cell : null),
                    _commands.Make("hero-recall", key, () =>
                    {
                        _heroes.Recall(hero);
                        return hero.Home;
                    })
                });
            }
        }

        private static string TraitOf(HeroTrait trait) => trait switch
        {
            HeroTrait.Brave => "Храбрый",
            HeroTrait.Reckless => "Безрассудный",
            HeroTrait.Wise => "Мудрый",
            _ => "Гордый"
        };

        private static string QuestOf(QuestKind quest) => quest switch
        {
            QuestKind.Monster => "чудовище",
            QuestKind.Corruption => "Источник порчи",
            QuestKind.God => "бог чужой веры",
            QuestKind.War => "война",
            QuestKind.Titan => "титан",
            _ => "нет, ждёт подвига"
        };
    }
}
