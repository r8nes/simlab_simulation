using Godland.Core.World.Grid;

namespace Godland.Core.Life.Features
{
    public sealed class FeatureThreat
    {
        public FeatureThreat(FeatureInteraction interaction, GridCoord predator, int yearsLeft)
        {
            Interaction = interaction;
            Predator = predator;
            YearsLeft = yearsLeft;
        }

        public FeatureInteraction Interaction { get; }
        public GridCoord Predator { get; }
        public int YearsLeft { get; private set; }

        public bool Tick() => --YearsLeft <= 0;
    }
}
