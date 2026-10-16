using System.Collections.Generic;

namespace Godland.Core.Life.Kingdoms
{
    public interface IKingdomRules
    {
        int FormPeriod { get; }
        int MinMembers { get; }
        int SurviveMembers { get; }
        int DissolveStrikes { get; }
        int StabilityYears { get; }
        int WarCheckPeriod { get; }
        int WarChancePercent { get; }
        int WarMinYears { get; }
        int WarMaxYears { get; }
        int BattlePeriod { get; }
        int BattlePopulationLoss { get; }
        IReadOnlyList<string> Names { get; }
    }
}
