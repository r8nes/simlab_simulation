using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public interface ISovereignFall
    {
        string Describe();
        bool IsOverthrown(SovereignContext context, Religion claimant);
    }
}
