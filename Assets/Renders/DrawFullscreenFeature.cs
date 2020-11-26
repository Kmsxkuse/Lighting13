using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Renders
{
    public enum BufferType
    {
        CameraColor,
        Custom 
    }

    public class DrawFullscreenFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;

            public Material BlitMaterial;
            public int BlitMaterialPassIndex = -1;
            public BufferType SourceType = BufferType.CameraColor;
            public BufferType DestinationType = BufferType.CameraColor;
            public string SourceTextureId = "_SourceTexture";
            public string DestinationTextureId = "_DestinationTexture";
        }

        public Settings Setting = new Settings();
        private DrawFullscreenPass _blitPass;

        public override void Create()
        {
            _blitPass = new DrawFullscreenPass(name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Setting.BlitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            _blitPass.renderPassEvent = Setting.RenderPassEvent;
            _blitPass.Settings = Setting;
            renderer.EnqueuePass(_blitPass);
        }
    }
}