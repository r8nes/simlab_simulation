namespace Godland.Core.Life.People
{
    public sealed class Dogma
    {
        public Dogma(string id, int faithBonus, int neighborFertility, bool huntTaboo, bool resistsDesecration, int conversionBonusPercent)
        {
            Id = id;
            FaithBonus = faithBonus;
            NeighborFertility = neighborFertility;
            HuntTaboo = huntTaboo;
            ResistsDesecration = resistsDesecration;
            ConversionBonusPercent = conversionBonusPercent;
        }

        public string Id { get; }
        public int FaithBonus { get; }
        public int NeighborFertility { get; }
        public bool HuntTaboo { get; }
        public bool ResistsDesecration { get; }
        public int ConversionBonusPercent { get; }
    }
}
