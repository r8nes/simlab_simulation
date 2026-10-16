namespace Godland.Core.Powers.Gods
{
    public interface ITempleRules
    {
        int TemplePeriod { get; }
        int NeglectLimit { get; }
        int TempleDesecrationLoss { get; }
        int MaxTemplesPerGod { get; }
    }
}
