using System;
using System.Collections.Generic;
using System.Linq;

namespace Godland.Core.Flow
{
    public sealed class FlowBias
    {
        private readonly Dictionary<string, int> _percents = new();
        private readonly HashSet<string> _seen = new();
        private int _unseenPercent = 100;

        public event Action<string> Noted;

        public IReadOnlyCollection<string> Seen => _seen;

        public void Load(IReadOnlyDictionary<string, int> percents, int unseenPercent)
        {
            _percents.Clear();
            foreach (var (key, percent) in percents) _percents[key] = percent;
            _unseenPercent = unseenPercent;
        }

        public int PercentOf(string key) => _percents.TryGetValue(key, out var percent) ? percent : _unseenPercent;

        public float FactorOf(string key)
        {
            var factor = PercentOf(key) / 100f;
            return factor < 1f ? factor * factor : factor;
        }

        public int Chance(string key, int chancePercent) =>
            chancePercent <= 0 ? 0 : Math.Clamp((int)Math.Round(chancePercent * FactorOf(key)), 1, 100);

        public int Weight(string key, int weight) => weight <= 0 ? 0 : Math.Max(1, (int)Math.Round(weight * FactorOf(key)));

        public float Weight(string key, float weight) => weight * FactorOf(key);

        public void Note(string key)
        {
            if (_seen.Add(key)) Noted?.Invoke(key);
        }
    }

    [Serializable]
    public sealed class FlowStat
    {
        public string Key;
        public int Seen;
        public float Recent;
        public int LastRun;
    }

    public sealed class FlowRules
    {
        public FlowRules(int decayPercent, int boostPercent, int cutPercent)
        {
            DecayPercent = decayPercent;
            BoostPercent = boostPercent;
            CutPercent = cutPercent;
        }

        public int DecayPercent { get; }
        public int BoostPercent { get; }
        public int CutPercent { get; }
    }

    public static class FlowStatistics
    {
        public static void Fold(List<FlowStat> stats, int run, IEnumerable<string> runKeys, FlowRules rules)
        {
            var happened = runKeys.ToHashSet();
            var keep = rules.DecayPercent / 100f;
            foreach (var stat in stats)
            {
                var occurred = happened.Remove(stat.Key);
                stat.Recent = stat.Recent * keep + (occurred ? 1f - keep : 0f);
                if (!occurred) continue;

                stat.Seen++;
                stat.LastRun = run;
            }

            foreach (var key in happened)
                stats.Add(new FlowStat { Key = key, Seen = 1, Recent = 1f - keep, LastRun = run });
        }

        public static (Dictionary<string, int> Percents, int UnseenPercent) Bias(IReadOnlyList<FlowStat> stats, int runs, FlowRules rules)
        {
            var percents = new Dictionary<string, int>();
            if (runs == 0) return (percents, 100);

            var keep = rules.DecayPercent / 100f;
            foreach (var stat in stats)
            {
                var usual = stat.Seen / (float)runs;
                var interval = 1f / Math.Max(usual, 0.0001f);
                var gap = runs - stat.LastRun;
                var drought = Math.Clamp(gap / interval - 0.5f, 0f, 1f);
                var excess = Math.Clamp(stat.Recent - usual, 0f, 1f) / Math.Max(1f - usual, 0.05f);
                var echo = (float)Math.Pow(keep, gap * 2);
                var cut = Math.Clamp(Math.Max(excess, 0.5f * Math.Clamp(stat.Recent, 0f, 1f) + 0.5f * echo), 0f, 1f);
                percents[stat.Key] = Math.Max(5, (int)Math.Round(100 + rules.BoostPercent * drought - rules.CutPercent * cut));
            }

            return (percents, 100 + rules.BoostPercent);
        }

        public static float Similarity(IReadOnlyCollection<string> first, IReadOnlyCollection<string> second)
        {
            var union = first.Union(second).Count();
            return union == 0 ? 1f : first.Intersect(second).Count() / (float)union;
        }
    }
}
