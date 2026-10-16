namespace Godland.Core.Life.Minds
{
    public readonly struct MindDecision
    {
        public MindDecision(MindOption option, IMindStrategy strategy)
        {
            Option = option;
            Strategy = strategy;
        }

        public MindOption Option { get; }
        public IMindStrategy Strategy { get; }
        public string Reason => Strategy.Reason;
    }
}
