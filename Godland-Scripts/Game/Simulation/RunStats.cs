using System.Collections.Generic;

namespace Godland.Game.Simulation
{
    public sealed class RunStats
    {
        public int Seed;
        public readonly Dictionary<string, int> Counters = new();
        public readonly Dictionary<int, Dictionary<string, int>> Snapshots = new();
        public readonly Dictionary<string, int> FirstYear = new();
        public readonly List<string> Names = new();
        public readonly List<string> FlowKeys = new();

        public event System.Action<int, string> Counted;

        public void Count(string key, int year)
        {
            Counted?.Invoke(year, key);
            Counters[key] = Counters.GetValueOrDefault(key) + 1;
            if (!FirstYear.ContainsKey(key)) FirstYear[key] = year;
        }
    }
}
