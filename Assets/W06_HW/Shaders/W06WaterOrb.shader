Shader "W06/Water Orb"
{
    Properties
    {
        _BaseColor ("Water tint", Color) = (0.12,0.65,0.83,1)
        _RimColor ("Water edge", Color) = (0.39,0.94,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.94
        _Opacity ("Water tint opacity", Range(0,1)) = 0.38
        _Refraction ("Refraction", Range(0,0.1)) = 0.035
        _Reflection ("Environment reflection", Range(0,2)) = 0.70
        _IdleWobble ("Idle surface movement", Range(0,0.12)) = 0.035
        _ImpactTime ("Last hit time", Float) = -100
        _ImpactPoint ("Hit direction in object space", Vector) = (0,0,-1,0)
        _ImpactStrength ("Hit movement", Range(0,2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "WaterSurface"
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
                half4 _BaseColor, _RimColor;
                float4 _ImpactPoint;
                half _Smoothness, _Opacity, _Refraction, _Reflection, _IdleWobble;
                float _ImpactTime, _ImpactStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            // Two long waves change the silhouette; the hit adds a traveling spherical wave.
            float SurfaceOffset(float3 n)
            {
                float t = _Time.y;
                float idle = (sin(n.x*4.6+n.y*2.2+t*1.85)*0.56
                            + sin(n.z*5.3-n.y*3.8-t*1.45)*0.44) * _IdleWobble;
                float age = max(t-_ImpactTime,0);
                float active = step(_ImpactTime,t) * exp(-age*1.16) * _ImpactStrength;
                float arc = acos(clamp(dot(n,normalize(_ImpactPoint.xyz+float3(0,0,0.00001))),-0.9999,0.9999));
                float traveling = sin(arc*12.0-age*13.5) * exp(-pow(arc-age*1.9,2)*1.2);
                float wobble = sin(age*15.5+n.y*3.6+n.x*2.2) * 0.58;
                return idle + active*(traveling+wobble)*0.105;
            }

            float3 SurfaceNormal(float3 n)
            {
                float3 refAxis = abs(n.y)<0.95 ? float3(0,1,0) : float3(1,0,0);
                float3 tangent = normalize(cross(refAxis,n));
                float3 bitangent = cross(n,tangent);
                float eps = 0.012;
                float center = SurfaceOffset(n);
                float dx = (SurfaceOffset(normalize(n+tangent*eps))-center)/eps;
                float dy = (SurfaceOffset(normalize(n+bitangent*eps))-center)/eps;
                return normalize(n-tangent*dx-bitangent*dy);
            }
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 radial = normalize(input.positionOS.xyz);
                float3 displaced = input.positionOS.xyz * (1+SurfaceOffset(radial));
                VertexPositionInputs p = GetVertexPositionInputs(displaced);
                output.positionCS = p.positionCS;
                output.positionWS = p.positionWS;
                output.normalOS = radial;
                output.fogFactor = ComputeFogFactor(p.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 radial = normalize(input.normalOS);
                float3 objectN = SurfaceNormal(radial);
                // Tiny moving ripples break up the mirror without obscuring the large waves.
                float t = _Time.y;
                objectN = normalize(objectN+float3(sin(radial.y*27+t*2.3),sin(radial.z*25-t*2.0),sin(radial.x*24+t*1.7))*0.018);
                half3 n = TransformObjectToWorldNormal(objectN);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(n,view));
                half fresnel = pow(1-facing,3.2);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                half3 normalVS = mul((half3x3)UNITY_MATRIX_V,n);
                half3 through = SampleSceneColor(saturate(screenUV-normalVS.xy*_Refraction*(0.4+facing)));
                half3 reflected = GlossyEnvironmentReflection(reflect(-view,n),input.positionWS,1-_Smoothness,1,screenUV);
                half depthTint = saturate(_Opacity*(0.6+facing*0.5));
                half3 color = lerp(through,through*_BaseColor.rgb + _BaseColor.rgb*0.11,depthTint);
                color = lerp(color,reflected*_Reflection + _RimColor.rgb*0.12,0.1+fresnel*0.65);
                Light sun = GetMainLight();
                half spec = pow(saturate(dot(n,SafeNormalize(sun.direction+view))),lerp(90,430,_Smoothness));
                color += sun.color*spec*2.2 + _RimColor.rgb*pow(1-facing,6)*0.30;
                // Thin crests brighten briefly as the impact wave travels over the sphere.
                float age = max(_Time.y-_ImpactTime,0);
                float arc = acos(clamp(dot(radial,normalize(_ImpactPoint.xyz+float3(0,0,0.00001))),-0.9999,0.9999));
                float crest = pow(saturate(sin(arc*12-age*13.5)),10)*exp(-pow(arc-age*1.9,2)*1.5)*exp(-age*1.3)*_ImpactStrength;
                color += _RimColor.rgb*crest*0.17;
                color = MixFog(color,input.fogFactor);
                return half4(color,saturate(0.82+_Opacity*0.13+fresnel*0.08));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
