using System.Collections.Generic;

namespace Godland.Core
{
    public sealed class WorldSteps
    {
        public WorldSteps(IReadOnlyList<IWorldStep> ordered) => Ordered = ordered;

        public IReadOnlyList<IWorldStep> Ordered { get; }
    }
}
