namespace Godland.Core.Fate.Cataclysms
{
    public interface ICataclysmRules
    {
        int MinWorldBelievers { get; }
        int DominancePercent { get; }
        int SummonFervor { get; }
        int WarningYears { get; }
        int PopulationLoss { get; }
        bool IsImmune(string tileId);
        Cataclysm CataclysmOf(string idolId);
    }
}
