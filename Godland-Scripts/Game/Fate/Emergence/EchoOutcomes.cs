using System;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class MemoryOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, InfoBox("Что мир запомнит в месте события — так одно возникновение рождает следующее.")] private string _key;
        [SerializeField, MinValue(1)] private int _amount = 1;

        public void Apply(EmergenceContext context, GridCoord site, string title) => context.Memory.Add(site, _key, _amount);
    }

    [Serializable]
    public sealed class RouseTitanOutcome : IEmergenceOutcome
    {
        [SerializeField, InfoBox("Будит ближайшего спящего титана (усыплённого или спящего от рождения).")] private bool _nearest = true;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var sleepers = context.Titans.All.Where(titan => !titan.IsAwake && !titan.IsTamed);
            var sleeper = (_nearest
                    ? sleepers.OrderBy(titan => Math.Abs(titan.Position.X - site.X) + Math.Abs(titan.Position.Y - site.Y))
                    : sleepers.OrderBy(titan => titan.BornYear))
                .FirstOrDefault();
            if (sleeper != null) context.Titans.Rouse(sleeper);
        }
    }

    [Serializable]
    public sealed class SummonHeroOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(0), InfoBox("Герой рождается в поселении места события или в ближайшем в этом радиусе.")] private int _radius = 2;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var home = context.Settlements.All.Keys
                .Where(cell => Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius)
                .OrderBy(cell => Math.Abs(cell.X - site.X) + Math.Abs(cell.Y - site.Y)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (home is { } cell) context.Heroes.Summon(cell);
        }
    }

    [Serializable]
    public sealed class KindleOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(0)] private int _radius = 2;
        [SerializeField, InfoBox("Отрицательное значение остужает веру.")] private int _fervor = 15;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var religions = context.Settlements.All.Keys
                .Where(cell => Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius)
                .Select(cell => context.Shrines.TryGet(cell, out var religion) ? religion : null)
                .Where(religion => religion != null)
                .Distinct()
                .ToList();
            foreach (var religion in religions)
                if (_fervor >= 0) religion.Kindle(_fervor);
                else religion.Cool(-_fervor);
        }
    }
}
