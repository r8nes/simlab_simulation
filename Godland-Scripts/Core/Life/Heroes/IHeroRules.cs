using System.Collections.Generic;

namespace Godland.Core.Life.Heroes
{
    public interface IHeroRules
    {
        int HeroesFromEpoch { get; }
        int BirthMinYears { get; }
        int BirthMaxYears { get; }
        int HeroMinPopulation { get; }
        int MoveYears { get; }
        int SuccessChancePercent { get; }
        int BlessedBonusPercent { get; }
        int BraveBonusPercent { get; }
        int AllyBonusPercent { get; }
        int LegendPopulation { get; }
        int TragedyLoss { get; }
        int MaxAgeYears { get; }
        int FiascoChancePercent { get; }
        int RecklessFiascoPercent { get; }
        int WiseFiascoPercent { get; }
        int FiascoSpread { get; }
        int TreatyChancePercent { get; }
        int WiseTreatyPercent { get; }
        int DuelChancePercent { get; }
        int GloryPerVictory { get; }
        int MeetDistance { get; }
        int DemigodChancePercent { get; }
        int DemigodBonusPercent { get; }
        int PantheonGuardPercent { get; }
        IReadOnlyList<IHeroArchetype> Archetypes { get; }
    }
}
