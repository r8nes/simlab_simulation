using System.Collections.Generic;
using System.Linq;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Minds
{
    public sealed class NatureStrategy : IMindStrategy
    {
        public string Id => "nature";
        public string Reason => "по нраву";
        public int Urgency(Mind mind) => 0;
        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random) => option.Nature;
    }

    public sealed class HabitStrategy : IMindStrategy
    {
        public string Id => "habit";
        public string Reason => "по привычке";
        public int Urgency(Mind mind) => -mind.Despair;
        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random) =>
            mind.HabitOf(option.Id) + option.Nature / 4;
    }

    public sealed class ImitationStrategy : IMindStrategy
    {
        public string Id => "imitation";
        public string Reason => "подражая успешным";
        public int Urgency(Mind mind) => 0;

        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random)
        {
            var teacher = models.Where(model => model != mind).OrderByDescending(model => model.Success).FirstOrDefault();
            return teacher == null || teacher.Success <= mind.Success
                ? option.Nature / 2
                : teacher.HabitOf(option.Id) + option.Nature / 4;
        }
    }

    public sealed class PlanStrategy : IMindStrategy
    {
        public string Id => "plan";
        public string Reason => "по замыслу";
        public int Urgency(Mind mind) => mind.Despair / 2;
        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random) =>
            mind.ChainOf(mind.LastAction, option.Id) * 2 + option.Nature / 4;
    }

    public sealed class NoveltyStrategy : IMindStrategy
    {
        public string Id => "novelty";
        public string Reason => "наперекор другим";
        public int Urgency(Mind mind) => 0;

        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random)
        {
            var others = models.Where(model => model != mind && model.LastAction != null).ToList();
            var common = others.Count == 0 ? 0 : others.Count(model => model.LastAction == option.Id) * 100 / others.Count;
            return 100 - common + option.Nature / 4;
        }
    }

    public sealed class GambleStrategy : IMindStrategy
    {
        public string Id => "gamble";
        public string Reason => "на авось";
        public int Urgency(Mind mind) => mind.Despair;
        public int Value(Mind mind, MindOption option, IReadOnlyList<Mind> models, SeededRandom random) =>
            100 / (1 + mind.TimesTried(option.Id)) + random.Next(50);
    }
}
