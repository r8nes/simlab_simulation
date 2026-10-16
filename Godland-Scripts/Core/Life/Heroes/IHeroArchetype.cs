using System.Collections.Generic;

namespace Godland.Core.Life.Heroes
{
    public interface IHeroArchetype
    {
        string Title { get; }
        int Epoch { get; }
        bool NeedsCity { get; }
        int MinPopulation { get; }
        QuestKind FavoredQuest { get; }
        int FavoredBonusPercent { get; }
        bool HuntsGods { get; }
        IReadOnlyList<string> Names { get; }
    }
}
