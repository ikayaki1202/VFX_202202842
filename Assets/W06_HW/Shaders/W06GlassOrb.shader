Shader "W06/Glass Orb"
{
    Properties
    {
        _BaseColor ("Glass tint", Color) = (0.72,0.91,0.98,1)
        _RimColor ("Edge reflection", Color) = (0.69,0.91,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.98
        _Opacity ("Tint opacity", Range(0,1)) = 0.16
        _Refraction ("Refraction", Range(0,0.1)) = 0.025
        _Reflection ("Environment reflection", Range(0,2)) = 0.75
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "GlassSurface"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _Smoothness, _Opacity, _Refraction, _Reflection;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = p.positionCS;
                output.positionWS = p.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(p.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(n,view));
                half fresnel = pow(1-facing,3.6);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                half3 normalVS = mul((half3x3)UNITY_MATRIX_V,n);
                float2 bend = normalVS.xy * _Refraction * (0.35+facing);
                half3 through = SampleSceneColor(saturate(screenUV-bend));
                // Slight dispersion at the thick curved edge makes the clear volume legible.
                through.r = lerp(through.r,SampleSceneColor(saturate(screenUV-bend*1.09)).r,0.55);
                through.b = lerp(through.b,SampleSceneColor(saturate(screenUV-bend*0.91)).b,0.55);
                half3 reflected = GlossyEnvironmentReflection(reflect(-view,n), input.positionWS, 1-_Smoothness, 1, screenUV);
                half3 color = through * lerp(half3(1,1,1),_BaseColor.rgb,_Opacity);
                color = lerp(color,reflected * _Reflection + _RimColor.rgb*0.15,0.045+fresnel*0.76);
                Light sun = GetMainLight();
                half3 h = SafeNormalize(sun.direction+view);
                half spec = pow(saturate(dot(n,h)),lerp(80,640,_Smoothness));
                // A broad soft highlight complements the actual environment reflection.
                half soft = pow(saturate(dot(n,normalize(half3(-0.45,0.7,-0.55)))),42);
                color += sun.color * spec * 2.1 + half3(0.78,0.92,1)*soft*0.22;
                color += _RimColor.rgb * pow(1-facing,7)*0.26;
                color = MixFog(color,input.fogFactor);
                return half4(color,saturate(0.83+_Opacity*0.13+fresnel*0.04));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
