using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;

namespace Godland.Game.Writer.Dilemmas
{
    public sealed class DilemmaOption
    {
        public DilemmaOption(string label, int creation, int faith, Action apply)
        {
            Label = label;
            Creation = creation;
            Faith = faith;
            Apply = apply;
        }

        public string Label { get; }
        public int Creation { get; }
        public int Faith { get; }
        public Action Apply { get; }
    }

    public sealed class Dilemma
    {
        public Dilemma(string title, string text, GridCoord? place, params DilemmaOption[] options)
        {
            Title = title;
            Text = text;
            Place = place;
            Options = options.Where(option => option != null).ToList();
        }

        public string Title { get; }
        public string Text { get; }
        public GridCoord? Place { get; }
        public IReadOnlyList<DilemmaOption> Options { get; }
    }

    public sealed class DilemmaQueue
    {
        private readonly Queue<Dilemma> _pending = new();
        private readonly CreationPoints _creation;
        private readonly FaithPoints _faith;
        private readonly SeededRandom _random;

        public DilemmaQueue(CreationPoints creation, FaithPoints faith, SeededRandom random)
        {
            _creation = creation;
            _faith = faith;
            _random = random;
        }

        public event Action<Dilemma> Offered;
        public event Action<Dilemma, DilemmaOption> Resolved;

        public bool AutoChoose { get; set; }
        public Dilemma Current { get; private set; }

        public bool CanAfford(DilemmaOption option) => _creation.CanAfford(option.Creation) && _faith.CanAfford(option.Faith);

        public void Offer(Dilemma dilemma)
        {
            if (dilemma.Options.Count == 0) return;

            if (AutoChoose)
            {
                var affordable = dilemma.Options.Where(CanAfford).ToList();
                Resolve(dilemma, affordable.Count == 0 ? dilemma.Options[0] : affordable[_random.Next(affordable.Count)]);
                return;
            }

            _pending.Enqueue(dilemma);
            if (Current == null) Next();
        }

        public void Choose(DilemmaOption option)
        {
            if (Current == null || !Current.Options.Contains(option) || !CanAfford(option)) return;

            var dilemma = Current;
            Current = null;
            Resolve(dilemma, option);
            Next();
        }

        private void Resolve(Dilemma dilemma, DilemmaOption option)
        {
            if (!CanAfford(option)) option = dilemma.Options[0];
            _creation.TrySpend(option.Creation);
            _faith.TrySpend(option.Faith);
            option.Apply?.Invoke();
            Resolved?.Invoke(dilemma, option);
        }

        private void Next()
        {
            if (_pending.Count == 0) return;

            Current = _pending.Dequeue();
            Offered?.Invoke(Current);
        }
    }
}
