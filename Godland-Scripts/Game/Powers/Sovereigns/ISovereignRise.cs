using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public interface ISovereignRise
    {
        bool TryFindClaimant(SovereignContext context, out Religion claimant);
    }
}
