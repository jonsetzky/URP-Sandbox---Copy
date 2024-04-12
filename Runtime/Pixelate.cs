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
            RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
            RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
            RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;
        }

        //         private void Update()
        //         {
        // #if UNITY_EDITOR
        //             // this is required to make the layer changable in editor
        //             if (
        //                 gameObject.layer != originalLayer
        //                 && gameObject.layer != LayerMask.NameToLayer(LAYER_NAME)
        //             )
        //                 originalLayer = gameObject.layer;
        // #endif
        //         }

        void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            // Put the code that you want to execute before the camera renders here
            // If you are using URP or HDRP, Unity calls this method automatically
            // If you are writing a custom SRP, you must call RenderPipeline.BeginFrameRendering
            originalLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer(LAYER_NAME);
        }

        void OnEndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            // Put the code that you want to execute before the camera renders here
            // If you are using URP or HDRP, Unity calls this method automatically
            // If you are writing a custom SRP, you must call RenderPipeline.BeginFrameRendering

            gameObject.layer = originalLayer;
        }
    }
}
