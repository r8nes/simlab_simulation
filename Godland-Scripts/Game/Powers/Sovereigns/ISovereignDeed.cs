using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public interface ISovereignDeed
    {
        string Name { get; }
        int Score(SovereignContext context, Religion claimant, Temperament temperament);
        string Perform(SovereignContext context, Religion claimant);
    }
}
