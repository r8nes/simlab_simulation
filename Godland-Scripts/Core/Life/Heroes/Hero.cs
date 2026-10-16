using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Core.Life.Heroes
{
    public sealed class Hero
    {
        private readonly List<Hero> _companions = new();

        public Hero(string name, GridCoord home, HeroTrait trait, IHeroArchetype archetype = null)
        {
            Archetype = archetype;
            Name = name;
            Home = home;
            Position = home;
            Trait = trait;
        }

        public string Name { get; }
        public IHeroArchetype Archetype { get; }
        public GridCoord Home { get; }
        public HeroTrait Trait { get; }
        public GridCoord Position { get; internal set; }
        public GridCoord? Target { get; internal set; }
        public QuestKind Quest { get; internal set; }
        public bool Blessed { get; internal set; }
        public int Age { get; internal set; }
        public int Glory { get; internal set; }
        public bool IsDemigod { get; internal set; }
        public int Legends { get; internal set; }
        public IReadOnlyList<Hero> Companions => _companions;
        public IEnumerable<Hero> Party => _companions.Prepend(this);

        internal void Join(Hero other)
        {
            _companions.Add(other);
            _companions.AddRange(other._companions);
            other._companions.Clear();
            Blessed |= other.Blessed;
        }
    }
}
