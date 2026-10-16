namespace Godland.Core.Life.Features
{
    public sealed class FeatureInteraction
    {
        public FeatureInteraction(string predatorId, string preyId, int years, float counterChance, string mutantId)
        {
            PredatorId = predatorId;
            PreyId = preyId;
            Years = years;
            CounterChance = counterChance;
            MutantId = mutantId;
        }

        public string PredatorId { get; }
        public string PreyId { get; }
        public int Years { get; }
        public float CounterChance { get; }
        public string MutantId { get; }
    }
}
