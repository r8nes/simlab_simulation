using System.Collections.Generic;

namespace Godland.Core.Life.People
{
    public interface IShrineRules
    {
        int ShrinePeriod { get; }
        int ShrineChancePercent { get; }
        int ShrineFaith { get; }
        int DesecrationPopulationLoss { get; }
        int ConversionPeriod { get; }
        int ConversionChancePercent { get; }
        int SchismPeriod { get; }
        int SchismMinFollowers { get; }
        int SchismChancePercent { get; }
        Dogma DogmaFor(string godTileId);
        Dogma DefaultDogma { get; }
        IReadOnlyList<Dogma> HeresyDogmas { get; }
        int IdolRevealFervor { get; }
        int MiracleFervorLoss { get; }
        IReadOnlyList<IdolRule> Idols { get; }
    }
}
