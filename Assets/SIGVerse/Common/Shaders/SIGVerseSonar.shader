// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SIGVerse/SIGVerseSonar" 
{
	Properties 
	{
		_SensorNear ("Sonar Near", Range(0.01, 1)) = 0.02
		[MaterialToggle]_IsDebug ("Is Debug", Float) = 0.0
//		[Toggle(IS_DEBUG)]_IsDebug ("Is Debug", Float) = 0.0 // Could not control from C# script
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
//			#pragma shader_feature IS_DEBUG

			#include "UnityCG.cginc"

//			sampler2D _CameraDepthTexture;
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			uniform float _SensorNear;
			uniform float _IsDebug;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			//Vertex Shader
			v2f vert(appdata_img v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}
	
			//Fragment Shader
			fixed4 frag(v2f i) : SV_TARGET
			{
				float fov = 2.0 * atan(1.0f / unity_CameraProjection._m11 );
	 
				float sonar01 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

				float distanceFromCenterX = abs(i.uv.x-0.5)/0.5;
				float distanceFromCenterY = abs(i.uv.y-0.5)/0.5;
				float distanceFromCenter = sqrt(pow(distanceFromCenterX,2)+pow(distanceFromCenterY,2));

				if(distanceFromCenter > 1.0)
				{
					sonar01 = 1.0;
				}
				else
				{
					sonar01 *= sqrt(1+pow(distanceFromCenter*tan(fov/2),2));
				}

				float sonar_mm;

				if (sonar01 >= 1.0f)
				{
					sonar_mm = _ProjectionParams.z * 1000;
				}
				else if(sonar01 < _SensorNear/_ProjectionParams.z)
				{
					sonar_mm = _SensorNear * 1000;
				}
				else 
				{
					sonar_mm = sonar01 * _ProjectionParams.z * 1000;
				}

				if(_IsDebug != 0)
				{
					sonar01 = sonar_mm / _ProjectionParams.z / 1000;
					return fixed4(sonar01, sonar01, sonar01, 1);
				}
				else
				{
					float upperVal = trunc(sonar_mm / 256);
					float lowerVal = sonar_mm - upperVal * 256;

					fixed4 depth;
					depth.r = lowerVal / 256.0f;
					depth.g = upperVal / 256.0f;
					depth.b = 0;
					depth.a = 1;
		
					return depth;
				}
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
