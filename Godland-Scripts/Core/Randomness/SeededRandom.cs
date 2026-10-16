using System;

namespace Godland.Core.Randomness
{
    public sealed class SeededRandom
    {
        private uint _state;

        public SeededRandom(int seed)
        {
            Seed = seed;
            _state = seed == 0 ? 0x9E3779B9u : (uint)seed;
        }

        public int Seed { get; }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return (int)(NextUInt() % (uint)maxExclusive);
        }

        public float NextFloat() => (NextUInt() >> 8) / 16777216f;

        private uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }
    }
}
