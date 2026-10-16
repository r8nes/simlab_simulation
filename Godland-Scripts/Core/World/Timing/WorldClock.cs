using System;

namespace Godland.Core.World.Timing
{
    public sealed class WorldClock
    {
        public event Action<int> Ticked;

        public int Year { get; private set; }

        public void Advance()
        {
            Year++;
            Ticked?.Invoke(Year);
        }
    }
}
