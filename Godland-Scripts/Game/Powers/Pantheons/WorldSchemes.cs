using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Powers.Gods;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    public sealed class SchemeActor
    {
        public SchemeActor(object key, string kind, string name, int age, Func<PantheonDeedSite, GridCoord?> site)
        {
            Kind = kind;
            Key = key;
            Name = name;
            Age = age;
            Site = site;
        }

        public object Key { get; }
        public string Kind { get; }
        public string Name { get; }
        public int Age { get; }
        public Func<PantheonDeedSite, GridCoord?> Site { get; }
    }

    public sealed class WorldSchemes : IWorldStep
    {
        private readonly PantheonConfig _config;
        private readonly CatastropheContext _context;
        private readonly GodsConfig _gods;
        private readonly TitanConfig _titans;
        private readonly SovereignKeeper _sovereigns;
        private readonly Godland.Game.Fate.Sagas.SagaKeeper _sagas;
        private readonly Dictionary<object, HashSet<string>> _facts = new();
        private readonly Dictionary<object, HashSet<string>> _fulfilled = new();
        private readonly Dictionary<object, (GodScheme Scheme, List<GodAction> Plan)> _plans = new();
        private int _sovereignSince;

        public WorldSchemes(PantheonConfig config, CatastropheContext context, GodsConfig gods, TitanConfig titans, SovereignKeeper sovereigns,
            Godland.Game.Fate.Sagas.SagaKeeper sagas)
        {
            _config = config;
            _context = context;
            _gods = gods;
            _titans = titans;
            _sovereigns = sovereigns;
            _sagas = sagas;
            _sovereigns.Rose += _ => _sovereignSince = _context.World.Clock.Year;
        }

        public event Action<string, GodAction, GridCoord> Schemed;
        public event Action<string, GodScheme> Fulfilled;

        public IReadOnlyCollection<string> FactsOf(object actor) => _facts.TryGetValue(actor, out var facts) ? facts : (IReadOnlyCollection<string>)Array.Empty<string>();

        public IReadOnlyList<GodAction> PlanOf(object actor) => _plans.TryGetValue(actor, out var plan) ? plan.Plan : (IReadOnlyList<GodAction>)Array.Empty<GodAction>();

        public void Advance(int year)
        {
            if (_config.WorldBook == null || year % _config.SchemePeriod != 0) return;

            foreach (var actor in Actors(year).ToList()) Step(actor);
        }

        private IEnumerable<SchemeActor> Actors(int year)
        {
            var world = _context.World;
            foreach (var god in world.Pantheon.Gods.Where(god => god.PantheonId == null && god.HeroName == null).ToList())
            {
                var faith = god.Religion;
                yield return new SchemeActor(god, "стихия", _gods.DefinitionOf(god.Rule.Id).DisplayName, year - god.BornYear, site => site == PantheonDeedSite.Follower
                    ? world.Shrines.FollowersOf(faith).Cast<GridCoord?>().FirstOrDefault() ?? god.Seat
                    : site == PantheonDeedSite.Seat ? god.Seat : null);
            }

            foreach (var titan in world.Titans.All.Where(titan => titan.IsAwake && !titan.IsTamed).ToList())
                yield return new SchemeActor(titan, "титан", _titans.DefinitionOf(titan.Rule.Id).DisplayName, year - titan.BornYear, site => site == PantheonDeedSite.Rival ? null : titan.Position);

            if (_sovereigns.Ruler is { } ruler && _sovereigns.Claimant is { } claimant)
                yield return new SchemeActor(ruler, "правитель", ruler.DisplayName, year - _sovereignSince, site =>
                {
                    var followers = world.Shrines.FollowersOf(claimant);
                    return site == PantheonDeedSite.Rival || followers.Count == 0 ? null : followers[world.Random.Next(followers.Count)];
                });
        }

        private void Step(SchemeActor actor)
        {
            var facts = Facts(_facts, actor.Key);
            var fulfilled = Facts(_fulfilled, actor.Key);
            var book = _config.WorldBook;
            var actions = book.Actions.Where(action => Fits(actor, action.God) && Allows(actor, action)).ToList();
            if (!_plans.TryGetValue(actor.Key, out var current) || current.Plan.Count == 0 || !actions.Contains(current.Plan[0]) || !current.Plan[0].Needs.All(facts.Contains))
            {
                current = SchemePlanner.Choose(book.Schemes.Where(scheme => Fits(actor, scheme.God)), facts, fulfilled, actions, _context.World.Random, _config.PlanDepth);
                if (current.Scheme == null) return;
                _plans[actor.Key] = current;
            }

            var action = current.Plan[0];
            current.Plan.RemoveAt(0);
            if (actor.Site(action.Site) is not { } site) return;

            Perform(actor, action, site, facts);
            if (!facts.Contains(current.Scheme.Goal)) return;

            fulfilled.Add(current.Scheme.Goal);
            _plans.Remove(actor.Key);
            if (current.Scheme.Repeatable) facts.Remove(current.Scheme.Goal);
            Fulfilled?.Invoke(actor.Name, current.Scheme);
        }

        private static bool Fits(SchemeActor actor, string filter) =>
            !string.IsNullOrEmpty(filter) && (filter == actor.Kind || SchemePlanner.Fits(actor.Name, filter));

        private bool Allows(SchemeActor actor, GodAction action) => actor.Age >= action.MinGodAge && (action.When == null || action.When.IsReady(_context));

        private void Perform(SchemeActor actor, GodAction action, GridCoord site, HashSet<string> facts)
        {
            var world = _context.World;
            var title = $"{actor.Name}: {action.Name}";
            foreach (var outcome in action.Outcomes) outcome.Apply(world, site, title);
            foreach (var fact in action.Removes) facts.Remove(fact);
            foreach (var fact in action.Adds) facts.Add(fact);
            if (!string.IsNullOrEmpty(action.Demigod) && world.Settlements.TryGet(site, out _)) world.Heroes.SummonDemigod(site, $"Полубог {action.Demigod}");
            if (action.Saga != null) _sagas.TryCall(action.Saga);
            world.Memory.Add(site, $"scheme:{action.Name}");
            world.History.Note(site, title);
            Schemed?.Invoke(actor.Name, action, site);
        }

        private static HashSet<string> Facts(Dictionary<object, HashSet<string>> store, object key)
        {
            if (!store.TryGetValue(key, out var facts)) store[key] = facts = new HashSet<string>();
            return facts;
        }
    }

    [Serializable]
    public sealed class LullTitanOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 150;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var titan = context.Titans.All.Where(candidate => candidate.IsAwake && !candidate.IsTamed)
                .OrderBy(candidate => Math.Abs(candidate.Position.X - site.X) + Math.Abs(candidate.Position.Y - site.Y))
                .FirstOrDefault();
            if (titan != null) context.Titans.Lull(titan, _years);
        }
    }
}
