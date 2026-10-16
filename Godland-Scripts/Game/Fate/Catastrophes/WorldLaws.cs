using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class SuppressGodsLaw : IAgeLaw
    {
        [SerializeField, InfoBox("Живые боги падают в начале эпохи.")] private bool _slayLiving;

        public void Begin(CatastropheContext context, string cause)
        {
            if (_slayLiving)
                foreach (var god in context.World.Pantheon.Gods.ToList()) context.World.Pantheon.Slay(god);
            context.World.Pantheon.Suppress(true);
        }

        public void Year(CatastropheContext context, int age, string cause) { }

        public void End(CatastropheContext context) => context.World.Pantheon.Suppress(false);
    }

    [Serializable]
    public sealed class AwakenTitansLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 80;
        [SerializeField, MinValue(1)] private int _maxAwake = 3;
        [SerializeField, Required, AssetsOnly] private List<TitanDefinition> _titans = new();

        public void Begin(CatastropheContext context, string cause) => Awaken(context);

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period == 0) Awaken(context);
        }

        public void End(CatastropheContext context) { }

        private void Awaken(CatastropheContext context)
        {
            var world = context.World;
            if (_titans.Count == 0 || world.Titans.All.Count(titan => titan.IsAwake) >= _maxAwake) return;

            var land = context.Shuffled(world.Tiles.Keys(context));
            if (land.Length == 0) return;

            world.Titans.Awaken(_titans[world.Random.Next(_titans.Count)].Rule, land[0], "эпоха будит титанов");
        }
    }

    [Serializable]
    public sealed class PlagueLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 40;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var settlements = context.Shuffled(context.World.Settlements.All.Keys);
            if (settlements.Length > 0) context.World.Plague.Strike(settlements[0]);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class MemoryLaw : IAgeLaw
    {
        [SerializeField, Required, InfoBox("Что эпоха копит в общей памяти мира каждый период.")] private string _key;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 10;
        [SerializeField, MinValue(1)] private int _amount = 1;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period == 0) context.World.Memory.Add(context.World.Grid.Center, _key, _amount);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class TransformNearSettlementsLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(1), InfoBox("Сколько клеток меняется за период во всём мире.")] private int _cells = 2;
        [SerializeField, MinValue(1), InfoBox("Только вокруг поселений не меньше этого.")] private int _minPopulation = 6;
        [SerializeField, Required, AssetsOnly] private List<Godland.Game.World.Tiles.TileDefinition> _from = new();
        [SerializeField, Required, AssetsOnly] private Godland.Game.World.Tiles.TileDefinition _to;
        [SerializeField, InfoBox("Что эпоха пишет в память клетки.")] private string _memoryKey;
        [SerializeField, InfoBox("Менять и заселённые клетки (люди остаются, их уклад меняется вместе с тайлом).")] private bool _includeSettled;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var cells = world.Settlements.All
                .Where(pair => pair.Value >= _minPopulation)
                .SelectMany(pair => world.Grid.Neighbors(pair.Key).Append(pair.Key))
                .Distinct()
                .Where(cell => context.IsShaped(cell) && !world.Guards.IsProtected(cell) && (_includeSettled || !world.Settlements.TryGet(cell, out _))
                    && world.Tiles.TryGet(cell, out var id) && _from.Any(tile => tile.Id == id));
            foreach (var cell in context.Shuffled(cells).Take(_cells))
            {
                if (world.Features.TryGet(cell, out _)) world.Features.Remove(cell);
                world.Tiles.Place(cell, _to.Id, cause);
                if (!string.IsNullOrEmpty(_memoryKey)) world.Memory.Add(cell, _memoryKey);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class ShrinkLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(0), InfoBox("Сколько жителей теряет каждое поселение.")] private int _loss = 1;
        [SerializeField, MinValue(0), InfoBox("Сколько самых маленьких поселений пустеет совсем.")] private int _abandon;
        [SerializeField, AssetsOnly, InfoBox("Во что превращается опустевшее место (пусто — не меняется).")] private Godland.Game.World.Tiles.TileDefinition _left;
        [SerializeField, MinValue(0), InfoBox("Не трогать мир, где поселений не больше этого.")] private int _keep = 8;
        [SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Сколько лет опустевшее место никто не заселяет.")] private int _desertYears = 150;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            if (world.Settlements.All.Count <= _keep) return;

            if (_loss > 0)
                foreach (var cell in world.Settlements.All.Keys.ToList()) world.Settlements.Weaken(cell, _loss);

            var smallest = world.Settlements.All.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key.X).ThenBy(pair => pair.Key.Y)
                .Take(Math.Min(_abandon, Math.Max(0, world.Settlements.All.Count - _keep))).ToList();
            foreach (var (cell, _) in smallest)
            {
                world.Settlements.Leave(cell);
                world.Settlements.Desert(cell, world.Clock.Year + _desertYears);
                if (_left != null && !world.Settlements.TryGet(cell, out _) && context.IsShaped(cell)) world.Tiles.Place(cell, _left.Id, cause);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class TradeLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(1), InfoBox("Торговые пути связывают поселения не меньше этого.")] private int _minPopulation = 6;
        [SerializeField, MinValue(1)] private int _distance = 3;
        [SerializeField, MinValue(1), InfoBox("Сколько путей держит одно поселение.")] private int _routes = 2;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var hubs = world.Settlements.All.Where(pair => pair.Value >= _minPopulation).Select(pair => pair.Key)
                .OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            foreach (var hub in hubs)
            {
                var partners = hubs.Where(other => other != hub && Math.Abs(other.X - hub.X) + Math.Abs(other.Y - hub.Y) <= _distance).Take(_routes).ToList();
                if (partners.Count == 0) continue;

                if (world.Settlements.TryGet(hub, out var population) && population < world.Settlements.CapacityOf(hub)) world.Settlements.Bless(hub, 1);
                world.Memory.Add(hub, "trade", partners.Count);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class SmogLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(1), InfoBox("Мастерские больших поселений коптят землю вокруг: плодородие соседей падает.")] private int _minPopulation = 8;
        [SerializeField, MinValue(1)] private int _loss = 1;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            foreach (var (city, _) in world.Settlements.All.Where(pair => pair.Value >= _minPopulation).ToList())
            foreach (var cell in world.Grid.Neighbors(city).Append(city))
            {
                context.Fertility.Add(cell, -_loss);
                world.Memory.Add(cell, "smoke");
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class MegacityLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 25;
        [SerializeField, MinValue(1), InfoBox("Мегаполисы тянут людей из соседних сёл: столько жителей уходит из каждого соседа в крупнейшее поселение рядом.")] private int _pull = 1;
        [SerializeField, MinValue(1)] private int _minPopulation = 10;
        [SerializeField, MinValue(0), InfoBox("Насколько мегаполис может переполниться сверх вместимости.")] private int _overflow = 4;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            foreach (var (city, population) in world.Settlements.All.Where(pair => pair.Value >= _minPopulation).OrderByDescending(pair => pair.Value).ToList())
            foreach (var neighbor in world.Grid.Neighbors(city).Where(cell => world.Settlements.TryGet(cell, out var other) && other > _pull && other < population).ToList())
            {
                if (!world.Settlements.TryGet(city, out var current) || current >= world.Settlements.CapacityOf(city) + _overflow) break;

                world.Settlements.TryGet(neighbor, out var before);
                world.Settlements.Weaken(neighbor, _pull);
                var moved = world.Settlements.TryGet(neighbor, out var after) ? before - after : before;
                if (moved > 0) world.Settlements.Bless(city, moved);
            }
        }

        public void End(CatastropheContext context) { }
    }

    internal static class CatastropheTiles
    {
        public static IEnumerable<Godland.Core.World.Grid.GridCoord> Keys(this Godland.Core.World.Tiles.WorldTiles tiles, CatastropheContext context) =>
            tiles.All.Keys.Where(cell => context.IsShaped(cell) && !context.World.Guards.IsProtected(cell));
    }
}
