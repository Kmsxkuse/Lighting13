using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Renders
{
    /// <summary>
    ///     Draws full screen mesh using given material and pass and reading from source target.
    /// </summary>
    internal class DrawFullscreenPass : ScriptableRenderPass
    {
        private readonly string _mProfilerTag;
        private readonly int _temporaryRTId = Shader.PropertyToID("_TempRT");
        private RenderTargetIdentifier _destination;
        private int _destinationId;
        private bool _isSourceAndDestinationSameTarget;

        private RenderTargetIdentifier _source;

        private int _sourceId;
        public DrawFullscreenFeature.Settings Settings;

        public DrawFullscreenPass(string tag)
        {
            _mProfilerTag = tag;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            blitTargetDescriptor.depthBufferBits = 0;

            _isSourceAndDestinationSameTarget = Settings.SourceType == Settings.DestinationType &&
                                                (Settings.SourceType == BufferType.CameraColor ||
                                                 Settings.SourceTextureId == Settings.DestinationTextureId);

            var renderer = renderingData.cameraData.renderer;

            if (Settings.SourceType == BufferType.CameraColor)
            {
                _sourceId = -1;
                _source = renderer.cameraColorTarget;
            }
            else
            {
                _sourceId = Shader.PropertyToID(Settings.SourceTextureId);
                cmd.GetTemporaryRT(_sourceId, blitTargetDescriptor, FilterMode.Point);
                _source = new RenderTargetIdentifier(_sourceId);
            }

            if (_isSourceAndDestinationSameTarget)
            {
                _destinationId = _temporaryRTId;
                cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode.Point);
                _destination = new RenderTargetIdentifier(_destinationId);
            }
            else if (Settings.DestinationType == BufferType.CameraColor)
            {
                _destinationId = -1;
                _destination = renderer.cameraColorTarget;
            }
            else
            {
                _destinationId = Shader.PropertyToID(Settings.DestinationTextureId);
                cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode.Point);
                _destination = new RenderTargetIdentifier(_destinationId);
            }
        }

        /// <inheritdoc />
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(_mProfilerTag);

            // Can't read and write to same color target, create a temp render target to blit. 
            if (_isSourceAndDestinationSameTarget)
            {
                Blit(cmd, _source, _destination, Settings.BlitMaterial, Settings.BlitMaterialPassIndex);
                Blit(cmd, _destination, _source);
            }
            else
            {
                Blit(cmd, _source, _destination, Settings.BlitMaterial, Settings.BlitMaterialPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc />
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (_destinationId != -1)
                cmd.ReleaseTemporaryRT(_destinationId);

            if (_source == _destination && _sourceId != -1)
                cmd.ReleaseTemporaryRT(_sourceId);
        }
    }
}