namespace Godland.Core.Life.Minds
{
    public interface IMindRules
    {
        int LearningPercent { get; }
        int Noise { get; }
        int DespairReward { get; }
        int InheritPercent { get; }
        int NoveltyPercent { get; }
    }
}
