using System;
using VContainer.Unity;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftOfferPresenter : IInitializable, IDisposable
    {
        private readonly GiftOffers _offers;
        private readonly GiftOfferView _view;

        public GiftOfferPresenter(GiftOffers offers, GiftOfferView view)
        {
            _offers = offers;
            _view = view;
        }

        public void Initialize()
        {
            _view.Hide();
            _offers.Offered += Show;
            _offers.Chosen += OnChosen;
        }

        public void Dispose()
        {
            _offers.Offered -= Show;
            _offers.Chosen -= OnChosen;
        }

        private void Show()
        {
            foreach (var gift in _offers.Current)
                _view.AddCard().Bind(gift.DisplayName, gift.Describe(), () => _offers.Choose(gift));
            _view.Show();
        }

        private void OnChosen(GiftDefinition _) => _view.Hide();
    }
}
