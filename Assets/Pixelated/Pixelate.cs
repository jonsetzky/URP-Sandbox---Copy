using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Pixelate : MonoBehaviour
{
    public const string pixelLayerName = "Pixelated";
    public static int pixelLayer
    {
        get { return LayerMask.NameToLayer(pixelLayerName); }
    }
    public const string noRenderLayerName = "NoRender";
    public static int noRenderLayer
    {
        get { return LayerMask.NameToLayer(noRenderLayerName); }
    }

    [SerializeField]
    private UniversalRendererData pipelineAsset;
    private V2PixelRenderFeature pixelFeature;

    MeshRenderer meshRenderer;

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
    }

    // public void RemoveLayer(int layer)
    // {
    //     meshRenderer.renderingLayerMask &= ~(1u << layer);
    // }

    // public void AddLayer(int layer)
    // {
    //     meshRenderer.renderingLayerMask |= ~(1u << layer);
    // }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pipelineAsset.rendererFeatures.Find(x => x.GetType() == typeof(V2PixelRenderFeature));

        // SetLayer(noRenderLayer);
    }
}
