using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Godland.Game.Fate.Epochs
{
    public sealed class EpochView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _banner;

        private const int MaxNotes = 3;

        private Sequence _bannerSequence;

        public void ShowCurrent(string epochName, string goal, IEnumerable<string> notes)
        {
            var text = $"Эпоха: {epochName}";
            if (goal != null) text += $"\n<size=70%>Далее — {goal}</size>";
            var shown = 0;
            foreach (var note in notes)
                if (shown++ < MaxNotes) text += $"\n<size=70%>{note}</size>";

            if (shown > MaxNotes) text += $"\n<size=62%><color=#9a948a>…и ещё {shown - MaxNotes} — во вкладке «Эпоха и мир» (O)</color></size>";
            _label.text = text;
        }

        public void Announce(string header, string title, float duration)
        {
            _bannerSequence.Stop();
            _banner.text = $"{header}\n<size=140%>{title}</size>";
            _banner.alpha = 0f;
            _banner.gameObject.SetActive(true);

            var fade = duration * 0.2f;
            _bannerSequence = Sequence.Create()
                .Chain(Tween.Alpha(_banner, 1f, fade))
                .ChainDelay(duration - fade * 2f)
                .Chain(Tween.Alpha(_banner, 0f, fade))
                .ChainCallback(this, view => view._banner.gameObject.SetActive(false));
        }
    }
}
