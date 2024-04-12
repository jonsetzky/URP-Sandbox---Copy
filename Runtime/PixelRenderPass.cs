using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Pixelated
{
    public class PixelRenderPass : ScriptableRenderPass
    {
        RTHandle m_ColorHandle;
        RTHandle m_DepthHandle;

        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Pixel Render Pass");
        Material m_BlitMaterial;
        Material m_CopyDepthMaterial;
        RTHandle m_CameraColorTarget;
        RTHandle m_CameraDepthTarget;

        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        FilteringSettings m_FilteringSettings;
        RendererListParams m_RendererListParams;

        public PixelRenderPass(RenderPassEvent rpEvent)
        {
            m_BlitMaterial = CoreUtils.CreateEngineMaterial("Unlit/Pixelate/Blit");
            m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial("Unlit/Pixelate/CopyDepth");

            renderPassEvent = rpEvent;
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_FilteringSettings = new FilteringSettings(
                RenderQueueRange.opaque,
                1 << LayerMask.NameToLayer(Pixelate.LAYER_NAME)
            );
        }

        public void SetTarget(RTHandle colorHandle, RTHandle depthHandle)
        {
            m_CameraColorTarget = colorHandle;
            m_CameraDepthTarget = depthHandle;
        }

        public void Setup(RTHandle colorHandle, RTHandle depthHandle)
        {
            m_ColorHandle = colorHandle;
            m_DepthHandle = depthHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            // remove norender layer from the camera
            // camera.cullingMask &= ~(1 << LayerMask.NameToLayer("NoRender"));

            // ConfigureTarget(m_CameraColorTarget);
            // var desc = renderingData.cameraData.cameraTargetDescriptor;
            m_RendererListParams = new RendererListParams
            {
                cullingResults = renderingData.cullResults,
                drawSettings = CreateDrawingSettings(
                    m_ShaderTagIdList,
                    ref renderingData,
                    SortingCriteria.CommonTransparent
                ),
                filteringSettings = m_FilteringSettings,
                isPassTagName = true,
                tagName = new ShaderTagId("Pixel Render Objects")
            };
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // m_DestinationColor = null;
            // m_DestinationDepth = null;
        }

        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData
        )
        {
            var cameraData = renderingData.cameraData;
            if (
                (
                    cameraData.camera.cameraType != CameraType.Game
                    && cameraData.camera.cameraType != CameraType.SceneView
                )
            )
                return;

            if (m_BlitMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                ConfigureTarget(m_ColorHandle, m_DepthHandle);
                cmd.ClearRenderTarget(true, true, Color.clear); // clear both buffers

                // cmd.ClearRenderTarget(false, true, Color.clear); // clear color only
                m_BlitMaterial.SetTexture("_RTColor", m_ColorHandle, RenderTextureSubElement.Color);
                m_BlitMaterial.SetTexture("_RTDepth", m_DepthHandle, RenderTextureSubElement.Depth);
                m_BlitMaterial.SetVector(
                    "_rtHandleScale",
                    m_ColorHandle.rtHandleProperties.rtHandleScale
                );
                // m_Material.SetFloat("_Intensity", m_Intensity);

                var rl = context.CreateRendererList(ref m_RendererListParams);
                CoreUtils.DrawRendererList(context, cmd, rl);
                Blitter.BlitCameraTexture(
                    cmd,
                    m_ColorHandle,
                    m_CameraColorTarget,
                    m_BlitMaterial,
                    0
                );

                m_CopyDepthMaterial.SetTexture(
                    "_RTDepth",
                    m_DepthHandle,
                    RenderTextureSubElement.Depth
                );
                Blitter.BlitCameraTexture(
                    cmd,
                    m_DepthHandle,
                    m_CameraDepthTarget,
                    m_CopyDepthMaterial,
                    0
                );
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }
}
