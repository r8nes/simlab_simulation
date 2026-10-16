using System;
using Godland.Core.World.Grid;
using UnityEngine;

namespace Godland.Game.World.Grid
{
    public sealed class GridView : MonoBehaviour
    {
        private static readonly int GridSizeId = Shader.PropertyToID("_GridSize");
        private static readonly int CellMaskId = Shader.PropertyToID("_CellMask");
        private static readonly int VisibilityId = Shader.PropertyToID("_Visibility");
        private static readonly int RealmMaskId = Shader.PropertyToID("_RealmMask");

        [SerializeField] private Renderer _renderer;

        private Texture2D _cellMask;
        private Texture2D _realmMask;
        private MaterialPropertyBlock _properties;

        public void Build(int width, int height, Vector2 size)
        {
            transform.localScale = new Vector3(size.x, size.y, 1f);

            _cellMask = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _cellMask.SetPixels32(new Color32[width * height]);
            _cellMask.Apply();

            _realmMask = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _realmMask.SetPixels32(new Color32[width * height]);
            _realmMask.Apply();

            _properties = new MaterialPropertyBlock();
            _properties.SetTexture(RealmMaskId, _realmMask);
            _properties.SetVector(GridSizeId, new Vector4(width, height));
            _properties.SetTexture(CellMaskId, _cellMask);
            SetVisibility(0f);
        }

        public void SetUnlocked(GridCoord coord) => WriteCell(coord, pixel => new Color(pixel.r, 1f, pixel.b, pixel.a));

        public void SetPurchasable(GridCoord coord) => WriteCell(coord, pixel => new Color(pixel.r, 0.25f, pixel.b, pixel.a));

        public void SetOccupied(GridCoord coord) => WriteCell(coord, pixel => new Color(pixel.r, 0.5f, pixel.b, pixel.a));

        public void SetHighlight(GridCoord coord, float intensity) => WriteCell(coord, pixel => new Color(intensity, pixel.g, pixel.b, pixel.a));

        public void SetFertility(GridCoord coord, float intensity) => WriteCell(coord, pixel => new Color(pixel.r, pixel.g, intensity, pixel.a));

        public void SetSelected(GridCoord coord, bool selected) => WriteCell(coord, pixel => new Color(pixel.r, pixel.g, pixel.b, selected ? 1f : 0f));

        public void SetRealm(GridCoord coord, Color? color)
        {
            _realmMask.SetPixel(coord.X, coord.Y, color is { } tint ? new Color(tint.r, tint.g, tint.b, 1f) : Color.clear);
            _realmMask.Apply();
        }

        public void SetVisibility(float visibility)
        {
            _properties.SetFloat(VisibilityId, visibility);
            _renderer.SetPropertyBlock(_properties);
        }

        public void Release()
        {
            Destroy(_cellMask);
            Destroy(_realmMask);
        }

        private void WriteCell(GridCoord coord, Func<Color, Color> change)
        {
            _cellMask.SetPixel(coord.X, coord.Y, change(_cellMask.GetPixel(coord.X, coord.Y)));
            _cellMask.Apply();
        }
    }
}
