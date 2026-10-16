using Godland.Game.World.Tiles;
using UnityEngine;

namespace Godland.Game.Powers.Gods
{
    public sealed class GodAvatarView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private TileView _body;
        [SerializeField] private Renderer[] _tinted;

        public TileView Body => _body;

        public void Tint(Color color)
        {
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
            block.SetColor(EmissionColorId, color * 2f);
            foreach (var renderer in _tinted) renderer.SetPropertyBlock(block);
        }
    }
}
