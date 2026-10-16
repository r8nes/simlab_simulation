using Godland.Core.World.Grid;

namespace Godland.Core.Life.People
{
    public sealed class SettlementHunt
    {
        public SettlementHunt(GridCoord prey, string preyFeatureId, int yearsLeft)
        {
            Prey = prey;
            PreyFeatureId = preyFeatureId;
            YearsLeft = yearsLeft;
        }

        public GridCoord Prey { get; }
        public string PreyFeatureId { get; }
        public int YearsLeft { get; private set; }

        public bool Tick() => --YearsLeft <= 0;
    }
}
