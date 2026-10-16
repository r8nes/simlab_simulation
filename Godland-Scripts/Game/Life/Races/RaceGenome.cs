using System;
using System.Linq;
using Godland.Core.Randomness;
using Godland.Game.Fate.Emergence;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [Serializable]
    public sealed class RaceGenome
    {
        private const int Genes = 4;

        [field: SerializeField, Range(0, 100), InfoBox("Нрав народа как геном: мутирует при расселении, смешивается при завоевании. Гены сдвигают выбор деяний.")]
        public int Aggression { get; private set; } = 50;
        [field: SerializeField, Range(0, 100)] public int Fertility { get; private set; } = 50;
        [field: SerializeField, Range(0, 100)] public int Wanderlust { get; private set; } = 50;
        [field: SerializeField, Range(0, 100)] public int Curiosity { get; private set; } = 50;

        public RaceGenome Copy() => new() { Aggression = Aggression, Fertility = Fertility, Wanderlust = Wanderlust, Curiosity = Curiosity };

        public void Mutate(SeededRandom random, int step)
        {
            var delta = random.Next(step * 2 + 1) - step;
            switch (random.Next(Genes))
            {
                case 0: Aggression = Clamp(Aggression + delta); break;
                case 1: Fertility = Clamp(Fertility + delta); break;
                case 2: Wanderlust = Clamp(Wanderlust + delta); break;
                default: Curiosity = Clamp(Curiosity + delta); break;
            }
        }

        public void Blend(RaceGenome other, int percent)
        {
            Aggression = Mix(Aggression, other.Aggression, percent);
            Fertility = Mix(Fertility, other.Fertility, percent);
            Wanderlust = Mix(Wanderlust, other.Wanderlust, percent);
            Curiosity = Mix(Curiosity, other.Curiosity, percent);
        }

        public int Distance(RaceGenome other) =>
            Math.Abs(Aggression - other.Aggression) + Math.Abs(Fertility - other.Fertility) +
            Math.Abs(Wanderlust - other.Wanderlust) + Math.Abs(Curiosity - other.Curiosity);

        public int Bias(RaceAction action, int nature) => nature * (50 + GeneFor(action)) / 100;

        public string Epithet()
        {
            var genes = new[] { (Aggression, "Кровавые"), (Fertility, "Плодовитые"), (Wanderlust, "Кочевые"), (Curiosity, "Пытливые") };
            return genes.OrderByDescending(gene => gene.Item1).First().Item2;
        }

        public string Describe() => $"жестокость {Aggression}, плодовитость {Fertility}, тяга к странствиям {Wanderlust}, пытливость {Curiosity}";

        private int GeneFor(RaceAction action)
        {
            if (action.Outcomes.Any(outcome => outcome is RavageOutcome or AggressorOutcome)) return Aggression;
            if (action.Outcomes.Any(outcome => outcome is ExpandOutcome)) return Wanderlust;
            if (action.Outcomes.Any(outcome => outcome is GrowOutcome)) return Fertility;
            return Curiosity;
        }

        private static int Clamp(int value) => Math.Clamp(value, 0, 100);

        private static int Mix(int own, int other, int percent) => own + (other - own) * percent / 100;
    }
}
