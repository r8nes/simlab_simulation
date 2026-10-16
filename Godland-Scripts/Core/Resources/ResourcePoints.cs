using System;

namespace Godland.Core.Resources
{
    public abstract class ResourcePoints
    {
        protected ResourcePoints(int startingAmount)
        {
            if (startingAmount < 0) throw new ArgumentOutOfRangeException(nameof(startingAmount));
            Amount = startingAmount;
        }

        public event Action<int> Changed;

        public int Amount { get; private set; }

        public bool CanAfford(int cost) => Amount >= cost;

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;

            Amount += amount;
            Changed?.Invoke(Amount);
        }

        public bool TrySpend(int cost)
        {
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
            if (!CanAfford(cost)) return false;

            Amount -= cost;
            Changed?.Invoke(Amount);
            return true;
        }
    }
}
