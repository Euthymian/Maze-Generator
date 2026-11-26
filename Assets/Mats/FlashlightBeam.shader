Shader "Maze/Flashlight Beam"
{
    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            v2f_img Vert(appdata_img input)
            {
                return vert_img(input);
            }

            half4 Frag(v2f_img input) : SV_Target
            {
                float2 offset = input.uv * 2.0 - 1.0;
                float radiusSquared = dot(offset, offset);
                float edge = 1.0 - smoothstep(0.20, 0.98, radiusSquared);
                float hotspot = exp(-5.5 * radiusSquared);
                half beam = edge * lerp(0.38, 1.0, hotspot);
                return half4(beam, beam, beam, beam);
            }
            ENDHLSL
        }
    }
}
