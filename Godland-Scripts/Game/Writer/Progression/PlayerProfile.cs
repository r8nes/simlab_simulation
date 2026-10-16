using System;
using System.Collections.Generic;

namespace Godland.Game.Writer.Progression
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public int Version;
        public int Memory;
        public List<string> Discoveries = new();
        public List<string> Unlocks = new();
        public List<string> Claimed = new();
        public int FlowRuns;
        public List<Godland.Core.Flow.FlowStat> Flow = new();
        public List<string> PendingRun = new();

        public bool HasDiscovered(string id) => Discoveries.Contains(id);

        public bool HasUnlocked(string id) => Unlocks.Contains(id);

        public bool HasClaimed(string id) => Claimed.Contains(id);

        public bool CanClaim(string id) => HasDiscovered(id) && !HasClaimed(id);

        public void Discover(string id)
        {
            if (!HasDiscovered(id)) Discoveries.Add(id);
        }

        public bool Claim(string id, int reward)
        {
            if (!CanClaim(id)) return false;

            Claimed.Add(id);
            Memory += reward;
            return true;
        }

        public void Migrate()
        {
            if (Version >= 1) return;

            Claimed = new List<string>(Discoveries);
            Version = 1;
        }

        public void Unlock(string id)
        {
            if (!HasUnlocked(id)) Unlocks.Add(id);
        }
    }
}
