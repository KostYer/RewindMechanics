Shader "Unlit/FullScreenDistortionTEST"
{
   SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            Name "Test"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            v2f vert(uint id : SV_VertexID)
            {
                float2 quad[4] = {
                    float2(-1, -1),
                    float2( 1, -1),
                    float2(-1,  1),
                    float2( 1,  1)
                };

                v2f o;
                o.uv = (quad[id].xy + 1.0) * 0.5;
                o.pos = float4(quad[id], 0, 1);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 offset = float2(sin(i.uv.y * 50.0) * 0.01, 0);
                float2 uv = i.uv + offset;
                return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
            }

            ENDHLSL
        }
    }
}
