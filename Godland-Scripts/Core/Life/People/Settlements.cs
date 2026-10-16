using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.People
{
    public sealed class Settlements
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly CellFertility _fertility;
        private readonly IFeatureRules _featureRules;
        private readonly ISettlementRules _rules;
        private readonly SeededRandom _random;
        private readonly FaithPoints _faith;
        private readonly WorldEpoch _epoch;
        private readonly FeatureDepletion _depletion;
        private Func<GridCoord, bool> _huntTaboo = _ => false;
        private Func<GridCoord, bool> _faithless = _ => false;
        private Func<GridCoord, bool> _sturdy = _ => false;
        private Func<GridCoord, int> _faithMultiplier = _ => 1;
        private readonly Dictionary<GridCoord, int> _population = new();
        private readonly Dictionary<GridCoord, SettlementHunt> _hunts = new();
        private int _year;
        private readonly Dictionary<GridCoord, int> _energy = new();
        private readonly Dictionary<GridCoord, int> _desertedUntil = new();
        private int _nextExodus;

        public Settlements(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, CellFertility fertility,
            IFeatureRules featureRules, ISettlementRules rules, SeededRandom random, FaithPoints faith, WorldEpoch epoch, FeatureDepletion depletion)
        {
            _grid = grid;
            _tiles = tiles;
            _features = features;
            _fertility = fertility;
            _featureRules = featureRules;
            _rules = rules;
            _random = random;
            _faith = faith;
            _epoch = epoch;
            _depletion = depletion;

            _tiles.Removed += Abandon;
            _tiles.Placed += OnTilePlaced;
        }

        public event Action<GridCoord> Founded;
        public event Action<GridCoord, int, int> PopulationChanged;
        public event Action<GridCoord> Abandoned;
        public event Action<GridCoord, SettlementCulture> CultureChanged;
        public event Action<GridCoord, GridCoord> Migrated;
        public event Action<GridCoord, SettlementHunt> HuntStarted;
        public event Action<GridCoord, SettlementHunt> HuntFinished;
        public event Action<GridCoord, GridCoord, string> WorldShaped;
        public event Action<GridCoord, GridCoord> Rescued;
        public event Action<GridCoord, GridCoord> Exodus;

        public bool IsDeathless { get; private set; }

        public void MakeDeathless() => IsDeathless = true;

        public bool IsLandBarren { get; private set; }
        public bool IsHuntingForbidden { get; private set; }
        public bool IsGodless { get; private set; }
        public int FaithBoostPercent { get; private set; } = 100;

        public void MakeLandBarren() => IsLandBarren = true;

        public void ForbidHunting() => IsHuntingForbidden = true;

        public void MakeGodless() => IsGodless = true;

        public void BoostFaith(int percent) => FaithBoostPercent = FaithBoostPercent * percent / 100;

        public IReadOnlyDictionary<GridCoord, int> All => _population;

        public bool TryGet(GridCoord coord, out int population) => _population.TryGetValue(coord, out population);

        public bool TryGetHunt(GridCoord coord, out SettlementHunt hunt) => _hunts.TryGetValue(coord, out hunt);

        public int CapacityOf(GridCoord coord) =>
            Math.Max(1, _rules.BaseCapacity + CultureOf(coord).CapacityBonus + CountFood(coord) * _rules.CapacityPerFood);

        public int LandFood(GridCoord coord)
        {
            if (IsLandBarren || !_fertility.CanHold(coord)) return 0;

            var fertility = _fertility.Get(coord);
            return fertility >= _fertility.Max ? 2 : fertility * 2 >= _fertility.Max ? 1 : 0;
        }

        public bool IsHungry(GridCoord coord) => _population.ContainsKey(coord) && CountFood(coord) == 0;

        public SettlementCulture CultureOf(GridCoord coord) =>
            _tiles.TryGet(coord, out var tileId) ? _rules.GetCulture(tileId) : null;

        public void Update(int year)
        {
            _year = year;
            AdvanceHunts();

            if (year % _rules.GrowthPeriod == 0)
            {
                Grow();
                Cultivate();
                Migrate();
            }

            ShapeWorld(year);
            if (year % _rules.HuntPeriod == 0) StartHunts();
            if (year % _rules.FaithPeriod == 0)
                _faith.Add(_population.Sum(pair => FaithOf(pair.Key, pair.Value)));
            if (year % _rules.SpawnPeriod == 0) TrySpawn();
        }

        public void SetFaithless(Func<GridCoord, bool> faithless) => _faithless = faithless;

        public void SetSturdy(Func<GridCoord, bool> sturdy) => _sturdy = sturdy;

        public void SetFaithMultiplier(Func<GridCoord, int> multiplier) => _faithMultiplier = multiplier;

        public void SetHuntTaboo(Func<GridCoord, bool> taboo) => _huntTaboo = taboo;

        public IReadOnlyList<GridCoord> SettleableCells => _grid.Coords.Where(CanSettle).ToList();

        public bool TrySettle(GridCoord coord, int population)
        {
            if (!CanSettle(coord)) return false;

            Found(coord, population);
            return true;
        }

        public void Bless(GridCoord coord, int amount)
        {
            if (_population.TryGetValue(coord, out var population)) SetPopulation(coord, population + amount);
        }

        public void Weaken(GridCoord coord, int amount)
        {
            if (!_population.TryGetValue(coord, out var population)) return;

            if (population <= amount && (_sturdy(coord) || IsDeathless)) SetPopulation(coord, 1);
            else if (population <= amount && TryRescue(coord)) return;
            else if (population <= amount) Abandon(coord);
            else SetPopulation(coord, population - amount);
        }

        private int FaithOf(GridCoord coord, int population) =>
            IsGodless || _faithless(coord) ? 0 : (population >= _rules.VillagePopulation ? 2 : 1) * CultureOf(coord).FaithMultiplier * _faithMultiplier(coord) * FaithBoostPercent / 100;

        public int EnergyOf(GridCoord coord) => _energy.TryGetValue(coord, out var energy) ? energy : 0;

        private void Grow()
        {
            foreach (var coord in _population.Keys.ToList())
            {
                if (!_population.TryGetValue(coord, out var population)) continue;
                var energy = EnergyOf(coord) + CapacityOf(coord) * _rules.EnergyPerCapacity - population * _rules.UpkeepPerPerson;
                var next = population;
                if (energy >= _rules.BirthCost)
                {
                    next++;
                    energy -= _rules.BirthCost;
                }
                else if (energy <= -_rules.StarveDebt)
                {
                    next--;
                    energy += _rules.StarveDebt;
                }

                _energy[coord] = Math.Clamp(energy, -_rules.StarveDebt, _rules.MaxReserve);
                if (next <= 0) Abandon(coord);
                else if (next != population) SetPopulation(coord, next);
            }
        }

        private void Cultivate()
        {
            foreach (var coord in _population.Keys.ToList())
            {
                var amount = CultureOf(coord).NeighborFertility;
                if (amount == 0) continue;

                foreach (var neighbor in _grid.Neighbors(coord))
                    _fertility.Add(neighbor, amount);
            }
        }

        private void Migrate()
        {
            foreach (var coord in _population.Keys.ToList())
            {
                if (!_population.TryGetValue(coord, out var population)) continue;
                if (population < _rules.MigrationMinPopulation || population < CapacityOf(coord)) continue;

                var targets = _grid.Neighbors(coord).Where(cell => CanSettle(cell) && CountFood(cell) > 0).ToList();
                if (targets.Count == 0)
                {
                    if (IsDeathless) TryExodus(coord, population);
                    continue;
                }

                var target = targets[_random.Next(targets.Count)];
                SetPopulation(coord, population - _rules.MigrationSize);
                Found(target, _rules.MigrationSize);
                Migrated?.Invoke(coord, target);
            }
        }

        private void ShapeWorld(int year)
        {
            foreach (var coord in _population.Keys.ToList())
            {
                var culture = _population.ContainsKey(coord) ? CultureOf(coord) : null;
                if (culture == null) continue;

                foreach (var transform in culture.Transforms)
                {
                    if (year % transform.PeriodYears != 0) continue;

                    var candidates = _grid.Neighbors(coord)
                        .Where(cell => _tiles.TryGet(cell, out var id) && id == transform.FromTileId)
                        .ToList();
                    if (candidates.Count == 0) continue;

                    var target = candidates[_random.Next(candidates.Count)];
                    _tiles.Place(target, transform.ToTileId, "работа жителей");
                    WorldShaped?.Invoke(coord, target, transform.ToTileId);
                }
            }
        }

        private void StartHunts()
        {
            foreach (var coord in _population.Keys.ToList())
            {
                if (_hunts.ContainsKey(coord) || _population[coord] < _rules.HuntMinPopulation || _huntTaboo(coord) || IsHuntingForbidden) continue;

                var prey = _grid.Neighbors(coord)
                    .Where(IsFoodAt)
                    .ToList();
                if (prey.Count == 0) continue;

                var target = prey[_random.Next(prey.Count)];
                _features.TryGet(target, out var featureId);
                var hunt = new SettlementHunt(target, featureId, _rules.HuntYears);
                _hunts[coord] = hunt;
                HuntStarted?.Invoke(coord, hunt);
            }
        }

        private void AdvanceHunts()
        {
            foreach (var coord in _hunts.Keys.ToList())
            {
                var hunt = _hunts[coord];
                var preyAlive = _features.TryGet(hunt.Prey, out var id) && id == hunt.PreyFeatureId;
                if (!_population.ContainsKey(coord) || !preyAlive)
                {
                    _hunts.Remove(coord);
                    continue;
                }

                if (!hunt.Tick()) continue;

                _hunts.Remove(coord);
                _depletion.Deplete(hunt.Prey, _featureRules.RecoveryYears);
                HuntFinished?.Invoke(coord, hunt);
            }
        }

        private bool IsFoodAt(GridCoord cell) =>
            _features.TryGet(cell, out var featureId) && _featureRules.IsFood(featureId) && !_depletion.IsDepleted(cell);

        private void TrySpawn()
        {
            if (_epoch.Index < _rules.SpawnFromEpoch) return;

            var candidates = _grid.Coords.Where(IsHabitable).ToList();
            if (candidates.Count == 0) return;

            Found(candidates[_random.Next(candidates.Count)], _rules.StartPopulation);
        }

        private void Found(GridCoord coord, int population)
        {
            _population[coord] = population;
            Founded?.Invoke(coord);
            PopulationChanged?.Invoke(coord, 0, population);
        }

        public void Desert(GridCoord coord, int untilYear) => _desertedUntil[coord] = untilYear;

        public void Leave(GridCoord coord) => Abandon(coord);

        public IReadOnlyDictionary<GridCoord, int> Deserted => _desertedUntil;

        public bool IsDeserted(GridCoord coord) => _desertedUntil.TryGetValue(coord, out var until) && until > _year;

        private bool CanSettle(GridCoord coord) =>
            !_population.ContainsKey(coord) && !IsDeserted(coord) && _tiles.TryGet(coord, out var tileId) && _rules.CanHost(tileId);

        private bool IsHabitable(GridCoord coord) =>
            CanSettle(coord)
            && (!_fertility.CanHold(coord) || _fertility.Get(coord) >= _rules.MinFertility);

        private Func<GridCoord, int> _foodBonus = _ => 0;

        public void SetFoodBonus(Func<GridCoord, int> bonus) => _foodBonus = bonus;

        private int CountFood(GridCoord coord)
        {
            var culture = _tiles.TryGet(coord, out var tileId) ? _rules.GetCulture(tileId) : null;
            var food = _grid.Neighbors(coord).Append(coord).Where(IsFoodAt).Sum(cell => 1 + _foodBonus(cell));

            if (culture != null)
                food += _grid.Neighbors(coord).Count(cell => _tiles.TryGet(cell, out var id) && culture.FoodTileIds.Contains(id));

            food += _grid.Neighbors(coord).Append(coord).Sum(cell => _tiles.TryGet(cell, out var id) ? _rules.SharedFoodValue(id) : 0);
            food += LandFood(coord);
            return food;
        }

        private void SetPopulation(GridCoord coord, int population)
        {
            var previous = _population[coord];
            _population[coord] = population;
            PopulationChanged?.Invoke(coord, previous, population);
        }

        private void OnTilePlaced(GridCoord coord, string tileId)
        {
            if (_population.ContainsKey(coord)) CultureChanged?.Invoke(coord, _rules.GetCulture(tileId));
        }

        private void TryExodus(GridCoord coord, int population)
        {
            var frontier = _grid.Neighbors(coord).Where(cell => _grid.Contains(cell) && !_grid.IsUnlocked(cell)).ToList();
            if (_year < _nextExodus || frontier.Count == 0 || !_tiles.TryGet(coord, out var homeTile)) return;

            _nextExodus = _year + _rules.ExodusPeriod;

            var target = frontier[_random.Next(frontier.Count)];
            _grid.Unlock(target);
            _tiles.Place(target, homeTile, "исход за край мира");
            SetPopulation(coord, population - _rules.MigrationSize);
            Found(target, _rules.MigrationSize);
            Exodus?.Invoke(coord, target);
        }

        private bool TryRescue(GridCoord coord)
        {
            var helper = _grid.Neighbors(coord)
                .Where(cell => _population.TryGetValue(cell, out var population) && population >= _rules.RescueMinPopulation)
                .OrderByDescending(cell => _population[cell])
                .Cast<GridCoord?>()
                .FirstOrDefault();
            if (helper is not { } rescuer || _random.Next(100) >= _rules.RescueChancePercent) return false;

            SetPopulation(rescuer, _population[rescuer] - 1);
            SetPopulation(coord, 1);
            Rescued?.Invoke(rescuer, coord);
            return true;
        }

        private void Abandon(GridCoord coord)
        {
            _hunts.Remove(coord);
            _energy.Remove(coord);
            if (_population.Remove(coord)) Abandoned?.Invoke(coord);
        }
    }
}
