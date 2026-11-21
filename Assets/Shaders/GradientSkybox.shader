Shader "Custom/GradientSkybox"
{
    Properties
    {
        _UpColor("Up Color", Color) = (1, 1, 1, 1)
        _DownColor("Down Color", Color) = (1, 1, 1, 1)
        _Up ("Up Vector", Vector) = (0, 1, 0)
        _Offset("Offset", float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Skybox" "Queue" = "Background"}

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _UpColor;
                half4 _DownColor;
                float3 _Up;
                float _Offset;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(GetCameraPositionWS().xyz - IN.worldPos);
                float d = dot(viewDir, _Up);
                float value = d + 1 + _Offset;
                value = clamp(value, 0, 2);
                half4 color = lerp(_UpColor, _DownColor, value / 2);
                return color;
            }
            ENDHLSL
        }
    }
}
