using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    public sealed class EmergenceKeeper : IWorldStep
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private const int HungerPeriod = 10;
        private const int GrowthPeriod = 5;

        private readonly EmergenceConfig _config;
        private readonly EmergenceContext _context;
        private readonly Dictionary<EmergenceDefinition, int> _counts = new();

        public EmergenceKeeper(EmergenceConfig config, EmergenceContext context, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _config = config;
            _context = context;
        }

        public event Action<EmergenceDefinition, GridCoord> Emerged;

        public int TimesOf(EmergenceDefinition emergence) => _counts.GetValueOrDefault(emergence);

        public void Advance(int year)
        {
            var center = _context.Grid.Center;
            if (_context.Biomes.Current != null) _context.Memory.Add(center, "superbiome-years");
            _context.Memory.Add(center, "quiet");
            if (_context.Memory.Get(center, "tyrant-fell") > 0 && _context.Sovereigns.Ruler == null) _context.Memory.Add(center, "tyrant-age");
            if (year % GrowthPeriod == 0)
                foreach (var (cell, featureId) in _context.Features.All.ToList())
                    _context.Memory.Add(cell, $"grown:{featureId}", GrowthPeriod);
                foreach (var settlement in _context.Settlements.All.Keys.ToList())
                foreach (var cell in _context.Grid.Neighbors(settlement).Append(settlement))
                    if (_context.Features.TryGet(cell, out var eaten))
                        _context.Memory.Add(settlement, $"ate:{eaten}", GrowthPeriod);
            if (year % HungerPeriod == 0)
                foreach (var settlement in _context.Settlements.All.Keys.Where(_context.Settlements.IsHungry).ToList())
                    _context.Memory.Add(settlement, "hunger");
            if (year % _config.CheckPeriod != 0) return;

            foreach (var emergence in _config.Emergences)
            {
                if (_counts.GetValueOrDefault(emergence) >= emergence.MaxPerRun || _context.Epoch.Index < emergence.MinEpoch) continue;

                var sites = emergence.Condition.Sites(_context).ToList();
                if (sites.Count == 0) continue;

                var site = sites[_context.Random.Next(sites.Count)];
                if (_context.Random.Next(100) >= _flow.Chance($"emergence:{emergence.name}", emergence.ChancePercent * _context.Modifiers.EmergenceChancePercent / 100))
                {
                    if (emergence.IsFleeting) emergence.Condition.Consume(_context, site);
                    continue;
                }

                emergence.Condition.Consume(_context, site);
                foreach (var outcome in emergence.Outcomes) outcome.Apply(_context, site, emergence.Title);
                _context.History.Note(site, $"возникло: {emergence.Title}");
                _flow.Note($"emergence:{emergence.name}");
                _context.Memory.Add(site, $"emerged:{emergence.name}");
                _context.Memory.Add(_context.Grid.Center, $"emerged:{emergence.name}");
                _counts[emergence] = _counts.GetValueOrDefault(emergence) + 1;
                Emerged?.Invoke(emergence, site);
            }
        }
    }
}
