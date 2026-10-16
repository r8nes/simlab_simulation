using Godland.Core;
using Godland.Game.Fate.Catastrophes;

namespace Godland.Game.Fate.Epochs
{
    public sealed class EpochLawKeeper : IWorldStep
    {
        private readonly EpochProgression _progression;
        private readonly CatastropheContext _context;
        private EpochDefinition _active;

        public EpochLawKeeper(EpochProgression progression, CatastropheContext context)
        {
            _progression = progression;
            _context = context;
        }

        public void Advance(int year)
        {
            var current = _progression.Current;
            if (current != _active)
            {
                if (_active != null)
                    foreach (var law in _active.Laws) law.End(_context);
                _active = current;
                foreach (var law in current.Laws) law.Begin(_context, current.DisplayName.ToLowerInvariant());
                return;
            }

            var age = year - _progression.StartYear;
            if (age > 0)
                foreach (var law in current.Laws) law.Year(_context, age, current.DisplayName.ToLowerInvariant());
        }
    }
}
