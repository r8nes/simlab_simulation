using TMPro;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    public sealed class TileInfoView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _details;

        public void Show(string title, string details)
        {
            _title.text = title;
            _details.text = details;
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);
    }
}
