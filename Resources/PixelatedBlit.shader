Shader "Unlit/Pixelate/Blit"
{
    	Properties 
    	{
    	    [HideInInspector]_RTColor ("Base (RGB)", 2D) = "red" {}
    	    [HideInInspector]_RTDepth ("Base (RGB)", 2D) = "red" {}
            _rtHandleScale ("RT Handle Scale", Vector) = (0,0,0,0)
    	}
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        Pass
        {
            Name "PixelBlitPass"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            // ZTest Always
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_RTColor);
            SAMPLER(sampler_RTColor);

            TEXTURE2D_X(_RTDepth);
            SAMPLER(sampler_RTDepth);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _rtHandleScale;

            float SampleDepth(float2 uv) {
                return SAMPLE_DEPTH_TEXTURE(_RTDepth, sampler_RTDepth, uv);
            }
            
            float SampleCameraDepth(float2 uv) {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            }

            half4 frag (Varyings input, out float depth : SV_DEPTH) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // using the formula in 
                // https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@12.0/manual/rthandle-system-using.html#using-rthandles-in-shaders:~:text=float2%20scaledUVs%20%3D%20fullScreenUVs%20*%20rtHandleScale.xy%3B
                float2 fullScreenUVs = input.texcoord / _rtHandleScale.xy;

                float4 color = SAMPLE_TEXTURE2D_X(_RTColor, sampler_RTColor, input.texcoord);
                depth = SampleDepth(input.texcoord);

                // do ztesting manually since we're blitting on a quad/triangle
                if (SampleCameraDepth(fullScreenUVs) > SampleDepth(input.texcoord))
                    clip(-1);

                return color;
            }
            ENDHLSL
        }
    }
}