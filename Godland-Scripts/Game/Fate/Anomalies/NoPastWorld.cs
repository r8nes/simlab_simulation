using System;
using System.Collections.Generic;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class NoPastWorld : IPastWorld
    {
        public IReadOnlyList<string> PastTiles => Array.Empty<string>();
        public IReadOnlyList<string> PastNames => Array.Empty<string>();
    }
}
