using System;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.Features
{
    public static class CreatureNames
    {
        public static string Prefix(CreatureGene gene) => gene switch
        {
            CreatureGene.Size => "Гигантские",
            CreatureGene.Fecundity => "Плодовитые",
            CreatureGene.Ferocity => "Свирепые",
            _ => "Стойкие"
        };

        public static string Effect(CreatureGene gene) => gene switch
        {
            CreatureGene.Size => "кормят соседей вдвое",
            CreatureGene.Fecundity => "дают лишнего потомка",
            CreatureGene.Ferocity => "нападают на соседние поселения",
            _ => "расселяются на любые тайлы"
        };

        public static string Describe(Creature creature) =>
            $"размер {creature[CreatureGene.Size]}, плодовитость {creature[CreatureGene.Fecundity]}, свирепость {creature[CreatureGene.Ferocity]}, стойкость {creature[CreatureGene.Hardiness]}";

        public static string NameOf(Creature creature, string baseName) =>
            creature?.Lineage is { } lineage ? $"{Prefix(lineage)} {baseName.ToLowerInvariant()}" : baseName;
    }

    public sealed class CreaturesPresenter : IStartable, IDisposable
    {
        private readonly CreatureGenes _genes;
        private readonly TileFeatures _features;
        private readonly FeatureCatalog _catalog;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;

        public CreaturesPresenter(CreatureGenes genes, TileFeatures features, FeatureCatalog catalog, EventJournal journal, Chronicle chronicle)
        {
            _genes = genes;
            _features = features;
            _catalog = catalog;
            _journal = journal;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _genes.Speciated += OnSpeciated;
            _genes.Mauled += OnMauled;
        }

        public void Dispose()
        {
            _genes.Speciated -= OnSpeciated;
            _genes.Mauled -= OnMauled;
        }

        private string NameAt(GridCoord cell) =>
            _features.TryGet(cell, out var id) ? CreatureNames.NameOf(_genes.TryGet(cell, out var creature) ? creature : null, _catalog.Get(id).DisplayName) : "звери";

        private void OnSpeciated(GridCoord cell, CreatureGene gene)
        {
            var text = $"Новый вид: {NameAt(cell)} — {CreatureNames.Effect(gene)}";
            _journal.Write(text, JournalTone.Info);
            _chronicle.Add(text, cell);
        }

        private void OnMauled(GridCoord cell, GridCoord victim) =>
            _journal.Write($"{NameAt(cell)} напали на поселение ({victim.X}, {victim.Y})", JournalTone.Threat);
    }
}
