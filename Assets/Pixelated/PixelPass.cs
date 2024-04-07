using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// bool MaskHas(int mask, int layer)
// {
//     return (mask & layer) != 0;
// }

// int AddLayerToMask(int mask, int layer)
// {
//     return mask | (1 << layer);
// }

// int RemoveLayerFromMask(int mask, int layer)
// {
//     return mask & ~(1 << layer);
// }

// Renders Pixel layered objects onto a render texture
public class PixelRenderPass : ScriptableRenderPass
{
    private RTHandle m_colorOutput;
    private RTHandle m_depthOutput;

    Material blitMat;

    int m_cameraMask;

    List<ShaderTagId> m_shaderTagIdList = new List<ShaderTagId>();
    FilteringSettings m_filteringSettings;
    RenderStateBlock m_renderStateBlock;
    DrawingSettings m_drawingSettings;
    RendererListParams m_rendererListParams;

    public RTHandle m_cameraColorTarget;
    public RTHandle m_cameraDepthTarget;

    private BufferedRTHandleSystem m_colorHandleSystem;
    private BufferedRTHandleSystem m_depthHandleSystem;

    public PixelRenderPass(
        RenderPassEvent renderEvent,
        Material blitMat,
        float pixelDensity,
        LayerMask layerMask
    )
    {
        renderPassEvent = renderEvent;
        profilingSampler = new ProfilingSampler("Pixel Render Pass");

        m_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
        m_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        this.blitMat = blitMat;
    }

    public void SetHandleSystems(BufferedRTHandleSystem color, BufferedRTHandleSystem depth)
    {
        m_colorHandleSystem = color;
        m_depthHandleSystem = depth;
    }

    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        m_drawingSettings = CreateDrawingSettings(
            m_shaderTagIdList,
            ref renderingData,
            SortingCriteria.CommonTransparent
        );
        m_rendererListParams = new RendererListParams
        {
            cullingResults = renderingData.cullResults,
            drawSettings = CreateDrawingSettings(
                m_shaderTagIdList,
                ref renderingData,
                SortingCriteria.CommonTransparent
            ),
            filteringSettings = m_filteringSettings,
            isPassTagName = true,
            tagName = new ShaderTagId("Pixel Render Objects")
        };

        ConfigureTarget(m_colorOutput, m_depthOutput);
    }

    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        CommandBuffer cmd = CommandBufferPool.Get("PixelRenderPass");
        ScriptableRenderer renderer = renderingData.cameraData.renderer;

        using (new ProfilingScope(cmd, profilingSampler))
        {
            // blitMat.SetTexture("_RTColor", hColor, RenderTextureSubElement.Color);
            // blitMat.SetTexture("_RTDepth", hDepth, RenderTextureSubElement.Depth);
            // cmd.SetRenderTarget(
            //     hColor,
            //     RenderBufferLoadAction.DontCare,
            //     RenderBufferStoreAction.Store,
            //     hDepth,
            //     RenderBufferLoadAction.DontCare,
            //     RenderBufferStoreAction.Store
            // );
            cmd.ClearRenderTarget(false, true, Color.clear);
            // CoreUtils.SetRenderTarget(cmd, hColor, hDepth, ClearFlag.Color, Color.cyan);
            var rl = context.CreateRendererList(ref m_rendererListParams);
            CoreUtils.DrawRendererList(context, cmd, rl);

            // context.DrawRenderers(
            //     renderingData.cullResults,
            //     ref m_rendererListParams.drawSettings,
            //     ref m_filteringSettings
            // );
            // context.ExecuteCommandBuffer(cmd);
            // cmd.Clear();

            // cmd.SetGlobalTexture(hColor.name, hColor.nameID, RenderTextureSubElement.Color);
            // cmd.SetGlobalTexture(hDepth.name, hDepth.nameID, RenderTextureSubElement.Depth);
            // Blitter.BlitCameraTexture(cmd, hColor, m_cameraColorTarget, blitMat, 0);

            // m_colorHandleSystem.ReleaseBuffer(cameraID);
            // m_depthHandleSystem.ReleaseBuffer(cameraID);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // m_hColor?.Release();
        // m_hDepth?.Release();
    }

    public void Dispose()
    {
        // m_hColor?.Release();
        // m_hDepth?.Release();
    }
}
