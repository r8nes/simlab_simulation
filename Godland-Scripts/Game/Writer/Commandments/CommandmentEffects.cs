using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Commandments
{
    [Serializable]
    public sealed class TilePair
    {
        [field: SerializeField, Required, AssetsOnly] public TileDefinition A { get; private set; }
        [field: SerializeField, Required, AssetsOnly] public TileDefinition B { get; private set; }
    }

    [Serializable]
    public sealed class FlipWorldEffect : ICommandmentEffect
    {
        [SerializeField, InfoBox("Каждая пара меняется местами: вода ↔ суша, огонь ↔ лёд.")] private List<TilePair> _pairs = new();
        [SerializeField, MinValue(0)] private int _radius = 1;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var world = context.World;
            var flipped = false;
            foreach (var cell in world.Grid.Coords.Where(cell => context.IsShaped(cell)
                         && Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius).ToList())
            {
                world.Tiles.TryGet(cell, out var id);
                var pair = _pairs.FirstOrDefault(candidate => candidate.A.Id == id || candidate.B.Id == id);
                if (pair == null) continue;

                world.Tiles.Place(cell, pair.A.Id == id ? pair.B.Id : pair.A.Id, cause);
                flipped = true;
            }

            return flipped;
        }
    }

    [Serializable]
    public sealed class RiseFromAbyssEffect : ICommandmentEffect
    {
        [SerializeField, Required, AssetsOnly] private List<TileDefinition> _tiles = new();
        [SerializeField, MinValue(1)] private int _cells = 2;
        [SerializeField, Range(0, 100), SuffixLabel("%")] private int _settleChancePercent = 50;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var world = context.World;
            var candidates = world.Grid.Coords
                .Where(cell => !world.Tiles.All.ContainsKey(cell) && world.Grid.Neighbors(cell).Any(world.Tiles.All.ContainsKey))
                .OrderBy(cell => Math.Abs(cell.X - site.X) + Math.Abs(cell.Y - site.Y))
                .ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Take(_cells)
                .ToList();
            foreach (var cell in candidates)
            {
                if (!world.Grid.IsUnlocked(cell)) world.Grid.Unlock(cell);
                world.Tiles.Place(cell, _tiles[world.Random.Next(_tiles.Count)].Id, cause);
                if (world.Random.Next(100) < _settleChancePercent) world.Settlements.TrySettle(cell, 2);
            }

            return candidates.Count > 0;
        }
    }

    [Serializable]
    public sealed class WakeTitanEffect : ICommandmentEffect
    {
        [SerializeField, AssetsOnly, InfoBox("Пусто — любой титан.")] private List<TitanDefinition> _titans = new();

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var pool = _titans.Count > 0 ? _titans : context.Titans.All.ToList();
            if (pool.Count == 0 || !context.IsShaped(site)) return false;

            context.World.Titans.Awaken(pool[context.World.Random.Next(pool.Count)].Rule, site, cause);
            return true;
        }
    }

    [Serializable]
    public sealed class NewRaceEffect : ICommandmentEffect
    {
        public bool Apply(CommandmentContext context, GridCoord site, string cause) =>
            context.IsShaped(site) && context.Races.Summon(site, cause);
    }

    [Serializable]
    public sealed class ManifestGodEffect : ICommandmentEffect
    {
        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var pantheon = context.World.Pantheon;
            var rules = context.Gods.Gods.Where(rule => rule.IsElemental && pantheon.CanBirth(rule.Id)).OrderBy(rule => rule.Id).ToList();
            return rules.Count > 0 && pantheon.TryBirth(rules[context.World.Random.Next(rules.Count)].Id);
        }
    }

    [Serializable]
    public sealed class DeadRiseEffect : ICommandmentEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _bones;
        [SerializeField, AssetsOnly, InfoBox("Нежить: какое чудовище встаёт из костей.")] private FeatureDefinition _undead;
        [SerializeField, MinValue(0)] private int _harm = 1;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var world = context.World;
            if (!context.IsShaped(site)) return false;

            if (world.Features.TryGet(site, out _)) world.Features.Remove(site);
            world.Tiles.Place(site, _bones.Id, cause);
            if (_undead != null && world.Features.CanAdd(site)) world.Features.AddWild(site, _undead.Id);
            foreach (var cell in world.Grid.Neighbors(site).Where(cell => world.Settlements.TryGet(cell, out _)).ToList())
                world.Settlements.Weaken(cell, _harm);
            return true;
        }
    }

    [Serializable]
    public sealed class HastenEpochEffect : ICommandmentEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 50;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            context.Epochs.Hasten(_years);
            return true;
        }
    }

    [Serializable]
    public sealed class ForgetEffect : ICommandmentEffect
    {
        [SerializeField, MinValue(0)] private int _radius = 2;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            context.World.Memory.ForgetAll(site, _radius);
            var race = context.Races.RaceAt(site);
            race?.ForgetMind();
            return true;
        }
    }

    [Serializable]
    public sealed class CurseEffect : ICommandmentEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _cursed;
        [SerializeField, MinValue(0)] private int _harm = 2;

        public bool Apply(CommandmentContext context, GridCoord site, string cause)
        {
            var world = context.World;
            if (!context.IsShaped(site) || world.Guards.IsProtected(site)) return false;

            world.Tiles.Place(site, _cursed.Id, cause);
            if (world.Settlements.TryGet(site, out _)) world.Settlements.Weaken(site, _harm);
            return true;
        }
    }
}

namespace Godland.Game.Writer.Commandments
{
    [System.Serializable]
    public sealed class RaiseRidgeEffect : ICommandmentEffect
    {
        [UnityEngine.SerializeField, Sirenix.OdinInspector.MinValue(1)] private int _size = 6;

        public bool Apply(CommandmentContext context, Godland.Core.World.Grid.GridCoord site, string cause) =>
            context.Peaks.RaiseRidge(site, _size, cause).Count > 0;
    }
}
