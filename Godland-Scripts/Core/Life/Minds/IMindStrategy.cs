using System.Collections.Generic;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Minds
{
    public interface IMindStrategy
    {
        string Id { get; }
        string Reason { get; }
        int Urgency(Mind mind);
        int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random);
    }
}
