using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEditor.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/*public class PixelRenderFeature : ScriptableRendererFeature
{
    private Vector2Int GetRTDimensions(RenderingData renderingData)
    {
        const int pixelDensity = 4;
        ref CameraData cameraData = ref renderingData.cameraData;
        Camera camera = cameraData.camera;
        int pixelWidth = (int)(camera.pixelWidth / pixelDensity);
        int pixelHeight = (int)(camera.pixelHeight / pixelDensity);
        return new Vector2Int(pixelWidth, pixelHeight);
    }

    class PixelRenderPass : ScriptableRenderPass
    {
        // public RTHandle m_hColor;
        // public RTHandle m_hDepth;

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

        private bool MaskHas(int mask, int layer)
        {
            return (mask & layer) != 0;
        }

        private int AddLayerToMask(int mask, int layer)
        {
            return mask | (1 << layer);
        }

        private int RemoveLayerFromMask(int mask, int layer)
        {
            return mask & ~(1 << layer);
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
            // Debug.Log(m_hColor.nameID);

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
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData
        )
        {
            // var cameraData = renderingData.cameraData;
            // if (cameraData.camera.cameraType != CameraType.Game)
            //     return;

            CommandBuffer cmd = CommandBufferPool.Get("PixelRenderPass");
            ScriptableRenderer renderer = renderingData.cameraData.renderer;

            using (new ProfilingScope(cmd, profilingSampler))
            {
                int cameraID = renderingData.cameraData.camera.GetHashCode();
                RTHandle hColor = m_colorHandleSystem.GetFrameRT(cameraID, 0);
                RTHandle hDepth = m_depthHandleSystem.GetFrameRT(cameraID, 0);
                blitMat.SetTexture("_RTColor", hColor, RenderTextureSubElement.Color);
                blitMat.SetTexture("_RTDepth", hDepth, RenderTextureSubElement.Depth);
                cmd.SetRenderTarget(
                    hColor,
                    RenderBufferLoadAction.DontCare,
                    RenderBufferStoreAction.Store,
                    hDepth,
                    RenderBufferLoadAction.DontCare,
                    RenderBufferStoreAction.Store
                );
                cmd.ClearRenderTarget(false, true, Color.clear);
                // CoreUtils.SetRenderTarget(cmd, hColor, hDepth, ClearFlag.Color, Color.cyan);
                var rl = context.CreateRendererList(ref m_rendererListParams);
                CoreUtils.DrawRendererList(context, cmd, rl);

                // context.DrawRenderers(
                //     renderingData.cullResults,
                //     ref m_rendererListParams.drawSettings,
                //     ref m_filteringSettings
                // );
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // cmd.SetGlobalTexture(hColor.name, hColor.nameID, RenderTextureSubElement.Color);
                // cmd.SetGlobalTexture(hDepth.name, hDepth.nameID, RenderTextureSubElement.Depth);
                Blitter.BlitCameraTexture(cmd, hColor, m_cameraColorTarget, blitMat, 0);

                m_colorHandleSystem.ReleaseBuffer(cameraID);
                m_depthHandleSystem.ReleaseBuffer(cameraID);
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

    [System.Serializable]
    public class PixelRenderFeatureSettings
    {
        public LayerMask layerMask = 0;
        public RenderPassEvent Event = RenderPassEvent.BeforeRenderingTransparents;

        [Range(1f, 15f)]
        public float pixelDensity = 1f;
    }

    private BufferedRTHandleSystem colorHandles = new BufferedRTHandleSystem();
    private BufferedRTHandleSystem depthHandles = new BufferedRTHandleSystem();

    // public Shader blitShader;
    public PixelRenderFeatureSettings settings = new PixelRenderFeatureSettings();

    public Material m_material;

    PixelRenderPass m_renderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        // m_material = CoreUtils.CreateEngineMaterial(blitShader);

        RTHandles.Initialize(Screen.width, Screen.height);
        m_renderPass = new PixelRenderPass(
            settings.Event,
            m_material,
            settings.pixelDensity,
            settings.layerMask
        );
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData
    )
    {
        // does this work?
        if (!(renderingData.cameraData.cameraType is CameraType.Game or CameraType.Preview))
            return;

        int cameraID = renderingData.cameraData.camera.GetHashCode();

        colorHandles.AllocBuffer(
            cameraID,
            delegate(RTHandleSystem rtHandleSystem, int i)
            {
                return rtHandleSystem.Alloc(
                    new Vector2(0.25f, 0.25f),
                    depthBufferBits: 0,
                    filterMode: FilterMode.Point,
                    name: "_RTColor"
                );
            },
            1
        );
        depthHandles.AllocBuffer(
            cameraID,
            delegate(RTHandleSystem rtHandleSystem, int i)
            {
                return rtHandleSystem.Alloc(
                    new Vector2(0.25f, 0.25f),
                    depthBufferBits: DepthBits.Depth24,
                    filterMode: FilterMode.Point,
                    name: "_RTDepth"
                );
            },
            1
        );
        m_renderPass.SetHandleSystems(colorHandles, depthHandles);

        renderer.EnqueuePass(m_renderPass);
    }

    public override void SetupRenderPasses(
        ScriptableRenderer renderer,
        in RenderingData renderingData
    )
    {
        if (!(renderingData.cameraData.cameraType is CameraType.Game or CameraType.Preview))
            return;
        m_renderPass.m_cameraColorTarget = renderer.cameraColorTargetHandle;
        m_renderPass.m_cameraDepthTarget = renderer.cameraDepthTargetHandle;

        Camera camera = renderingData.cameraData.camera;

        // Vector2Int rtDim = GetRTDimensions(renderingData);
        var desc = renderingData.cameraData.cameraTargetDescriptor;

        // var changed = RenderingUtils.ReAllocateIfNeeded(
        //     ref m_renderPass.m_hDepth,
        //     new Vector2(0.25f, 0.25f),
        //     desc,
        //     FilterMode.Point,
        //     TextureWrapMode.Clamp,
        //     name: "_RTDepth"
        // );
        // desc.depthBufferBits = 0;
        // changed |= RenderingUtils.ReAllocateIfNeeded(
        //     ref m_renderPass.m_hColor,
        //     new Vector2(0.25f, 0.25f),
        //     desc,
        //     FilterMode.Point,
        //     TextureWrapMode.Clamp,
        //     name: "_RTColor"
        // );
        // if (changed)
        //     Debug.Log("setup render passes called");
    }

    protected override void Dispose(bool disposing)
    {
        m_renderPass.Dispose();
        colorHandles.ReleaseAll();
        colorHandles.Dispose();
    }
}*/
