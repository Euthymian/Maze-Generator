Shader "Maze/Day Night Skybox"
{
    Properties
    {
        _DayHorizon ("Day Horizon", Color) = (0.76, 0.85, 0.94, 1)
        _DayZenith ("Day Zenith", Color) = (0.22, 0.47, 0.76, 1)
        _NightHorizon ("Night Horizon", Color) = (0.28, 0.38, 0.57, 1)
        _NightZenith ("Night Zenith", Color) = (0.065, 0.11, 0.23, 1)
        _NightBlend ("Night Blend", Range(0, 1)) = 0
        [HideInInspector] _SunDirection ("Sun Direction", Vector) = (0.3214, 0.7660, -0.5567, 0)
        [HideInInspector] _MoonDirection ("Moon Direction", Vector) = (-0.4368, 0.6691, -0.6012, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DayHorizon;
                half4 _DayZenith;
                half4 _NightHorizon;
                half4 _NightZenith;
                half _NightBlend;
                float4 _SunDirection;
                float4 _MoonDirection;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float Hash(float2 cell)
            {
                float3 seed = frac(cell.xyx * float3(0.1031, 0.1030, 0.0973));
                seed += dot(seed, seed.yzx + 33.33);
                return frac((seed.x + seed.y) * seed.z);
            }

            float Noise(float2 samplePosition)
            {
                float2 cell = floor(samplePosition);
                float2 blend = smoothstep(0.0, 1.0, frac(samplePosition));
                float bottom = lerp(Hash(cell), Hash(cell + float2(1, 0)), blend.x);
                float top = lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), blend.x);
                return lerp(bottom, top, blend.y);
            }

            float Clouds(float3 direction)
            {
                float height = max(direction.y, 0.0);
                float2 samplePosition = direction.xz / (height + 0.32) * float2(3.0, 5.5);
                float cloud = 0.57 * Noise(samplePosition) + 0.29 * Noise(2.05 * samplePosition) + 0.14 * Noise(4.1 * samplePosition);
                return smoothstep(0.53, 0.8, cloud) * smoothstep(0.03, 0.24, height);
            }

            float Stars(float3 direction)
            {
                float latitude = asin(saturate(direction.y)) * (2.0 / PI);
                float longitude = frac(atan2(direction.z, direction.x) / (2.0 * PI) + 0.5);
                float2 grid = float2(longitude, latitude) * float2(144, 52);
                float2 cell = floor(grid);
                float seed = Hash(cell);
                float2 center = 0.25 + 0.5 * float2(Hash(cell + 19.7), Hash(cell + 41.3));
                float size = lerp(0.035, 0.10, Hash(cell + 62.4));
                float starDistance = length(frac(grid) - center);
                float star = 1.0 - smoothstep(size * 0.45, size + 0.025, starDistance);
                float visibility = smoothstep(0.08, 0.37, direction.y) * (1.0 - smoothstep(0.97, 1.0, direction.y));
                return star * smoothstep(0.88, 0.99, seed) * visibility;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float height = saturate(direction.y);
                float skyBlend = smoothstep(0.0, 0.9, height);
                half3 daySky = lerp(_DayHorizon.rgb, _DayZenith.rgb, skyBlend);
                half3 nightSky = lerp(_NightHorizon.rgb, _NightZenith.rgb, skyBlend);
                float cloud = Clouds(direction);

                float sunAlignment = saturate(dot(direction, normalize(_SunDirection.xyz)));
                float sunDisc = smoothstep(0.9995, 0.99983, sunAlignment);
                daySky = lerp(daySky, half3(0.89, 0.92, 0.98), cloud * 0.54);
                daySky += half3(1.0, 0.69, 0.35) * (0.28 * pow(sunAlignment, 26.0) + sunDisc);

                float3 moonDirection = normalize(_MoonDirection.xyz);
                float moonAlignment = saturate(dot(direction, moonDirection));
                float moonDisc = smoothstep(0.99945, 0.99965, moonAlignment);
                float3 moonUp = abs(moonDirection.y) < 0.98 ? float3(0, 1, 0) : float3(1, 0, 0);
                float3 moonRight = normalize(cross(moonUp, moonDirection));
                float3 shadowDirection = normalize(moonDirection + moonRight * 0.023);
                float moonShadow = smoothstep(0.99945, 0.99965, dot(direction, shadowDirection));
                float crescent = moonDisc * (1.0 - moonShadow);
                float moonGlow = 0.13 * pow(moonAlignment, 49.0) + 0.13 * pow(moonAlignment, 260.0);
                nightSky = lerp(nightSky, half3(0.12, 0.19, 0.32), cloud * 0.32);
                nightSky += half3(0.44, 0.64, 1.0) * moonGlow;
                if (_NightBlend > 0.0)
                    nightSky += half3(0.66, 0.78, 1.0) * (Stars(direction) * (1.0 - cloud) * (1.0 - moonDisc));
                nightSky = lerp(nightSky, half3(0.81, 0.89, 1.0), crescent);

                return half4(lerp(daySky, nightSky, _NightBlend), 1.0);
            }
            ENDHLSL
        }
    }
}
