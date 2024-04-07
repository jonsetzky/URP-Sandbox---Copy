// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PixelatedBlit"
{
    	Properties 
    	{
    	    [HideInInspector]_RTColor ("Base (RGB)", 2D) = "red" {}
    	    [HideInInspector]_RTDepth ("Base (RGB)", 2D) = "red" {}
    		_PixelDensity ("Pixel Density", float) = 10
            _Power ("Power", float) = 50
    		_PosterizationCount ("Count", int) = 8
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

            float SampleDepth(float2 uv)
            {
                return SAMPLE_DEPTH_TEXTURE(_RTDepth, sampler_RTDepth, uv);
            }
            
            float SampleCameraDepth(float2 uv) {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            }

            half4 frag (Varyings input, out float depth : SV_DEPTH) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 fullScreenUVs = input.texcoord / _rtHandleScale.xy;
                float4 color = SAMPLE_TEXTURE2D_X(_RTColor, sampler_RTColor, input.texcoord);
                depth = SampleDepth(input.texcoord);
                if (SampleCameraDepth(fullScreenUVs) > SampleDepth(input.texcoord))
                    clip(-1);

                // depth = 0;
                return color;
            }
            ENDHLSL
        }
    }
}