using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Fate.Emergence;
using Godland.Game.Powers.Pantheons;
using Godland.Game.Writer.Threads;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentRewrite : IDisposable
    {
        private readonly CommandmentKeeper _keeper;
        private readonly CommandmentDraft _draft;
        private readonly WriterConfig _config;
        private readonly EmergenceContext _world;
        private readonly PantheonRegistry _pantheons;
        private bool _pending;

        public CommandmentRewrite(CommandmentKeeper keeper, CommandmentDraft draft, WriterConfig config, EmergenceContext world, PantheonRegistry pantheons)
        {
            _keeper = keeper;
            _draft = draft;
            _config = config;
            _world = world;
            _pantheons = pantheons;
            _keeper.Proclaimed += OnProclaimed;
        }

        public event Action Requested;
        public event Action<Commandment, int> Rewritten;

        public bool AutoProclaim { get; set; }
        public int Times { get; private set; }

        public void Dispose() => _keeper.Proclaimed -= OnProclaimed;

        public GridCoord? Request()
        {
            if (_keeper.Current == null || _pending) return null;

            _pending = true;
            if (AutoProclaim)
            {
                _draft.Offer();
                _keeper.Proclaim(_draft.PickRandom());
            }
            else Requested?.Invoke();

            return _world.Grid.Center;
        }

        private void OnProclaimed(Commandment commandment)
        {
            if (!_pending) return;

            _pending = false;
            Times++;
            foreach (var faith in _world.Shrines.Religions) faith.Cool(_config.RewriteCool);
            foreach (var house in _pantheons.Active.Where(house => house.Head != null))
                _world.Memory.Add(house.Head.Seat, "god-spurned");
            _world.Memory.Add(_world.Grid.Center, "old-covenant");

            var keepers = KeepOldCovenant();
            Rewritten?.Invoke(commandment, keepers);
        }

        private int KeepOldCovenant()
        {
            var church = _world.Shrines.Religions
                .Where(faith => !faith.IsHeresy)
                .OrderByDescending(faith => _world.Shrines.FollowersOf(faith).Count).ThenBy(faith => faith.Shrine?.X ?? 0).ThenBy(faith => faith.Shrine?.Y ?? 0)
                .FirstOrDefault();
            if (church == null) return 0;

            var covenant = new Religion(church.Shrine, church.Dogma, true);
            covenant.Kindle(40);
            var keepers = 0;
            foreach (var cell in _world.Shrines.FollowersOf(church).ToList())
            {
                if (_world.Random.Next(100) >= _config.OldCovenantPercent) continue;

                _world.Shrines.Impose(cell, covenant, BeliefChange.Schism);
                _world.Memory.Add(cell, "old-covenant");
                keepers++;
            }

            return keepers;
        }
    }
}
