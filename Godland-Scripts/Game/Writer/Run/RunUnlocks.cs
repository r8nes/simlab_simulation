using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Writer.Actions;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;

namespace Godland.Game.Writer.Run
{
    public sealed class RunUnlocks
    {
        private readonly RunConfig _config;
        private readonly List<TileActionDefinition> _actions = new();
        private readonly List<FeatureDefinition> _upgrades = new();
        private readonly Dictionary<string, List<TileEvolutionDefinition>> _evolutions = new();

        public RunUnlocks(RunConfig config)
        {
            _config = config;
        }

        public event Action Changed;

        public IEnumerable<TileActionDefinition> Actions => _config.BaseActions.Concat(_actions);

        public IEnumerable<FeatureDefinition> Upgrades => _config.BaseUpgrades.Concat(_upgrades);

        public IEnumerable<TileEvolutionDefinition> EvolutionsFor(TileDefinition tile) =>
            _evolutions.TryGetValue(tile.Id, out var unlocked) ? tile.Evolutions.Concat(unlocked) : tile.Evolutions;

        public void UnlockAction(TileActionDefinition action) => AddUnique(_actions, action);

        public void UnlockUpgrade(FeatureDefinition upgrade) => AddUnique(_upgrades, upgrade);

        public void UnlockEvolution(TileDefinition tile, TileEvolutionDefinition evolution)
        {
            if (!_evolutions.TryGetValue(tile.Id, out var list)) _evolutions[tile.Id] = list = new List<TileEvolutionDefinition>();
            AddUnique(list, evolution);
        }

        private void AddUnique<T>(List<T> list, T item)
        {
            if (list.Contains(item)) return;

            list.Add(item);
            Changed?.Invoke();
        }
    }
}
