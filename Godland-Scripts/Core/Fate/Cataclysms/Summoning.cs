using Godland.Core.Life.People;

namespace Godland.Core.Fate.Cataclysms
{
    public sealed class Summoning
    {
        public Summoning(Religion religion, int yearsLeft)
        {
            Religion = religion;
            YearsLeft = yearsLeft;
        }

        public Religion Religion { get; }
        public int YearsLeft { get; private set; }

        public bool Tick() => --YearsLeft <= 0;
    }
}
