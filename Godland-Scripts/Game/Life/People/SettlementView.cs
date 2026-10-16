using Godland.Game.World.Tiles;
using PrimeTween;
using UnityEngine;

namespace Godland.Game.Life.People
{
    public sealed class SettlementView : MonoBehaviour
    {
        [SerializeField] private TileView _body;
        [SerializeField] private Transform[] _huts;
        [SerializeField] private Transform _villageHall;
        [SerializeField] private Transform _cityWalls;

        private Vector3[] _hutScales;
        private Vector3 _hallScale;
        private Vector3 _wallsScale;

        public TileView Body => _body;

        public void SetPopulation(int population, bool isVillage, bool isCity)
        {
            if (_hutScales == null)
            {
                _hutScales = new Vector3[_huts.Length];
                for (var i = 0; i < _huts.Length; i++) _hutScales[i] = _huts[i].localScale;
                _hallScale = _villageHall.localScale;
                _wallsScale = _cityWalls.localScale;
            }

            var visibleHuts = Mathf.Clamp((population + 1) / 2, 1, _huts.Length);
            for (var i = 0; i < _huts.Length; i++)
                Toggle(_huts[i], _hutScales[i], i < visibleHuts);
            Toggle(_villageHall, _hallScale, isVillage);
            Toggle(_cityWalls, _wallsScale, isCity);
        }

        private static void Toggle(Transform part, Vector3 scale, bool visible)
        {
            if (part.gameObject.activeSelf == visible) return;

            part.gameObject.SetActive(visible);
            if (visible) Tween.Scale(part, Vector3.zero, scale, 0.3f, Ease.OutBack);
        }
    }
}
