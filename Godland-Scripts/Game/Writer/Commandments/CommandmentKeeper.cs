using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentKeeper : IWorldStep
    {
        private readonly CommandmentConfig _config;
        private readonly CommandmentContext _context;
        private readonly List<GridCoord> _pending = new();
        private int _readyYear;
        private bool _applying;
        private readonly List<(int Year, GridCoord Place)> _recent = new();

        public CommandmentKeeper(CommandmentConfig config, CommandmentContext context, Feuds feuds)
        {
            feuds.Raided += (attacker, _, _) => Witness(CommandmentTrigger.War, attacker);
            _config = config;
            _context = context;
            var world = context.World;
            world.Settlements.Abandoned += cell => Witness(CommandmentTrigger.Death, cell);
            world.Settlements.Founded += cell => Witness(CommandmentTrigger.SettlementBorn, cell);
            world.Settlements.HuntFinished += (cell, _) => Witness(CommandmentTrigger.Hunt, cell);
            world.Pantheon.Born += god => Witness(CommandmentTrigger.GodBornOrFallen, god.Seat);
            world.Pantheon.Fallen += god => Witness(CommandmentTrigger.GodBornOrFallen, god.Seat);
            world.Realms.Conquered += (_, _, cell) => Witness(CommandmentTrigger.War, cell);
            world.Tiles.Removed += cell => Witness(CommandmentTrigger.TileLost, cell);
            world.Epoch.Advanced += _ =>
            {
                if (context.RandomShaped() is { } cell) Witness(CommandmentTrigger.EpochChanged, cell);
            };
        }

        public event Action<Commandment> Proclaimed;
        public event Action<Commandment, GridCoord> Fired;

        public Commandment Current { get; private set; }
        public int TimesFired { get; private set; }
        public int ReadyYear => _readyYear;
        public IReadOnlyList<(int Year, GridCoord Place)> Recent => _recent;

        public void Proclaim(Commandment commandment)
        {
            Current = commandment;
            _pending.Clear();
            Proclaimed?.Invoke(commandment);
        }

        public void Advance(int year)
        {
            if (Current == null) return;

            if (Current.When.Trigger == CommandmentTrigger.Prayer && year % _config.PrayerPeriod == 0
                && _context.Pick(_context.World.Settlements.All.Keys.Where(cell => _context.World.Shrines.TryGet(cell, out _))) is { } believer)
                _pending.Add(believer);

            if (_pending.Count == 0 || year < _readyYear)
            {
                _pending.Clear();
                return;
            }

            var origin = _pending[_context.World.Random.Next(_pending.Count)];
            _pending.Clear();
            Fire(origin, year);
        }

        private void Witness(CommandmentTrigger trigger, GridCoord cell)
        {
            if (_applying || Current == null || Current.When.Trigger != trigger) return;

            _pending.Add(cell);
        }

        private void Fire(GridCoord origin, int year)
        {
            var boost = Current.Boost;
            var dark = _context.World.Modifiers.IsNight(year) || _context.Catastrophes.Age != null;
            if (boost == CommandmentAmplifier.Dark && !dark) return;

            var repeats = boost switch
            {
                CommandmentAmplifier.Twice => 2,
                CommandmentAmplifier.Dark => _config.DarkMultiplier,
                _ => 1
            };
            var cause = $"заповедь: {Current.Text}";
            var fired = false;
            _applying = true;
            try
            {
                for (var i = 0; i < repeats; i++)
                foreach (var site in Current.Where.Place.Sites(_context, origin).ToList())
                {
                    if (!Current.Then.Effect.Apply(_context, site, cause)) continue;

                    fired = true;
                    _context.World.History.Note(site, cause);
                    _context.World.Memory.Add(site, "commandment");
                    if (boost == CommandmentAmplifier.Forever)
                    {
                        var guarded = site;
                        _context.World.Guards.Add(cell => cell == guarded);
                    }
                }
            }
            finally
            {
                _applying = false;
            }

            if (!fired) return;

            if (boost == CommandmentAmplifier.Price
                && _context.World.Settlements.All.OrderByDescending(pair => pair.Value).Select(pair => (GridCoord?)pair.Key).FirstOrDefault() is { } payer)
                _context.World.Settlements.Weaken(payer, 1);

            _readyYear = year + Current.Then.CooldownYears;
            TimesFired++;
            _recent.Insert(0, (year, origin));
            if (_recent.Count > 5) _recent.RemoveAt(_recent.Count - 1);
            Fired?.Invoke(Current, origin);
        }
    }
}
