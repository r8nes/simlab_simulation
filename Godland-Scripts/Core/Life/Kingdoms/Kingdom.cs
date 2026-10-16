using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;

namespace Godland.Core.Life.Kingdoms
{
    public sealed class Kingdom
    {
        private readonly HashSet<GridCoord> _members = new();

        public Kingdom(int index, string name, GridCoord capital, Religion religion)
        {
            Index = index;
            Name = name;
            Capital = capital;
            Religion = religion;
        }

        public int Index { get; }
        public string Name { get; }
        public GridCoord Capital { get; internal set; }
        public int Founded { get; internal set; }
        public Religion Religion { get; }
        public IReadOnlyCollection<GridCoord> Members => _members;
        public int Strikes { get; internal set; }

        internal void SetMembers(IEnumerable<GridCoord> members)
        {
            _members.Clear();
            _members.UnionWith(members);
        }
    }
}
