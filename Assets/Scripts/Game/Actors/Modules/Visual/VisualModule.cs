using Core.Actors.Modules.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Visual
{
    public sealed class VisualModule : IVisual, IInitializableModule
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Renderer renderer;
        private readonly float tintStrength;
        private readonly MaterialPropertyBlock block = new();
        private Color baseColor;

        public VisualModule(Renderer renderer, float tintStrength)
        {
            this.renderer = renderer;
            this.tintStrength = tintStrength;
        }

        public void Initialize()
        {
            baseColor = renderer.sharedMaterial.GetColor(BaseColorId);
            ClearTint();
        }

        public void ShowTint(Color tint)
        {
            block.SetColor(BaseColorId, Color.Lerp(baseColor, tint, tintStrength));
            renderer.SetPropertyBlock(block);
        }

        public void ClearTint()
        {
            renderer.SetPropertyBlock(null);
        }
    }
}
