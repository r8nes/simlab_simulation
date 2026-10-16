using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Timing
{
    public sealed class ClockView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _year;
        [SerializeField] private TMP_Text _speed;

        public void ShowYear(int year) => _year.text = $"Год {year}";

        public void ShowSpeed(bool paused, float speed) => _speed.text = paused ? "Пауза" : $"x{speed:0.#}";
    }
}
