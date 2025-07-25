Shader "Unlit/FullScreenDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlitTexture("Blit Texture", 2D) = "white" {}
    }
    SubShader
    {
         Tags { "RenderType"="Opaque" "Queue"="Overlay" }

        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

        Varyings Vert(uint vertexID : SV_VertexID)
        {
           Varyings o;

            // Step 1: Generate vertex positions for fullscreen triangle using vertexID
            // vertexID goes from 0 to 2 for the three triangle vertices
            float2 uv = float2((vertexID << 1) & 2, vertexID & 2);

            // Step 2: Convert those values (0 or 2) to UV coordinates in range [0,1]
            float2 uv01 = uv * 0.5;

            // Step 3: Pass UV coordinates to fragment shader
            o.uv = uv01;

            // Step 4: Convert UV coordinates to clip space position for the vertex
            // Clip space is [-1,1] in X and Y, so multiply by 2 and subtract 1
            o.positionHCS = float4(uv01 * 2.0 - 1.0, 0.0, 1.0);

            return o;
        
 
        }

               TEXTURE2D(_BlitTexture);           
               SAMPLER(sampler_BlitTexture);


           float DrawRing(float2 uv, float2 center, float radius, float thickness, float blur)
            {
                        float dist = distance(uv, center);

                        float halfThickness = thickness * 0.5;
                        float innerEdge = radius - halfThickness;
                        float outerEdge = radius + halfThickness;

                        float outer = smoothstep(outerEdge, outerEdge - blur, dist);
                        float inner = smoothstep(innerEdge, innerEdge + blur, dist);

                        return outer - inner; // ring with smooth edges
            }

     
             
            float4 Frag(Varyings i) : SV_Target
            {

                float2 uv = i.uv;
                uv.y = 1.0 - uv.y;

                // Center in screen-space
                float2 center = float2(0.5, 0.5);
                float speed = 0.8;  
                // Outer ring
                float outerRadius = 0.5;
           
                float animatedOuterRadius = frac(_Time.y * speed) * outerRadius;  
             
                float thickness = 0.02;
               
                float blur = 0.01;
                float ring1 = DrawRing(uv, center, animatedOuterRadius, thickness, blur);



                float waveFreq = 20;
                float waveAmp = 0.01;
                float wave = sin((uv.x + uv.y + _Time.y) * waveFreq) * waveAmp;

                float outerRadiusDistorted = animatedOuterRadius + wave;
            



                
             
              

                // Inner ring to cut out (must be smaller radius)
                float innerRadius = 0.4;
                float animatedInnerRadius = frac(_Time.y * speed) * innerRadius; // loop every few seconds
                float ring2 = DrawRing(uv, center, animatedInnerRadius, thickness * .7, blur);

                // Subtract inner from outer to get the final edge
                float result = ring1 - ring2;
            //   return float4(result, result, result, 1); // white edge, black inside
                    //  Distort based on ring intensity
                    float2 dir = normalize(uv - center);
                    float distortionStrength = 0.03; // tweak this
                    float2 distortedUV = uv + dir * result * distortionStrength;

                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, distortedUV);
                return color;
 
            }

            ENDHLSL
        }
    }
}
