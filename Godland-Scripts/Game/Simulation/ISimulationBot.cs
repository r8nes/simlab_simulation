using System.Collections.Generic;
using Godland.Core.Randomness;
using Godland.Game.Writer.Gifts;

namespace Godland.Game.Simulation
{
    public interface ISimulationBot
    {
        string Name { get; }
        void Act(int year, SimulatedWorld world, SeededRandom random);
        GiftDefinition ChooseGift(IReadOnlyList<GiftDefinition> offered, SeededRandom random);
    }
}
