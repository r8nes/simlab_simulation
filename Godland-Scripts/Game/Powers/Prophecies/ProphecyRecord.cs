using System.Collections.Generic;

namespace Godland.Game.Powers.Prophecies
{
    public sealed class ProphecyRecord
    {
        private readonly Dictionary<string, int> _counters = new();
        private readonly Dictionary<string, int> _baselines = new();

        public int GodsSlainByHeroes => CountOf("god-slain");
        public int AnnouncedYear { get; private set; }

        public void CountGodSlain() => Count("god-slain");

        public void Count(string key, int amount = 1) => _counters[key] = CountOf(key) + amount;

        public int CountOf(string key) => _counters.GetValueOrDefault(key);

        public void Set(string key, int value) => _counters[key] = value;

        public int Rise(string key, int current)
        {
            if (!_baselines.TryGetValue(key, out var baseline)) _baselines[key] = baseline = current;
            return current - baseline;
        }

        public void Reset(int year = 0)
        {
            AnnouncedYear = year;
            _counters.Clear();
            _baselines.Clear();
        }
    }
}
