Shader "SIGVerse/URPDepthEncoder"
{
	Properties
	{
		_MinValidDepth("Min Valid Depth (m)", Float) = 0.4
		_IsMillimeter("Is Millimeter Unit (0: m, 1: mm)", Float) = 1
		_DepthEncodingMode("Depth Encoding Mode (0: RG, 1: RGBA)", Float) = 0
	}

	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

		TEXTURE2D(_CameraDepthTexture);
		SAMPLER(sampler_CameraDepthTexture);

		CBUFFER_START(UnityPerMaterial)
		float _MinValidDepth;
		float _IsMillimeter;
		float _DepthEncodingMode;
		CBUFFER_END

		float2 DepthToRGEncoded(float v)
		{
			uint u = (uint)v; // For 16UC1

			return float2(
				(u >> 0) & 0xFF,
				(u >> 8) & 0xFF
			) / 255.0;
		}

		float4 DepthToRGBAEncoded(float v)
		{
			uint u = asuint(v); // For 32FC1

			return float4(
				(u >> 0)  & 0xFF,
				(u >> 8)  & 0xFF,
				(u >> 16) & 0xFF,
				(u >> 24) & 0xFF
			) / 255.0;
		}

		float4 DepthToRGBA(Varyings input) : SV_Target
		{
			// Flip UV vertically
			float2 flippedUV = float2(input.texcoord.x, 1.0 - input.texcoord.y);

			float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, flippedUV).r;
			float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
			float far = _ProjectionParams.z;

			float adjustedDepth;

			if (linearDepth <= _MinValidDepth || linearDepth >= far)
			{
				adjustedDepth = 0.0;
			}
			else
			{
				if (_IsMillimeter >= 0.5)
				{
					adjustedDepth = linearDepth * 1000.0;
				}
				else
				{
					adjustedDepth = linearDepth;
				}
			}

			if (_DepthEncodingMode >= 0.5)
			{
				return DepthToRGBAEncoded(adjustedDepth);
			}
			else
			{
				float2 rg = DepthToRGEncoded(adjustedDepth);
				return float4(rg, 0.0, 1.0);
			}
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }
		ZWrite Off
		Cull Off

		Pass
		{
			Name "DepthToRGBA"

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment DepthToRGBA
			ENDHLSL
		}
	}
}
