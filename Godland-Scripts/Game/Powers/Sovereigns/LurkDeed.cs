using System;
using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class LurkDeed : ISovereignDeed
    {
        public string Name => "затаился и копит силы";

        public int Score(SovereignContext context, Religion claimant, Temperament temperament)
        {
            var followers = SovereignPower.Followers(context, claimant);
            var rivals = SovereignPower.Rivals(context, claimant);
            return rivals > followers ? temperament.Fear + 30 : temperament.Fear / 2;
        }

        public string Perform(SovereignContext context, Religion claimant)
        {
            claimant.Kindle(10);
            return Name;
        }
    }
}
