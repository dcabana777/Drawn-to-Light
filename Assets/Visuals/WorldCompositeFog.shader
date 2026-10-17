Shader "UI/WorldCompositeFog"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("UI Texture", 2D) = "white" {}

        _DarkTex ("Dark World", 2D) = "black" {}
        _LightTex ("Light World", 2D) = "black" {}
        _ExistenceMaskTex ("Existence Mask", 2D) = "black" {}
        _FogMaskTex ("Fog Reveal Mask", 2D) = "black" {}

        _MaskBlackPoint ("Existence Mask Black Point", Range(0, 1)) = 0
        _MaskWhitePoint ("Existence Mask White Point", Range(0, 1)) = 1
        _MaskGamma ("Existence Mask Gamma", Range(0.1, 4)) = 1

        _DarkThreshold ("Dark World Threshold", Range(0, 1)) = 0.4
        _LightThreshold ("Light World Threshold", Range(0, 1)) = 0.6

        _FogColor ("Fog Color", Color) = (0, 0, 0, 0.97)
        _FogRevealMultiplier ("Fog Reveal Multiplier", Float) = 1
        _FogRevealPower ("Fog Reveal Power", Float) = 1

        [Toggle]
        _FlipY ("Flip Y", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend One Zero

        Pass
        {
            Name "WorldCompositeFog"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

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

            TEXTURE2D(_DarkTex);
            SAMPLER(sampler_DarkTex);

            TEXTURE2D(_LightTex);
            SAMPLER(sampler_LightTex);

            TEXTURE2D(_ExistenceMaskTex);
            SAMPLER(sampler_ExistenceMaskTex);

            TEXTURE2D(_FogMaskTex);
            SAMPLER(sampler_FogMaskTex);

            CBUFFER_START(UnityPerMaterial)
                float _MaskBlackPoint;
                float _MaskWhitePoint;
                float _MaskGamma;

                float _DarkThreshold;
                float _LightThreshold;

                half4 _FogColor;
                float _FogRevealMultiplier;
                float _FogRevealPower;

                float _FlipY;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half ReadMask(
                TEXTURE2D_PARAM(textureToRead, samplerToUse),
                float2 uv)
            {
                half3 maskColor =
                    SAMPLE_TEXTURE2D(
                        textureToRead,
                        samplerToUse,
                        uv
                    ).rgb;

                return max(
                    maskColor.r,
                    max(maskColor.g, maskColor.b)
                );
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                if (_FlipY > 0.5)
                {
                    uv.y = 1.0 - uv.y;
                }

                // ----------------------------------------------------
                // EXISTENCE / WORLD BLEND
                // ----------------------------------------------------

                half rawExistence =
                    ReadMask(
                        TEXTURE2D_ARGS(
                            _ExistenceMaskTex,
                            sampler_ExistenceMaskTex
                        ),
                        uv
                    );

                half maskRange =
                    max(
                        _MaskWhitePoint - _MaskBlackPoint,
                        0.0001
                    );

                half existence =
                    saturate(
                        (rawExistence - _MaskBlackPoint)
                        / maskRange
                    );

                existence =
                    pow(
                        existence,
                        max(_MaskGamma, 0.0001)
                    );

                half lightThreshold =
                    max(
                        _LightThreshold,
                        _DarkThreshold + 0.0001
                    );

                // Smooth transition only. No seam distortion,
                // instability, noise, or edge effects.
                half worldBlend =
                    smoothstep(
                        _DarkThreshold,
                        lightThreshold,
                        existence
                    );

                half4 darkColor =
                    SAMPLE_TEXTURE2D(
                        _DarkTex,
                        sampler_DarkTex,
                        uv
                    );

                half4 lightColor =
                    SAMPLE_TEXTURE2D(
                        _LightTex,
                        sampler_LightTex,
                        uv
                    );

                half4 worldColor =
                    lerp(
                        darkColor,
                        lightColor,
                        worldBlend
                    );

                // ----------------------------------------------------
                // DARK FOG
                // ----------------------------------------------------

                half rawFogReveal =
                    ReadMask(
                        TEXTURE2D_ARGS(
                            _FogMaskTex,
                            sampler_FogMaskTex
                        ),
                        uv
                    );

                half fogReveal =
                    saturate(
                        rawFogReveal
                        * _FogRevealMultiplier
                    );

                fogReveal =
                    pow(
                        fogReveal,
                        max(_FogRevealPower, 0.001)
                    );

                half fogAmount =
                    saturate(
                        (1.0h - fogReveal)
                        * _FogColor.a
                    );

                worldColor.rgb =
                    lerp(
                        worldColor.rgb,
                        _FogColor.rgb,
                        fogAmount
                    );

                // The composite itself is the final opaque game image.
                worldColor.a = 1.0h;

                return worldColor;
            }

            ENDHLSL
        }
    }
}
