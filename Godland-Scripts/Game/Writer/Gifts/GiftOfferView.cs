using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftOfferView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GiftCardView _cardPrefab;

        public GiftCardView AddCard() => Instantiate(_cardPrefab, _cardsContainer);

        public void Show() => _root.SetActive(true);

        public void Hide()
        {
            _root.SetActive(false);
            for (var i = _cardsContainer.childCount - 1; i >= 0; i--)
                Destroy(_cardsContainer.GetChild(i).gameObject);
        }
    }
}
