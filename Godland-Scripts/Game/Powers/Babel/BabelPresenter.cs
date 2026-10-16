using System;
using Godland.Core.Powers.Babel;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Powers.Babel
{
    public sealed class BabelPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 7f;

        private readonly TowerOfBabel _tower;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public BabelPresenter(TowerOfBabel tower, EventJournal journal, EpochView banner)
        {
            _tower = tower;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _tower.Founded += OnFounded;
            _tower.FloorBuilt += OnFloorBuilt;
            _tower.Completed += OnCompleted;
            _tower.Toppled += OnToppled;
        }

        public void Dispose()
        {
            _tower.Founded -= OnFounded;
            _tower.FloorBuilt -= OnFloorBuilt;
            _tower.Completed -= OnCompleted;
            _tower.Toppled -= OnToppled;
        }

        private void OnFounded(GridCoord _)
        {
            _banner.Announce("Вавилонская башня", "Люди строят башню до неба, чтобы добраться до богов\n<size=60%>Благословить — или низвергнуть?</size>", BannerSeconds);
            _journal.Write("Величайший город заложил башню до неба: люди хотят добраться до богов", JournalTone.Threat);
        }

        private void OnFloorBuilt(int floors) =>
            _journal.Write($"Башня растёт: этаж {floors} из {_tower.MaxFloors}", JournalTone.Info);

        private void OnCompleted(bool blessed)
        {
            if (blessed)
            {
                _banner.Announce("Башня достроена", "Весь мир принял веру строителей", BannerSeconds);
                _journal.Write("Башня достигла неба с благословения: весь мир принял веру строителей, города дают вдвое больше веры", JournalTone.Good);
                return;
            }

            _banner.Announce("Вызов небесам", "Люди достроили башню сами и отвергли чудеса", BannerSeconds);
            _journal.Write("Башня достигла неба без благословения: люди отвергли чудеса на века", JournalTone.Threat);
        }

        private void OnToppled()
        {
            _banner.Announce("Башня низвергнута", "Смешение языков", BannerSeconds);
            _journal.Write("Башня рухнула: город обезлюдел, веры по всему миру рвутся", JournalTone.Threat);
        }
    }
}
