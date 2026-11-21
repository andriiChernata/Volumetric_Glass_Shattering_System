
Shader "Custom/GaussianBlur"
{
    Properties
    {
        _BlurSize ("Blur Size", Range(0, 1)) = 1
        
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Diffuse ("Diffuse value", Range(0, 1)) = 1.0

        [Header(Specular)]
		_SpecColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Range(0.1, 10)) = 1
        _SpecLightDir("Light Dir", Vector) = (0,1,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

        
        CBUFFER_START(UnityPerMaterial)
            float _BlurSize;
            half4 _BaseColor;
            float _Diffuse;
            half4 _LightColor0;
            half4 _WorldSpaceLightPos0;
            half3 _SpecLightDir;
            float _FogFactor;
            half4 _SpecColor;
		    float _Shininess;
        CBUFFER_END

        struct Attributes
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normal : NORMAL;
        };

        struct Varyings
        {
             float4 pos : SV_POSITION;
             float3 normal : NORMAL;
             float3 worldPos : TEXCOORD1;
			 float3 worldNormal : TEXCOORD2;
             float2 uv : TEXCOORD0;

             float fogFactor : TEXCOORD3;
        };

        half3 SampleBlur(float2 direction, float4 pos)
        {
            float2 offset = direction * _BlurSize;

            float2 dirX = direction;
            dirX.y = 0;

            float2 dirY = direction;
            dirY.x = 0;

            float2 screenUV = GetNormalizedScreenSpaceUV(pos);

            // Gaussian weights
            half3 color = SampleSceneColor(screenUV) * 0.25;

            color += SampleSceneColor(screenUV + float2(offset.x, 0) * 1.384615) * 0.125;

            color += SampleSceneColor(screenUV - float2(offset.x, 0) * 1.384615) * 0.125;

            color += SampleSceneColor(screenUV + float2(0, offset.y) * 3.230769) * 0.125;

            color += SampleSceneColor(screenUV - float2(0, offset.y) * 3.230769) * 0.125;

            color += SampleSceneColor(screenUV + offset * 1.384615) * 0.0625;
            
            color += SampleSceneColor(screenUV - offset * 1.384615) * 0.0625;
            
            color += SampleSceneColor(screenUV + float2(offset.x, -offset.y) * 1.384615) * 0.0625;
            
            color += SampleSceneColor(screenUV + float2(-offset.x, offset.y) * 1.384615) * 0.0625;

            return color;
        }

        half4 FragResult(Varyings IN)
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(GetCameraPositionWS().xyz - IN.worldPos);
                float3 worldNormal = normalize(IN.worldNormal);

                float NdotL = max(0., dot(worldNormal, lightDir));
				half3 dif = NdotL * _Diffuse * _LightColor0.rgb;
                
                dif += _GlossyEnvironmentColor.rgb;

                lightDir = normalize(_SpecLightDir);
                NdotL = max(0., dot(worldNormal, lightDir));

                float3 refl = reflect(-lightDir, worldNormal);
			    float RdotV = max(0.0, dot(refl, viewDir));

                half3 spec = NdotL * _SpecColor.rgb * pow(RdotV, _Shininess);

                dif += spec;

                half4 resultColor = _BaseColor;

                resultColor.rgb *= dif;

                resultColor.rgb = MixFog(resultColor.rgb, IN.fogFactor);

                return resultColor;
            }

        ENDHLSL

        Pass
        {

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes IN)
            {
            Varyings OUT;

                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex.xyz);
                OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
                OUT.normal = normalize(mul(IN.normal, unity_WorldToObject).xyz);;
                OUT.worldNormal = normalize(mul(IN.normal, (float3x3)unity_WorldToObject));
                OUT.uv = IN.uv;

                OUT.fogFactor = ComputeFogFactor(OUT.pos.z);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 color = SampleBlur(float2(1, 1), IN.pos) * FragResult(IN);
                half4 result = _BaseColor;
                result.rgb = color.rgb;
                return result;
            }

            ENDHLSL
        }
    }
}
