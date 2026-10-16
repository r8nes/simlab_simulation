using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Features
{
    public sealed class FeatureInteractions
    {
        private readonly WorldGrid _grid;
        private readonly TileFeatures _features;
        private readonly IFeatureRules _rules;
        private readonly SeededRandom _random;
        private readonly InteractionModifiers _modifiers;
        private readonly FeatureDepletion _depletion;
        private readonly Dictionary<GridCoord, FeatureThreat> _threats = new();

        public FeatureInteractions(WorldGrid grid, TileFeatures features, IFeatureRules rules, SeededRandom random, InteractionModifiers modifiers, FeatureDepletion depletion)
        {
            _grid = grid;
            _features = features;
            _rules = rules;
            _random = random;
            _modifiers = modifiers;
            _depletion = depletion;
        }

        public event Action<GridCoord, FeatureThreat> ThreatStarted;
        public event Action<GridCoord, FeatureThreat> ThreatAdvanced;
        public event Action<GridCoord> ThreatLifted;
        public event Action<GridCoord, FeatureInteraction, bool> Resolved;

        public bool TryGetThreat(GridCoord prey, out FeatureThreat threat) => _threats.TryGetValue(prey, out threat);

        public void Resolve()
        {
            LiftStaleThreats();
            var started = StartNewThreats();

            foreach (var prey in _grid.Coords.Where(coord => _threats.ContainsKey(coord) && !started.Contains(coord)).ToList())
            {
                var threat = _threats[prey];
                if (threat.Tick()) Finish(prey, threat);
                else ThreatAdvanced?.Invoke(prey, threat);
            }
        }

        private void LiftStaleThreats()
        {
            foreach (var prey in _threats.Keys.ToList())
            {
                var threat = _threats[prey];
                var preyAlive = _features.TryGet(prey, out var preyId) && preyId == threat.Interaction.PreyId;
                var predatorAlive = _features.TryGet(threat.Predator, out var predatorId) && predatorId == threat.Interaction.PredatorId;
                if (preyAlive && predatorAlive && !_depletion.IsDepleted(prey) && !_depletion.IsDepleted(threat.Predator)) continue;

                _threats.Remove(prey);
                ThreatLifted?.Invoke(prey);
            }
        }

        private HashSet<GridCoord> StartNewThreats()
        {
            var started = new HashSet<GridCoord>();
            foreach (var prey in _grid.Coords)
            {
                if (_threats.ContainsKey(prey) || _depletion.IsDepleted(prey) || !_features.TryGet(prey, out var preyId)) continue;

                foreach (var interaction in _rules.Interactions.Where(rule => rule.PreyId == preyId))
                {
                    var predator = _grid.Neighbors(prey)
                        .Where(neighbor => _features.TryGet(neighbor, out var id) && id == interaction.PredatorId && !_depletion.IsDepleted(neighbor))
                        .Cast<GridCoord?>()
                        .FirstOrDefault();
                    if (predator is not { } predatorCoord) continue;

                    var threat = new FeatureThreat(interaction, predatorCoord, interaction.Years + ShelterYears(prey));
                    _threats[prey] = threat;
                    started.Add(prey);
                    ThreatStarted?.Invoke(prey, threat);
                    break;
                }
            }

            return started;
        }

        private int ShelterYears(GridCoord prey) =>
            _grid.Neighbors(prey).Sum(neighbor => _features.TryGet(neighbor, out var id) ? _rules.GetShelterYears(id) : 0);

        private void Finish(GridCoord prey, FeatureThreat threat)
        {
            _threats.Remove(prey);
            var interaction = threat.Interaction;
            var countered = _random.NextFloat() < interaction.CounterChance * _modifiers.CounterChanceMultiplier;

            if (countered)
            {
                _depletion.Deplete(threat.Predator, _rules.RecoveryYears);
                if (!string.IsNullOrEmpty(interaction.MutantId)) _features.Replace(prey, interaction.MutantId);
            }
            else
            {
                _depletion.Deplete(prey, _rules.RecoveryYears);
            }

            Resolved?.Invoke(prey, interaction, countered);
        }
    }
}
