using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Randomness;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    public static class SchemePlanner
    {
        public static bool Fits(string actor, string filter) =>
            string.IsNullOrEmpty(filter) || actor == filter || (actor != null && actor.StartsWith(filter + ","));

        public static (GodScheme Scheme, List<GodAction> Plan) Choose(
            IEnumerable<GodScheme> schemes, IReadOnlyCollection<string> facts, IReadOnlyCollection<string> fulfilled,
            IReadOnlyList<GodAction> actions, SeededRandom random, int depth)
        {
            var candidates = schemes.Where(scheme => (scheme.Repeatable || !fulfilled.Contains(scheme.Goal)) && !facts.Contains(scheme.Goal)).ToList();
            while (candidates.Count > 0)
            {
                var roll = random.Next(candidates.Sum(scheme => scheme.Weight));
                var scheme = candidates.First(candidate => (roll -= candidate.Weight) < 0);
                candidates.Remove(scheme);
                var plan = Search(facts, scheme.Goal, actions, depth);
                if (plan != null) return (scheme, plan);
            }

            return (null, null);
        }

        public static List<GodAction> Search(IReadOnlyCollection<string> start, string goal, IReadOnlyList<GodAction> actions, int depth)
        {
            var frontier = new List<(HashSet<string> Facts, List<GodAction> Path, int Cost)> { (start.ToHashSet(), new List<GodAction>(), 0) };
            var seen = new HashSet<string> { Key(start) };
            for (var step = 0; step < depth && frontier.Count > 0; step++)
            {
                var next = new List<(HashSet<string> Facts, List<GodAction> Path, int Cost)>();
                foreach (var (facts, path, cost) in frontier)
                foreach (var action in actions.Where(action => action.Needs.All(facts.Contains)))
                {
                    var after = Apply(facts, action);
                    if (!seen.Add(Key(after))) continue;

                    var steps = path.Append(action).ToList();
                    if (after.Contains(goal)) return steps;
                    next.Add((after, steps, cost + action.Cost));
                }

                frontier = next.OrderBy(entry => entry.Cost).ToList();
            }

            return null;
        }

        public static HashSet<string> Apply(IEnumerable<string> facts, GodAction action)
        {
            var after = facts.ToHashSet();
            foreach (var fact in action.Removes) after.Remove(fact);
            foreach (var fact in action.Adds) after.Add(fact);
            return after;
        }

        private static string Key(IEnumerable<string> facts) => string.Join("|", facts.OrderBy(fact => fact, StringComparer.Ordinal));
    }
}
