using System;
using System.Linq;
using Godland.Core.Fate.Anomalies;
using Godland.Core.Fate.Emergence;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Tiles;
using Godland.Game.Powers.Sovereigns;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.World.Volcanoes;
using Godland.Game.Writer.Actions;
using VContainer.Unity;

namespace Godland.Game.Fate.Emergence
{
    public sealed class MemoryRecorder : IDisposable
    {
        private const int EruptionReach = 2;

        private readonly WorldMemory _memory;
        private readonly TileActions _actions;
        private readonly FeatureInteractions _interactions;
        private readonly VolcanoActivity _volcanoes;
        private readonly Realms _realms;
        private readonly Feuds _feuds;
        private readonly Plague _plague;
        private readonly HeroSaga _heroes;
        private readonly Settlements _settlements;
        private readonly WorldTiles _tiles;
        private readonly Shrines _shrines;
        private readonly Pantheon _pantheon;
        private readonly SovereignKeeper _sovereigns;
        private readonly Devourer _devourer;
        private readonly WorldGrid _grid;
        private readonly Godland.Game.World.Tiles.TilePlacement _placement;
        private readonly Godland.Game.World.Evolution.TileEvolutions _evolutions;
        private readonly Godland.Game.Life.Features.TileUpgrades _upgrades;
        private readonly Godland.Core.World.Creation.CellPurchase _purchase;
        private readonly Godland.Core.Powers.Titans.TitanHost _titans;
        private readonly Godland.Game.Fate.Anomalies.AnomalyKeeper _anomalies;
        private readonly System.Collections.Generic.Dictionary<GridCoord, string> _lastTile = new();
        private static readonly System.Collections.Generic.HashSet<string> Waters = new() { "water", "sea", "ocean", "river" };
        private const string Forest = "forest";
        private const string AncientForest = "ancient_forest";

        public MemoryRecorder(
            WorldMemory memory, TileActions actions, FeatureInteractions interactions, VolcanoActivity volcanoes, Realms realms, Feuds feuds,
            Plague plague, HeroSaga heroes, Settlements settlements, WorldTiles tiles, Shrines shrines, Pantheon pantheon,
            SovereignKeeper sovereigns, Devourer devourer, WorldGrid grid, Godland.Game.World.Tiles.TilePlacement placement,
            Godland.Game.World.Evolution.TileEvolutions evolutions, Godland.Game.Life.Features.TileUpgrades upgrades, Godland.Core.World.Creation.CellPurchase purchase,
            Godland.Core.Powers.Titans.TitanHost titans, Godland.Game.Fate.Anomalies.AnomalyKeeper anomalies)
        {
            _titans = titans;
            _anomalies = anomalies;
            _placement = placement;
            _evolutions = evolutions;
            _upgrades = upgrades;
            _purchase = purchase;
            _tiles = tiles;
            _shrines = shrines;
            _pantheon = pantheon;
            _sovereigns = sovereigns;
            _devourer = devourer;
            _grid = grid;
            _memory = memory;
            _actions = actions;
            _interactions = interactions;
            _volcanoes = volcanoes;
            _realms = realms;
            _feuds = feuds;
            _plague = plague;
            _heroes = heroes;
            _settlements = settlements;
            _actions.Applied += OnMiracle;
            _interactions.ThreatStarted += OnHunt;
            _volcanoes.Erupted += OnEruption;
            _realms.Conquered += OnConquered;
            _feuds.Raided += OnRaided;
            _plague.Recovered += OnRecovered;
            _heroes.Triumphed += OnTriumph;
            _heroes.Born += OnHeroBorn;
            _realms.WarDeclared += OnWar;
            _interactions.Resolved += OnDevoured;
            _plague.Outbreak += OnOutbreak;
            _plague.Spread += OnPlagueSpread;
            _shrines.BeliefChanged += OnBeliefChanged;
            _pantheon.Fallen += OnGodFell;
            _sovereigns.Fell += OnTyrantFell;
            _devourer.Ate += OnDevourerAte;
            _tiles.Changed += OnTileChanged;
            _placement.Placed += OnPlayerAct;
            _evolutions.Evolved += OnPlayerAct;
            _upgrades.Applied += OnPlayerAct;
            _purchase.Purchased += OnPlayerBought;
            _anomalies.Began += OnAnomaly;
        }

        public void Dispose()
        {
            _actions.Applied -= OnMiracle;
            _interactions.ThreatStarted -= OnHunt;
            _volcanoes.Erupted -= OnEruption;
            _realms.Conquered -= OnConquered;
            _feuds.Raided -= OnRaided;
            _plague.Recovered -= OnRecovered;
            _heroes.Triumphed -= OnTriumph;
            _heroes.Born -= OnHeroBorn;
            _realms.WarDeclared -= OnWar;
            _interactions.Resolved -= OnDevoured;
            _plague.Outbreak -= OnOutbreak;
            _plague.Spread -= OnPlagueSpread;
            _shrines.BeliefChanged -= OnBeliefChanged;
            _pantheon.Fallen -= OnGodFell;
            _sovereigns.Fell -= OnTyrantFell;
            _devourer.Ate -= OnDevourerAte;
            _tiles.Changed -= OnTileChanged;
            _placement.Placed -= OnPlayerAct;
            _evolutions.Evolved -= OnPlayerAct;
            _upgrades.Applied -= OnPlayerAct;
            _purchase.Purchased -= OnPlayerBought;
            _anomalies.Began -= OnAnomaly;
        }

        private void OnAnomaly(Godland.Game.Fate.Anomalies.AnomalyDefinition anomaly) => _memory.Add(_grid.Center, $"anomaly:{anomaly.name}");

        private void OnPlayerAct<T>(GridCoord cell, T _) => _memory.Forget(_grid.Center, "quiet", 0);

        private void OnPlayerBought(GridCoord cell) => _memory.Forget(_grid.Center, "quiet", 0);

        private void OnHeroBorn(Hero hero) => _memory.Add(hero.Home, "hero-born");

        private void OnWar(War war)
        {
            _memory.Add(war.First.Capital, "kingdom-war");
            _memory.Add(war.Second.Capital, "kingdom-war");
        }

        private void OnDevoured(GridCoord prey, FeatureInteraction interaction, bool countered)
        {
            if (!countered) _memory.Add(prey, "devoured");
        }

        private void OnOutbreak(GridCoord settlement, System.Collections.Generic.IReadOnlyList<string> causes) => _memory.Add(_grid.Center, "plague");

        private void OnPlagueSpread(GridCoord from, GridCoord to) => _memory.Add(_grid.Center, "plague");

        private void OnBeliefChanged(GridCoord settlement, Religion previous, Religion current, BeliefChange reason)
        {
            if (reason == BeliefChange.Schism && current?.Shrine is { } shrine) _memory.Add(shrine, "schism");
        }

        private void OnGodFell(God god)
        {
            _memory.Add(_grid.Center, $"god-fell:{god.Rule.Id}");
            _memory.Add(_grid.Center, "gods-fallen");
        }

        private void OnTyrantFell(SovereignDefinition sovereign) => _memory.Add(_grid.Center, "tyrant-fell");

        private void OnDevourerAte(GridCoord cell) => _memory.Add(_grid.Center, "devoured-land");

        private void OnTileChanged(GridCoord cell, string tileId, string cause)
        {
            if (_lastTile.TryGetValue(cell, out var previous) && (previous == Forest || previous == AncientForest) && tileId != previous)
                _memory.Add(cell, "felled");
            if (cause == "море поглотило воду") _memory.Add(_grid.Center, "sea-rose");
            if (tileId == null) _lastTile.Remove(cell);
            else _lastTile[cell] = tileId;
        }

        private void OnMiracle(GridCoord cell, TileActionDefinition action)
        {
            _memory.Forget(_grid.Center, "quiet", 0);
            if (action.FaithCost <= 0) return;

            _memory.Add(cell, "miracle");
            _titans.Disturb(cell);
            _memory.Add(_grid.Center, "miracles-total");
        }

        private void OnHunt(GridCoord prey, FeatureThreat threat)
        {
            _memory.Add(prey, $"hunt:{threat.Interaction.PredatorId}");
            if (_tiles.TryGet(prey, out var tileId) && Waters.Contains(tileId)) _memory.Add(prey, "hunt-water");
        }

        private void OnEruption(GridCoord volcano, System.Collections.Generic.IReadOnlyList<GridCoord> burned)
        {
            _memory.Add(volcano, "volcano-erupted");
            foreach (var settlement in _settlements.All.Keys.Where(cell =>
                         Math.Max(Math.Abs(cell.X - volcano.X), Math.Abs(cell.Y - volcano.Y)) <= EruptionReach).ToList())
                _memory.Add(settlement, "eruption");
        }

        private void OnConquered(Kingdom attacker, Kingdom defender, GridCoord cell)
        {
            _memory.Add(cell, "war");
            _titans.Disturb(cell);
        }

        private void OnRaided(GridCoord attacker, GridCoord defender, bool conquered) => _memory.Add(defender, "war");

        private void OnRecovered(GridCoord settlement) => _memory.Add(settlement, "plague-survived");

        private void OnTriumph(Hero hero)
        {
            if (hero.Quest == QuestKind.Monster) _memory.Add(hero.Home, "monster-slain");
        }
    }
}
