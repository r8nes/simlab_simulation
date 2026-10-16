using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public interface ISovereignEffect
    {
        void OnRise(SovereignContext context, Religion claimant);
        void OnReign(SovereignContext context, int year);
        void OnFall(SovereignContext context);
    }
}
