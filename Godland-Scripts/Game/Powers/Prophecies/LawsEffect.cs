using System;
using System.Collections.Generic;
using Godland.Core;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Epochs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class LawsEffect : IMilestoneEffect
    {
        [SerializeField] private string _cause = "веха";
        [SerializeReference] private List<IAgeLaw> _laws = new();

        public void Apply(ProphecyContext context)
        {
            foreach (var law in _laws) context.Laws.Enact(law, _cause);
        }
    }

    [Serializable]
    public sealed class HastenEpochsEffect : IMilestoneEffect
    {
        [SerializeField, MinValue(1), InfoBox("На сколько лет раньше наступает следующая эпоха за каждый век.")] private int _yearsPerCentury = 50;

        public void Apply(ProphecyContext context) => context.Laws.Accelerate(_yearsPerCentury);
    }

    public sealed class MilestoneLawKeeper : IWorldStep
    {
        private readonly CatastropheContext _context;
        private readonly EpochProgression _epochs;
        private readonly List<(IAgeLaw Law, int Start, string Cause)> _laws = new();
        private int _year;

        public MilestoneLawKeeper(CatastropheContext context, EpochProgression epochs)
        {
            _context = context;
            _epochs = epochs;
        }

        public int HastePerCentury { get; private set; }

        public void Enact(IAgeLaw law, string cause)
        {
            _laws.Add((law, _year, cause));
            law.Begin(_context, cause);
        }

        public void Accelerate(int yearsPerCentury) => HastePerCentury += yearsPerCentury;

        public void Advance(int year)
        {
            _year = year;
            if (HastePerCentury > 0 && year % 100 == 0) _epochs.Hasten(HastePerCentury);

            foreach (var (law, start, cause) in _laws.ToArray())
                if (year > start) law.Year(_context, year - start, cause);
        }
    }
}
