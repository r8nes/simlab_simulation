using TMPro;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    public sealed class TileBrushView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public void Show(string tileName, int cost, int cellCost) =>
            _label.text = $"Тайл: {tileName} ({cost}) [Tab]   Клетка: {cellCost}";
    }
}
