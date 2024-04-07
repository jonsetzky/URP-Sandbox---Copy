using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class V2PixelRenderFeature : ScriptableRendererFeature
{
    private RTHandle m_ColorHandle = null;
    private RTHandle m_DepthHandle = null;

    public Material m_Material;

    [SerializeField, Layer]
    public int m_Layer;
    public LayerMask m_LayerMask
    {
        get { return 1 << m_Layer; }
    }

    public RenderPassEvent m_RPEvent = RenderPassEvent.BeforeRenderingTransparents;

    V2PixelRenderPass m_RenderPass = null;

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData
    )
    {
        if (renderingData.cameraData.cameraType == CameraType.Game
        // || renderingData.cameraData.cameraType == CameraType.Preview
        )
            renderer.EnqueuePass(m_RenderPass);
    }

    public override void SetupRenderPasses(
        ScriptableRenderer renderer,
        in RenderingData renderingData
    )
    {
        if (renderingData.cameraData.cameraType == CameraType.Game
        // || renderingData.cameraData.cameraType == CameraType.Preview
        )
        {
            m_RenderPass.SetTarget(
                renderer.cameraColorTargetHandle,
                renderer.cameraDepthTargetHandle
            );

            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            m_RenderPass.ConfigureInput(ScriptableRenderPassInput.None);
        }
    }

    public override void Create()
    {
        m_RenderPass = new V2PixelRenderPass(
            m_Material,
            new LayerMask { value = 1 << m_Layer },
            m_RPEvent
        );

        if (m_ColorHandle == null)
            m_ColorHandle = RTHandles.Alloc(
                new Vector2(0.25f, 0.25f),
                filterMode: FilterMode.Point,
                wrapMode: TextureWrapMode.Clamp,
                depthBufferBits: DepthBits.None,
                name: "_RTColor"
            );

        if (m_DepthHandle == null)
            m_DepthHandle = RTHandles.Alloc(
                new Vector2(0.25f, 0.25f),
                filterMode: FilterMode.Point,
                wrapMode: TextureWrapMode.Clamp,
                depthBufferBits: DepthBits.Depth24,
                name: "_RTDepth"
            );
        m_RenderPass.Setup(m_ColorHandle, m_DepthHandle);
    }

    protected override void Dispose(bool disposing)
    {
        m_ColorHandle?.Release();
        m_ColorHandle = null;
        m_DepthHandle?.Release();
        m_DepthHandle = null;
    }
}
