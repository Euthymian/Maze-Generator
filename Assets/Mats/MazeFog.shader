Shader "Maze/Distance Fog"
{
    Properties
    {
        _FogStart ("Fog Start", Float) = 0.8
        _FogDensity ("Fog Density", Range(0, 1)) = 0.26
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Distance Fog"
            Cull Off
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha, Zero One

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _FogStart;
                float _FogDensity;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float depth = SampleSceneDepth(input.texcoord);

                // Pixels at the far plane belong to the skybox.
                #if UNITY_REVERSED_Z
                    if (depth <= 0.0)
                        return 0;
                #else
                    if (depth >= 1.0)
                        return 0;
                #endif

                float distancePastStart = max(LinearEyeDepth(depth, _ZBufferParams) - _FogStart, 0.0);
                float opacity = 1.0 - exp(-distancePastStart * _FogDensity);
                return half4(unity_AmbientSky.rgb, opacity);
            }
            ENDHLSL
        }
    }
}
