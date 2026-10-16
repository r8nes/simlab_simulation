using System.Collections.Generic;
using Godland.Core.Randomness;
using Godland.Game.Writer.Gifts;

namespace Godland.Game.Simulation
{
    public sealed class PassiveBot : ISimulationBot
    {
        public string Name => "Пассивный";

        public void Act(int year, SimulatedWorld world, SeededRandom random)
        {
        }

        public GiftDefinition ChooseGift(IReadOnlyList<GiftDefinition> offered, SeededRandom random) => offered[random.Next(offered.Count)];
    }
}
