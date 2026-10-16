using System;
using System.Collections.Generic;
using Godland.Core.Life.Heroes;
using Godland.Game.Fate.Epochs;
using Godland.Game.Powers.Gods;
using Godland.Game.World.Grid;
using Godland.Game.Presentation.Journal;
using Godland.Game.World.Tiles;
using PrimeTween;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Heroes
{
    public sealed class HeroesPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 4f;
        private const float StepSeconds = 0.6f;

        private readonly HeroSaga _saga;
        private readonly HeroConfig _config;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<Hero, GodAvatarView> _avatars = new();
        private readonly Dictionary<Hero, string> _announced = new();

        public HeroesPresenter(
            HeroSaga saga, HeroConfig config, EventJournal journal, EpochView banner, GridGeometry geometry, TilesView root,
            IObjectResolver resolver)
        {
            _saga = saga;
            _config = config;
            _journal = journal;
            _banner = banner;
            _geometry = geometry;
            _root = root;
            _resolver = resolver;
        }

        public void Start()
        {
            _saga.Born += OnBorn;
            _saga.Moved += OnMoved;
            _saga.Departed += OnDeparted;
            _saga.Triumphed += OnTriumphed;
            _saga.Perished += OnPerished;
            _saga.Retired += OnRetired;
            _saga.Faded += OnFaded;
            _saga.Fiasco += OnFiasco;
            _saga.Allied += OnAllied;
            _saga.Dueled += OnDueled;
            _saga.Treaty += OnTreaty;
            _saga.Ascended += OnAscended;
        }

        public void Dispose()
        {
            _saga.Born -= OnBorn;
            _saga.Moved -= OnMoved;
            _saga.Departed -= OnDeparted;
            _saga.Triumphed -= OnTriumphed;
            _saga.Perished -= OnPerished;
            _saga.Retired -= OnRetired;
            _saga.Faded -= OnFaded;
            _saga.Fiasco -= OnFiasco;
            _saga.Allied -= OnAllied;
            _saga.Dueled -= OnDueled;
            _saga.Treaty -= OnTreaty;
            _saga.Ascended -= OnAscended;
        }

        private void OnBorn(Hero hero)
        {
            var avatar = _resolver.Instantiate(_config.AvatarPrefab, _root.Root);
            avatar.name = $"Hero {hero.Name}";
            avatar.Body.Place(_geometry.ToWorld(hero.Position), _geometry.CellSize * _config.AvatarScale);
            avatar.Body.Appear(0f);
            avatar.Tint(hero.Archetype is HeroArchetypeDefinition archetype ? archetype.Color : _config.Color);
            _avatars[hero] = avatar;

            _banner.Announce("Родился герой", hero.Name, BannerSeconds);
            _journal.Write($"Родился герой своей эпохи: {hero.Name}, {TraitName(hero.Trait)}", JournalTone.Good);
        }

        private void OnMoved(Hero hero)
        {
            if (_avatars.TryGetValue(hero, out var avatar))
                Tween.Position(avatar.Body.transform, _geometry.ToWorld(hero.Position), StepSeconds, Ease.InOutSine);
        }

        private void OnDeparted(Hero hero)
        {
            var quest = QuestName(hero.Quest);
            if (_announced.TryGetValue(hero, out var last) && last == quest) return;

            _announced[hero] = quest;
            _journal.Write($"{hero.Name} выступает: {quest}", JournalTone.Info);
        }

        private void OnTriumphed(Hero hero)
        {
            _announced.Remove(hero);
            if (!hero.IsDemigod) Remove(hero);
            var blessing = hero.Blessed ? " с благословения свыше" : "";
            _banner.Announce("Легенда", $"{hero.Name}: {QuestName(hero.Quest)}", BannerSeconds);
            _journal.Write($"Легенда: {hero.Name} одолел — {QuestName(hero.Quest)}{blessing}", JournalTone.Good);
        }

        private void OnPerished(Hero hero)
        {
            _announced.Remove(hero);
            Remove(hero);
            _journal.Write($"Трагедия: {hero.Name} пал в походе — {QuestName(hero.Quest)}. Город скорбит", JournalTone.Threat);
        }

        private void OnRetired(Hero hero)
        {
            _announced.Remove(hero);
            Remove(hero);
            _journal.Write($"{hero.Name} так и не нашёл подвига и ушёл на покой", JournalTone.Info);
        }

        private void OnFaded(Hero hero)
        {
            _announced.Remove(hero);
            Remove(hero);
            _journal.Write($"{hero.Name} уходит в легенды ушедшей эпохи", JournalTone.Info);
        }

        private void OnFiasco(Hero hero)
        {
            _announced.Remove(hero);
            Remove(hero);
            _banner.Announce("Грандиозный провал", $"{hero.Name}: {FiascoName(hero.Quest)}", BannerSeconds);
            _journal.Write($"Грандиозный провал: {hero.Name} — {FiascoName(hero.Quest)}", JournalTone.Threat);
        }

        private void OnAllied(Hero leader, Hero follower)
        {
            Remove(follower);
            _journal.Write($"{leader.Name} и {follower.Name} заключают союз", JournalTone.Good);
        }

        private void OnDueled(Hero winner, Hero loser)
        {
            Remove(loser);
            _journal.Write($"Дуэль героев: {winner.Name} сразил {loser.Name}", JournalTone.Threat);
        }

        private void OnTreaty(Hero first, Hero second, bool peace)
        {
            var result = peace ? " — их царства заключили мир" : "";
            _journal.Write($"{first.Name} и {second.Name} договорились разойтись миром{result}", JournalTone.Good);
        }

        private static string TraitName(HeroTrait trait) => trait switch
        {
            HeroTrait.Brave => "отважный",
            HeroTrait.Reckless => "безрассудный",
            HeroTrait.Wise => "мудрый",
            _ => "гордый"
        };

        private static string FiascoName(QuestKind quest) => quest switch
        {
            QuestKind.Monster => "чудовище вырвалось и заразило округу",
            QuestKind.Corruption => "Источник порчи взорвался",
            QuestKind.God => "бог в ярости, город героя пал перед ним",
            _ => "всё пошло прахом"
        };

        private void OnAscended(Hero hero)
        {
            if (_avatars.TryGetValue(hero, out var avatar)) avatar.Tint(UnityEngine.Color.yellow);
            _banner.Announce("Полубог", $"{hero.Name} стал полубогом", BannerSeconds);
            _journal.Write($"{hero.Name} после легенды стал полубогом: не стареет и ищет новые подвиги", JournalTone.Good);
        }

        private void Remove(Hero hero)
        {
            if (_avatars.Remove(hero, out var avatar)) avatar.Body.Disappear();
        }

        private static string QuestName(QuestKind quest) => quest switch
        {
            QuestKind.Monster => "чудовище",
            QuestKind.Corruption => "Источник порчи",
            QuestKind.God => "чужой бог",
            QuestKind.War => "вражеское поселение",
            QuestKind.Titan => "титан",
            _ => "странствие"
        };
    }
}
