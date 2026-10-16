using System;
using System.Collections.Generic;
using Godland.Core.World.Creation;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.Writer.Run;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.Writer.Actions
{
    public sealed class TileActionsPresenter : IStartable, IDisposable
    {
        private readonly TileSelection _selection;
        private readonly TileActions _actions;
        private readonly CreationPoints _creation;
        private readonly WorldTiles _tiles;
        private readonly TileActionsView _view;
        private readonly TileEvolutions _evolutions;
        private readonly WorldClock _clock;
        private readonly TileUpgrades _upgrades;
        private readonly TileFeatures _features;
        private readonly RunUnlocks _unlocks;
        private readonly HintSettings _hints;

        public TileActionsPresenter(
            TileSelection selection, TileActions actions, CreationPoints creation, WorldTiles tiles, TileActionsView view, TileEvolutions evolutions, WorldClock clock,
            TileUpgrades upgrades, TileFeatures features, RunUnlocks unlocks,
            HintSettings hints)
        {
            _selection = selection;
            _actions = actions;
            _creation = creation;
            _tiles = tiles;
            _view = view;
            _evolutions = evolutions;
            _clock = clock;
            _upgrades = upgrades;
            _features = features;
            _unlocks = unlocks;
            _hints = hints;
        }

        public void Start()
        {
            Refresh();
            _selection.Changed += Refresh;
            _creation.Changed += OnCreationChanged;
            _tiles.Placed += OnPlaced;
            _tiles.Removed += OnRemoved;
            _actions.Applied += OnApplied;
            _evolutions.Evolved += OnEvolved;
            _clock.Ticked += OnTicked;
            _features.Added += OnFeatureChanged;
            _unlocks.Changed += Refresh;
            _hints.Changed += Refresh;
            _features.Removed += OnFeatureChanged;
        }

        public void Dispose()
        {
            _selection.Changed -= Refresh;
            _creation.Changed -= OnCreationChanged;
            _tiles.Placed -= OnPlaced;
            _tiles.Removed -= OnRemoved;
            _actions.Applied -= OnApplied;
            _evolutions.Evolved -= OnEvolved;
            _clock.Ticked -= OnTicked;
            _features.Added -= OnFeatureChanged;
            _unlocks.Changed -= Refresh;
            _hints.Changed -= Refresh;
            _features.Removed -= OnFeatureChanged;
        }

        private void OnCreationChanged(int _) => Refresh();

        private void OnTicked(int _) => Refresh();

        private void OnRemoved(GridCoord _) => Refresh();

        private void OnFeatureChanged(GridCoord coord, string _)
        {
            if (_selection.Selected == coord) Refresh();
        }

        private void OnApplied(GridCoord _, TileActionDefinition __) => Refresh();

        private void OnEvolved(GridCoord coord, TileDefinition _) => Refresh();

        private void OnPlaced(GridCoord coord, string _)
        {
            if (_selection.Selected == coord) Refresh();
        }

        private void Refresh()
        {
            var evolution = new ActionSection("Эволюция");
            var faith = new ActionSection("Вера");
            var additions = new ActionSection("Добавления");
            if (_selection.Selected is { } coord)
            {
                CollectActions(coord, faith, additions);
                CollectUpgrades(coord, additions);
                CollectEvolutions(coord, evolution);
            }

            _view.Show(new[] { evolution, faith, additions });
        }

        private void CollectActions(GridCoord coord, ActionSection faith, ActionSection additions)
        {
            foreach (var action in _actions.AvailableFor(coord))
            {
                var cooldown = _actions.CooldownLeft(action);
                var label = _actions.IsUsed(action, coord)
                    ? $"{action.DisplayName}: использовано"
                    : cooldown > 0
                        ? $"{action.DisplayName}: снова через {cooldown} лет"
                        : $"{action.DisplayName} ({action.CostText(_actions.FaithCostAt(action, coord))}): {_actions.DescribeAt(action, coord)}";
                var section = action.FaithCost > 0 ? faith : additions;
                section.Entries.Add(new ActionEntry(label, _actions.CanApply(action, coord), () => _actions.TryApply(action, coord)));
            }
        }

        private void CollectUpgrades(GridCoord coord, ActionSection additions)
        {
            foreach (var upgrade in _upgrades.AvailableFor(coord))
            {
                var label = $"+ {upgrade.DisplayName} ({upgrade.Cost}): {upgrade.Describe()}";
                additions.Entries.Add(new ActionEntry(label, _upgrades.CanApply(upgrade, coord), () => _upgrades.TryApply(upgrade, coord)));
            }
        }

        private void CollectEvolutions(GridCoord coord, ActionSection evolution)
        {
            foreach (var option in _evolutions.OptionsFor(coord))
            {
                var missing = _evolutions.MissingConditions(coord, option);
                if (missing.Count > 0 && !_hints.ShowHints) continue;
                var label = missing.Count == 0
                    ? $"→ {option.Result.DisplayName}{Describe(option.Result)}"
                    : $"→ {option.Result.DisplayName} (нужно: {string.Join(", ", missing)})";
                evolution.Entries.Add(new ActionEntry(label, missing.Count == 0, () => _evolutions.TryEvolve(coord, option)));
            }
        }

        private static string Describe(TileDefinition tile) =>
            tile.NeighborFertility > 0 ? $": соседям +{tile.NeighborFertility} плодородия в год" : string.Empty;
    }
}
