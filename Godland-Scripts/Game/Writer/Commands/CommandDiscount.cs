using System.Collections.Generic;
using System.Linq;

namespace Godland.Game.Writer.Commands
{
    public sealed class CommandDiscount
    {
        private readonly Dictionary<string, int> _sources = new();

        public int Percent => System.Math.Min(80, _sources.Values.Sum());

        public void Set(string source, int percent) => _sources[source] = percent;
    }
}
