Shader "Custom/DarkFogOverlay"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _FogColor ("Fog Color", Color) = (0, 0, 0, 0.97)
        _RevealMask ("Reveal Mask", 2D) = "black" {}

        _RevealMultiplier ("Reveal Multiplier", Float) = 1
        _RevealPower ("Reveal Power", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
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
                float _RevealMultiplier;
                float _RevealPower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 maskColor =
                    SAMPLE_TEXTURE2D(
                        _RevealMask,
                        sampler_RevealMask,
                        input.uv
                    ).rgb;

                half reveal =
                    max(
                        maskColor.r,
                        max(maskColor.g, maskColor.b)
                    );

                reveal =
                    saturate(
                        reveal * _RevealMultiplier
                    );

                reveal =
                    pow(
                        reveal,
                        max(_RevealPower, 0.001)
                    );

                half4 fog = _FogColor;

                fog.a *= 1.0h - reveal;

                return fog;
            }

            ENDHLSL
        }
    }
}