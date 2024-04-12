using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pixelated
{
    [ExecuteAlways]
    public class Pixelate : MonoBehaviour
    {
        public const string LAYER_NAME = ".pixelated";

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

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            originalLayer = gameObject.layer;

            if ((camera.cullingMask & (1 << originalLayer)) != 0)
                gameObject.layer = LayerMask.NameToLayer(LAYER_NAME);

            // if object's layer is not on the cameras mask, keep the layer
            // and the object won't be rendered.
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            gameObject.layer = originalLayer;
        }
    }
}
