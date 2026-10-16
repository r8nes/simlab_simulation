using System;

namespace Godland.Core.Fate.Anomalies
{
    public sealed class WorldSleep
    {
        public event Action Woke;

        public int Until { get; private set; }

        public bool IsAsleep(int year) => year < Until;

        public void Sleep(int untilYear) => Until = untilYear;

        public void Wake() => Woke?.Invoke();
    }
}
