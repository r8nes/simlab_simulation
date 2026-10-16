using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Commandments
{
    [Serializable]
    public sealed class HerePlace : ICommandmentPlace
    {
        public IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin)
        {
            yield return origin;
        }
    }

    [Serializable]
    public sealed class AroundPlace : ICommandmentPlace
    {
        [SerializeField, MinValue(1)] private int _radius = 2;
        [SerializeField, MinValue(1)] private int _count = 2;

        public IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin)
        {
            var cells = context.World.Grid.Coords
                .Where(cell => cell != origin && Math.Max(Math.Abs(cell.X - origin.X), Math.Abs(cell.Y - origin.Y)) <= _radius && context.IsShaped(cell))
                .OrderBy(cell => cell.X).ThenBy(cell => cell.Y)
                .OrderBy(_ => context.World.Random.Next(100000))
                .Take(_count);
            return cells.ToList();
        }
    }

    [Serializable]
    public sealed class OppositePlace : ICommandmentPlace
    {
        public IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin)
        {
            var center = context.World.Grid.Center;
            var mirror = new GridCoord(2 * center.X - origin.X, 2 * center.Y - origin.Y);
            var target = context.World.Grid.Coords.Where(context.World.Grid.IsUnlocked)
                .OrderBy(cell => Math.Abs(cell.X - mirror.X) + Math.Abs(cell.Y - mirror.Y))
                .ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>()
                .FirstOrDefault();
            if (target is { } cell) yield return cell;
        }
    }

    [Serializable]
    public sealed class EverywherePlace : ICommandmentPlace
    {
        public IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin)
        {
            var center = context.World.Grid.Center;
            var quarters = context.World.Tiles.All.Keys.Where(context.IsShaped)
                .GroupBy(cell => (Math.Sign(cell.X - center.X), Math.Sign(cell.Y - center.Y)))
                .OrderBy(group => group.Key.Item1).ThenBy(group => group.Key.Item2);
            foreach (var quarter in quarters)
                if (context.Pick(quarter) is { } cell) yield return cell;
        }
    }

    [Serializable]
    public sealed class StrongestPlace : ICommandmentPlace
    {
        public IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin)
        {
            var strongest = context.World.Settlements.All
                .OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key.X).ThenBy(pair => pair.Key.Y)
                .Select(pair => (GridCoord?)pair.Key)
                .FirstOrDefault();
            if (strongest is { } cell) yield return cell;
        }
    }
}
