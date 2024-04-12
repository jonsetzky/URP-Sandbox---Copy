using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pixelated
{
    [ExecuteAlways]
    [AddComponentMenu("Pixelated/Pixelate")]
    public class Pixelate : MonoBehaviour
    {
        public const string LAYER_NAME = ".pixelated";

        [SerializeField]
        private bool m_ApplyToChildren = false;

        [HideInInspector]
        public MeshRenderer meshRenderer;

        private int originalLayer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalLayer = gameObject.layer;
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void SetLayerRecursively(int layer)
        {
            gameObject.layer = layer;
            if (m_ApplyToChildren)
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject child = transform.GetChild(i).gameObject;
                    child.layer = layer;
                }
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            originalLayer = gameObject.layer;

            if ((camera.cullingMask & (1 << originalLayer)) == 0)
                return;
            // if object's layer is not on the cameras mask, keep the layer
            // and the object won't be rendered.

            SetLayerRecursively(LayerMask.NameToLayer(LAYER_NAME));
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            SetLayerRecursively(originalLayer);
        }
    }
}
