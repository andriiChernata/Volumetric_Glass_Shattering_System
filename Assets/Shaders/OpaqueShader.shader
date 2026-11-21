Shader "Custom/OpaqueShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Diffuse ("Diffuse value", Range(0, 1)) = 1.0
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 1, 1, 1)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 3
        _EmissionMap("Emission Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile_fog

            struct Attributes
            {
                float4 vertex: POSITION;     
                float3 normal: NORMAL; 
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
                float2 uv : TEXCOORD0;

                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Diffuse;
                half4 _LightColor0;
                half4 _WorldSpaceLightPos0;
                half4 _EmissionColor;
                float _EmissionStrength;
                sampler2D _EmissionMap;
                float _FogFactor;
            CBUFFER_END

            v2f vert(Attributes IN)
            {
                v2f OUT;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex.xyz);
                OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
                OUT.normal = normalize(mul(IN.normal, unity_WorldToObject).xyz);;
                OUT.worldNormal = normalize(mul(IN.normal, (float3x3)unity_WorldToObject));
                OUT.uv = IN.uv;

                OUT.fogFactor = ComputeFogFactor(OUT.pos.z);

                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(GetCameraPositionWS().xyz - IN.worldPos);
                float3 worldNormal = normalize(IN.worldNormal);

                half4 NdotL = max(0., dot(worldNormal, lightDir));
				half4 dif = NdotL * _Diffuse * _LightColor0;
                
                dif.rgb += _GlossyEnvironmentColor.rgb;

                half4 emision = tex2D(_EmissionMap, IN.uv).a * _EmissionColor * _EmissionStrength;

                dif += emision;

                half4 resultColor = _BaseColor * dif;

                resultColor.rgb = MixFog(resultColor.rgb, IN.fogFactor);

                return half4(resultColor.rgb,1);
            }

            ENDHLSL
        }
    }
}
