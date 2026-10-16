using PrimeTween;
using UnityEngine;
using VContainer;

namespace Godland.Game.World.Tiles
{
    public sealed class TileView : MonoBehaviour
    {
        private TileAnimationConfig _config;
        private float _size;
        private Tween _scaleTween;

        [Inject]
        public void Construct(TileAnimationConfig config) => _config = config;

        public void Place(Vector3 position, float cellSize)
        {
            _size = cellSize;
            transform.localPosition = position;
            transform.localScale = Vector3.one * cellSize;
        }

        public void Shrink(float factor) => _size *= factor;

        public void Appear(float delay)
        {
            transform.localScale = Vector3.zero;
            _scaleTween = Tween.Scale(transform, 0f, _size, _config.AppearDuration, _config.AppearEase, startDelay: delay);
        }

        public void Disappear()
        {
            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, 0f, _config.DisappearDuration, _config.DisappearEase)
                .OnComplete(this, view => Destroy(view.gameObject));
        }

        public void SetDepleted(bool depleted)
        {
            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, _size * (depleted ? _config.DepletedScale : 1f), _config.AppearDuration, Ease.OutQuad);
        }

        public void Pulse()
        {
            if (_scaleTween.isAlive) return;
            _scaleTween = Tween.PunchScale(transform, Vector3.one * (_size * _config.PulseStrength), _config.PulseDuration, frequency: 4f);
        }
    }
}
