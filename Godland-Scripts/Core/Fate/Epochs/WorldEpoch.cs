using System;

namespace Godland.Core.Fate.Epochs
{
    public sealed class WorldEpoch
    {
        public event Action<int> Advanced;

        public int Index { get; private set; }

        public void Advance()
        {
            Index++;
            Advanced?.Invoke(Index);
        }
    }
}
