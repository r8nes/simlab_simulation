using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace Godland.Editor.SimLab
{
    public sealed class SimLabShaderStripper : IPreprocessShaders
    {
        private static readonly string[] KeptPrefixes = { "UI/", "TextMeshPro/", "Hidden/", "Sprites/" };
        private static readonly string[] HeavyPostProcessing = { "UberPost", "Bloom", "LensFlare", "DepthOfField", "MotionBlur", "PaniniProjection", "SubpixelMorphological", "Stop NaN", "Edge Adaptive", "TemporalAA", "FinalPost" };

        public static bool Active { get; set; }

        public int callbackOrder => 0;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!Active) return;

            foreach (var heavy in HeavyPostProcessing)
                if (shader.name.Contains(heavy))
                {
                    data.Clear();
                    return;
                }

            foreach (var prefix in KeptPrefixes)
                if (shader.name.StartsWith(prefix)) return;

            data.Clear();
        }
    }
}
