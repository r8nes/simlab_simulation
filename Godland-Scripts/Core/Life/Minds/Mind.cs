using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Minds
{
    public sealed class Mind
    {
        private const int StartingTrust = 50;
        private const int MaxTrust = 500;
        private const int MaxDespair = 200;
        private const int DespairStep = 25;

        private static readonly IMindStrategy[] Strategies =
            { new NatureStrategy(), new HabitStrategy(), new ImitationStrategy(), new PlanStrategy(), new NoveltyStrategy(), new GambleStrategy() };

        private readonly IMindRules _rules;
        private readonly Dictionary<string, int> _habits = new();
        private readonly Dictionary<string, int> _tries = new();
        private readonly Dictionary<(string From, string To), int> _chains = new();
        private readonly Dictionary<string, int> _trust = Strategies.ToDictionary(strategy => strategy.Id, _ => StartingTrust);
        private string _previousAction;
        private IMindStrategy _lastStrategy;
        private int? _lastMeasure;
        private int _lastNovelty;

        public Mind(IMindRules rules) => _rules = rules;

        public string LastAction { get; private set; }
        public int Success { get; private set; }
        public int Despair { get; private set; }

        public int HabitOf(string action) => _habits.TryGetValue(action, out var value) ? value : 0;

        public int TimesTried(string action) => _tries.TryGetValue(action, out var value) ? value : 0;

        public int ChainOf(string from, string to) => from != null && _chains.TryGetValue((from, to), out var value) ? value : 0;

        public MindDecision? Decide(IReadOnlyList<MindOption> options, IReadOnlyList<Mind> models, SeededRandom random)
        {
            var allowed = options.Where(option => option.IsAllowed).ToList();
            if (allowed.Count == 0) return null;

            var strategy = ChooseStrategy(random);
            var best = allowed
                .Select(option => (Option: option, Score: strategy.Value(this, option, models, random) + random.Next(_rules.Noise + 1)))
                .OrderByDescending(pair => pair.Score)
                .First().Option;

            var others = models.Where(model => model != this && model.LastAction != null).ToList();
            _lastNovelty = others.Count == 0 ? 0 : 100 - others.Count(model => model.LastAction == best.Id) * 100 / others.Count;
            _previousAction = LastAction;
            LastAction = best.Id;
            _lastStrategy = strategy;
            _tries[best.Id] = TimesTried(best.Id) + 1;
            return new MindDecision(best, strategy);
        }

        public void Measure(int measure)
        {
            if (_lastMeasure is { } before && LastAction != null) Learn(measure - before);
            _lastMeasure = measure;
        }

        public void Forget()
        {
            _habits.Clear();
            _tries.Clear();
            _chains.Clear();
            foreach (var strategy in Strategies) _trust[strategy.Id] = StartingTrust;
            Despair = 0;
        }

        public void Inherit(Mind teacher)
        {
            var share = _rules.InheritPercent;
            foreach (var (action, value) in teacher._habits.ToList()) _habits[action] = Blend(HabitOf(action), value, share);
            foreach (var (chain, value) in teacher._chains.ToList()) _chains[chain] = Blend(ChainOf(chain.From, chain.To), value, share);
            foreach (var (strategy, value) in teacher._trust.ToList()) _trust[strategy] = Blend(_trust[strategy], value, share);
        }

        public string Describe(Func<string, string> nameOf)
        {
            var habit = _habits.Where(pair => pair.Value > 0).OrderByDescending(pair => pair.Value).Select(pair => pair.Key).FirstOrDefault();
            var trusted = Strategies.OrderByDescending(strategy => _trust[strategy.Id]).First();
            return habit == null ? $"действует {trusted.Reason}" : $"действует {trusted.Reason}, привычка: {nameOf(habit)}";
        }

        private IMindStrategy ChooseStrategy(SeededRandom random)
        {
            var weights = Strategies.Select(strategy => Math.Max(1, _trust[strategy.Id] + strategy.Urgency(this))).ToList();
            var roll = random.Next(weights.Sum());
            for (var i = 0; i < Strategies.Length; i++)
            {
                if (roll < weights[i]) return Strategies[i];
                roll -= weights[i];
            }

            return Strategies[0];
        }

        private void Learn(int reward)
        {
            reward += _lastNovelty * _rules.NoveltyPercent / 2500;
            var rate = _rules.LearningPercent;
            var target = reward * 10;
            _habits[LastAction] = Toward(HabitOf(LastAction), target, rate);
            if (_previousAction != null) _chains[(_previousAction, LastAction)] = Toward(ChainOf(_previousAction, LastAction), target, rate);
            if (_lastStrategy != null) _trust[_lastStrategy.Id] = Math.Clamp(_trust[_lastStrategy.Id] + reward * rate / 10, 1, MaxTrust);

            Success += reward;
            Despair = reward <= _rules.DespairReward ? Math.Min(MaxDespair, Despair + DespairStep) : Math.Max(0, Despair - DespairStep);
        }

        private static int Toward(int current, int target, int percent) => current + (target - current) * percent / 100;

        private static int Blend(int own, int other, int share) => own + (other - own) * share / 100;
    }
}
