Shader "Custom/DarkFogOverlay"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0, 0, 0, 0.95)
        _RevealMask ("Reveal Mask", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_RevealMask);
            SAMPLER(sampler_RevealMask);

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half reveal =
                    SAMPLE_TEXTURE2D(
                        _RevealMask,
                        sampler_RevealMask,
                        input.uv
                    ).r;

                reveal = saturate(reveal);

                half4 fog = _FogColor;

                fog.a *= 1.0h - reveal;

                return fog;
            }

            ENDHLSL
        }
    }
}