using System.Collections.Generic;

namespace Godland.Core.Life.Features
{
    public interface IFeatureRules
    {
        int GetNeighborFertility(string featureId);
        int GetSelfFertility(string featureId);
        int GetShelterYears(string featureId);
        bool IsFood(string featureId);
        int RecoveryYears { get; }
        int SpreadPeriod { get; }
        int SpreadEnergy { get; }
        int MinFertilityToThrive { get; }
        int MonsterPeriod { get; }
        int MonsterAttackChancePercent { get; }
        string MutantOf(string featureId);
        int MonsterAttack(string featureId);
        int ContagionPercent(string featureId);
        int EvolutionPeriod { get; }
        IReadOnlyList<FeatureEvolutionRule> EvolutionsOf(string featureId);
        bool CanLiveOn(string featureId, string tileId);
        bool TryGetCreationYield(string featureId, out int amount, out int periodYears);
        IReadOnlyList<FeatureInteraction> Interactions { get; }
    }
}
