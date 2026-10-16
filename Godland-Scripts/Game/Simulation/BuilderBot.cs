using Godland.Core.Powers.Babel;
using Godland.Core.Life.Heroes;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Game.Writer.Actions;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.Writer.Gifts;
using Godland.Game.World.Tiles;

namespace Godland.Game.Simulation
{
    public sealed class BuilderBot : ISimulationBot
    {
        private const int ActPeriod = 5;

        private readonly bool _usesMiracles;

        public BuilderBot(bool usesMiracles)
        {
            _usesMiracles = usesMiracles;
        }

        public string Name => _usesMiracles ? "Чудотворец" : "Строитель";

        public void Act(int year, SimulatedWorld world, SeededRandom random)
        {
            if (year % ActPeriod != 0) return;

            Evolve(world, random);
            Build(world, random);
            Upgrade(world, random);
            if (_usesMiracles) Miracle(world, random);
        }

        public GiftDefinition ChooseGift(IReadOnlyList<GiftDefinition> offered, SeededRandom random) => offered[random.Next(offered.Count)];

        private static void Evolve(SimulatedWorld world, SeededRandom random)
        {
            var tiles = world.Get<WorldTiles>();
            var evolutions = world.Get<TileEvolutions>();
            foreach (var coord in tiles.All.Keys.ToList())
            {
                var ready = evolutions.OptionsFor(coord).Where(option => evolutions.IsReady(coord, option)).ToList();
                if (ready.Count > 0 && random.Next(2) == 0) evolutions.TryEvolve(coord, ready[random.Next(ready.Count)]);
            }
        }

        private static void Build(SimulatedWorld world, SeededRandom random)
        {
            var grid = world.Get<WorldGrid>();
            var tiles = world.Get<WorldTiles>();
            var placement = world.Get<TilePlacement>();
            var brush = world.Get<TileBrush>();
            var purchase = world.Get<CellPurchase>();

            for (var spins = random.Next(3); spins > 0; spins--) brush.Next();

            var free = grid.Coords.Where(tiles.CanPlace).ToList();
            if (free.Count > 0)
            {
                placement.TryPlace(free[random.Next(free.Count)]);
                return;
            }

            var edge = grid.Coords.Where(purchase.CanBuy).ToList();
            if (edge.Count > 0) purchase.TryBuy(edge[random.Next(edge.Count)]);
        }

        private static void Upgrade(SimulatedWorld world, SeededRandom random)
        {
            if (random.Next(4) != 0) return;

            var upgrades = world.Get<TileUpgrades>();
            var options = world.Get<WorldTiles>().All.Keys
                .SelectMany(coord => upgrades.AvailableFor(coord).Where(upgrade => upgrades.CanApply(upgrade, coord)).Select(upgrade => (coord, upgrade)))
                .ToList();
            if (options.Count == 0) return;

            var (target, choice) = options[random.Next(options.Count)];
            upgrades.TryApply(choice, target);
        }

        private static void Miracle(SimulatedWorld world, SeededRandom random)
        {
            var actions = world.Get<TileActions>();
            var faith = world.Get<FaithPoints>();
            var options = world.Get<Settlements>().All.Keys.Append(world.Get<WorldGrid>().Center).Concat(world.Get<HeroSaga>().All.Select(hero => hero.Position)).Append(world.Get<TowerOfBabel>().Site)
                .SelectMany(coord => actions.AvailableFor(coord)
                    .Where(action => action.FaithCost > 0 && actions.CanApply(action, coord) && faith.Amount >= actions.FaithCostAt(action, coord) * 2)
                    .Select(action => (coord, action)))
                .ToList();
            if (options.Count == 0) return;

            var (target, choice) = options[random.Next(options.Count)];
            actions.TryApply(choice, target);
        }
    }

    public sealed class WriterBot : ISimulationBot
    {
        private const int CommandPeriod = 10;

        private readonly BuilderBot _builder = new(true);
        private readonly Dictionary<string, int> _used = new();

        public string Name => "Писатель";

        public void Act(int year, SimulatedWorld world, SeededRandom random)
        {
            _builder.Act(year, world, random);
            if (year % CommandPeriod != 0) return;

            var commands = world.Get<Godland.Game.Writer.Commands.EntityCommands>();
            var available = world.Get<IEnumerable<Godland.Game.Presentation.Overview.IWorldReport>>()
                .SelectMany(report => report.Lines())
                .SelectMany(line => line.Commands)
                .Where(commands.CanIssue)
                .ToList();
            if (available.Count == 0) return;

            var rarest = available.Min(candidate => _used.GetValueOrDefault(candidate.Definition.Id));
            var rare = available.Where(candidate => _used.GetValueOrDefault(candidate.Definition.Id) == rarest).ToList();
            var command = rare[random.Next(rare.Count)];
            _used[command.Definition.Id] = rarest + 1;
            if (!command.NeedsTarget)
            {
                commands.Issue(command);
                return;
            }

            var targets = world.Get<WorldGrid>().Coords.Where(command.Target).ToList();
            if (targets.Count == 0) return;

            commands.Issue(command);
            world.Get<Godland.Game.Writer.Commands.CommandTargeting>().TryPick(targets[random.Next(targets.Count)]);
        }

        public GiftDefinition ChooseGift(IReadOnlyList<GiftDefinition> offered, SeededRandom random) => _builder.ChooseGift(offered, random);
    }
}
