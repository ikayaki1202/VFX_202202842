Shader "W06/Impact Particle"
{
    Properties
    {
        _BaseColor ("Transparent material tint", Color) = (0.65,0.88,0.97,1)
        _Opacity ("Tint strength", Range(0,1)) = 0.24
        _Refraction ("Small particle refraction", Range(0,0.025)) = 0.004
        _Smoothness ("Smoothness", Range(0,1)) = 0.96
        _Reflection ("Environment reflection", Range(0,2)) = 0.70
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "TransparentImpact"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            // The supplied shard and droplet meshes are closed volumes. Drawing the
            // far side as another transparent layer makes these tiny shapes look solid.
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
                half _Opacity, _Refraction, _Smoothness, _Reflection;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 color : COLOR;
                half fogFactor : TEXCOORD2;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = p.positionCS;
                output.positionWS = p.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(p.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(n,view));
                half fresnel = pow(1-facing,3.2);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                half3 normalVS = mul((half3x3)UNITY_MATRIX_V,n);
                half3 scene = SampleSceneColor(saturate(screenUV-normalVS.xy*_Refraction));
                half3 tint = _BaseColor.rgb*lerp(half3(1,1,1),input.color.rgb,0.35);
                half3 through = scene*lerp(half3(1,1,1),tint,_Opacity);
                half3 reflected = GlossyEnvironmentReflection(reflect(-view,n),input.positionWS,1-_Smoothness,1,screenUV);
                half reflectAmount = 0.24+fresnel*0.60;
                half3 color = lerp(through,reflected*_Reflection,reflectAmount);
                // Keep a thin pale edge, with specular glints rather than diffuse white fill.
                color += lerp(half3(0.73,0.91,1),tint,0.15)*fresnel*0.23;
                Light sun = GetMainLight();
                half spec = pow(saturate(dot(n,SafeNormalize(sun.direction+view))),lerp(110,600,_Smoothness));
                color += sun.color*spec*0.85;
                color = MixFog(color,input.fogFactor);
                half alpha = input.color.a*_BaseColor.a*(0.85+fresnel*0.15);
                return half4(color,saturate(alpha));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
