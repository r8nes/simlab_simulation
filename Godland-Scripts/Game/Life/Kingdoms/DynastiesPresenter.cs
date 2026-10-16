using System;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Life.Kingdoms
{
    public sealed class DynastiesPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 3f;

        private readonly Dynasties _dynasties;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;
        private readonly EpochView _banner;

        public DynastiesPresenter(Dynasties dynasties, EventJournal journal, Chronicle chronicle, EpochView banner)
        {
            _dynasties = dynasties;
            _journal = journal;
            _chronicle = chronicle;
            _banner = banner;
        }

        public void Start()
        {
            _dynasties.Crowned += OnCrowned;
            _dynasties.Died += OnDied;
            _dynasties.Abdicated += OnAbdicated;
            _dynasties.Usurped += OnUsurped;
            _dynasties.CivilWar += OnCivilWar;
            _dynasties.WonderBuilt += OnWonder;
            _dynasties.Ruled += OnRuled;
            _dynasties.Specialized += OnSpecialized;
        }

        public void Dispose()
        {
            _dynasties.Crowned -= OnCrowned;
            _dynasties.Died -= OnDied;
            _dynasties.Abdicated -= OnAbdicated;
            _dynasties.Usurped -= OnUsurped;
            _dynasties.CivilWar -= OnCivilWar;
            _dynasties.WonderBuilt -= OnWonder;
            _dynasties.Ruled -= OnRuled;
            _dynasties.Specialized -= OnSpecialized;
        }

        private void OnCrowned(Kingdom kingdom, Ruler ruler) =>
            Write($"Царство {kingdom.Name}: коронован {ruler.Title}, {Dynasties.NameOf(ruler.Trait)}", JournalTone.Info, kingdom.Capital);

        private void OnDied(Kingdom kingdom, Ruler ruler, string cause) =>
            _journal.Write($"Царство {kingdom.Name}: {ruler.Title} {cause}");

        private void OnAbdicated(Kingdom kingdom, Ruler old, Ruler heir) =>
            Write($"Царство {kingdom.Name}: {old.Title} отрёкся и передал корону — {heir.Title}", JournalTone.Info, kingdom.Capital);

        private void OnUsurped(Kingdom kingdom, Ruler heir, Ruler usurper)
        {
            _banner.Announce("Узурпация", kingdom.Name, BannerSeconds);
            Write($"Царство {kingdom.Name}: наследника {heir.Title} не признали — трон захватил {usurper.Title}", JournalTone.Threat, kingdom.Capital);
        }

        private void OnCivilWar(Kingdom kingdom, GridCoord rebel, string cause)
        {
            _banner.Announce("Гражданская война", kingdom.Name, BannerSeconds);
            Write($"Гражданская война в царстве {kingdom.Name}: {cause}, город восстал", JournalTone.Threat, rebel);
        }

        private void OnWonder(Kingdom kingdom, WonderDefinition wonder)
        {
            _banner.Announce("Чудо света", wonder.Name, BannerSeconds);
            Write($"Царство {kingdom.Name} возвело чудо света: {wonder.Name}. {wonder.Text}", JournalTone.Good, kingdom.Capital);
        }

        private void OnRuled(Kingdom kingdom, Ruler ruler, string deed) => _journal.Write($"{ruler.Title} {deed}");

        private void OnSpecialized(GridCoord city, CitySpecialty specialty) => _journal.Write($"Новый город стал: {Dynasties.NameOf(specialty)}");

        private void Write(string text, JournalTone tone, GridCoord place)
        {
            _journal.Write(text, tone);
            _chronicle.Add(text, place);
        }
    }
}
