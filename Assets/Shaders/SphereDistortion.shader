Shader "Unlit/SphereDisstortion"
{
  Properties
    {
        _Distortion ("Distortion Amount", Range(0, 0.2)) = 0.05
        _Alpha ("Alpha", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" }

        Pass
        {
            Name "Distort"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float _Distortion;
            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(GetObjectToWorldMatrix(), v.vertex);
                o.worldPos = world.xyz;
                o.pos = TransformWorldToHClip(world.xyz);
                o.worldNormal = normalize(mul((float3x3)GetObjectToWorldMatrix(), v.normal));
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Normalize screen UV coords [0..1]
        float2 screenUV = i.screenPos.xy / i.screenPos.w;

        // Convert normal to view space
        float3 viewNormal = normalize(TransformWorldToViewDir(i.worldNormal));

        // Calculate Fresnel for edge intensity
        float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
        float fresnel = pow(1.0 - saturate(dot(viewDir, i.worldNormal)), 3.0);

        // Distortion offset scaled by fresnel and strength
        float distortionStrength = 0.3; // increase if effect is weak
        float2 offset = viewNormal.xy * distortionStrength * fresnel;

        // Apply offset to UVs and clamp
        float2 distortedUV = clamp(screenUV + offset, 0.001, 0.999);

        // Sample scene color at distorted UV
        half4 color = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV);

        // Return with alpha modulated by fresnel for soft edges
        return half4(color.rgb, fresnel * 0.5);
            }

            ENDHLSL
        }
    }
}
