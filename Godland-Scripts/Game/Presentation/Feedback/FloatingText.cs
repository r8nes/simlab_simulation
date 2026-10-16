using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class FloatingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public async UniTask PlayAsync(string text, Color color, Vector3 start, Func<Quaternion> facing, float rise, float duration, CancellationToken token)
        {
            _label.text = text;
            transform.position = start;

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var progress = elapsed / duration;
                transform.SetPositionAndRotation(start + Vector3.up * (rise * progress), facing());
                _label.color = new Color(color.r, color.g, color.b, 1f - progress * progress);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            Destroy(gameObject);
        }
    }
}
