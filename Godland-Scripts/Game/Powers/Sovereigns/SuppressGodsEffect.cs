using System;
using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class SuppressGodsEffect : ISovereignEffect
    {
        public void OnRise(SovereignContext context, Religion claimant) => context.Pantheon.Suppress(true);

        public void OnReign(SovereignContext context, int year) { }

        public void OnFall(SovereignContext context) => context.Pantheon.Suppress(false);
    }
}
