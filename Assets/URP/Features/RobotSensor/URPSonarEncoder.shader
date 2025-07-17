Shader "SIGVerse/URPSonarEncoder"
{
	Properties
	{
		_SensorNear("Sonar Near (m)", Float) = 0.02
	}

	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

		TEXTURE2D(_CameraDepthTexture);
		SAMPLER(sampler_CameraDepthTexture);

		CBUFFER_START(UnityPerMaterial)
		float _SensorNear;
		CBUFFER_END

		float2 Encode16bitToRG(float val)
		{
			uint u = (uint)val;

			return float2(
				(u >> 0) & 0xFF,
				(u >> 8) & 0xFF
			) / 255.0;
		}

		float4 SonarEncode(Varyings input) : SV_Target
		{
			float2 uv = input.texcoord;

			float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
			float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

			// Compute vertical FOV from projection matrix
			float fov = 2.0 * atan(1.0f / UNITY_MATRIX_P._m11);

			// Compute distance from screen center
			float2 uvCenterOffset = abs(uv - float2(0.5, 0.5)) / 0.5;
			float distanceFromCenter = length(uvCenterOffset);
			distanceFromCenter = saturate(distanceFromCenter);

			// Correct depth based on angle from center
			float correction = sqrt(1.0 + pow(distanceFromCenter * tan(fov * 0.5), 2));
			float correctedDepth = linearDepth * correction;

			float far = _ProjectionParams.z;
			float sonar_mm;

			if (correctedDepth >= far)
			{
				sonar_mm = far * 1000.0;
			}
			else if (correctedDepth <= _SensorNear)
			{
				sonar_mm = _SensorNear * 1000.0;
			}
			else
			{
				sonar_mm = correctedDepth * 1000.0;
			}

			float2 encoded = Encode16bitToRG(sonar_mm);
			return float4(encoded, 0.0, 1.0);
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }
		ZWrite Off
		Cull Off

		Pass
		{
			Name "SonarEncode"

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment SonarEncode
			ENDHLSL
		}
	}
}
