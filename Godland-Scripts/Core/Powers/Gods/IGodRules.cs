using System.Collections.Generic;

namespace Godland.Core.Powers.Gods
{
    public interface IGodRules : ITempleRules
    {
        int GodsFromEpoch { get; }
        int BirthPeriod { get; }
        int CallPeriod { get; }
        int CallRadius { get; }
        int CallChancePercent { get; }
        int FadePeriod { get; }
        int FadeFervor { get; }
        int FadeChancePercent { get; }
        int ResistFervor { get; }
        int RebirthCooldown { get; }
        int MaxGods { get; }
        int PressurePerSettlement { get; }
        int DisplacePercent { get; }
        IReadOnlyList<GodRule> Gods { get; }
    }
}
