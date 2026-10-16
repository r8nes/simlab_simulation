using PrimeTween;
using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Menu
{
    public sealed class MenuShowcaseView : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private TMP_Text _caption;

        private Tween _spin;

        public Transform Anchor => _anchor;

        public void Spin(float turnSeconds)
        {
            _spin.Stop();
            _spin = Tween.LocalEulerAngles(_anchor, Vector3.zero, new Vector3(0f, 360f, 0f), turnSeconds, Ease.Linear, -1);
        }

        public void StopSpin() => _spin.Stop();

        public void ShowCaption(string text)
        {
            _caption.text = text;
            Tween.Alpha(_caption, 0f, 1f, 0.5f);
        }

        public void HideCaption() => Tween.Alpha(_caption, 0f, 0.4f);
    }
}
