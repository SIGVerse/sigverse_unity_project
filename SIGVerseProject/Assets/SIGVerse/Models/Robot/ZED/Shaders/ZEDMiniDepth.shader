Shader "SIGVerse/ZEDMiniDepth" 
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv:TEXCOORD0;
			};

			//Vertex Shader
			v2f vert(appdata_base v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.pos);
				o.uv.y = 1 - o.uv.y;

				return o;
			}
	
			//Fragment Shader
			float frag(v2f i) : COLOR
			{
				float depthValue = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

				float depth_m;

				if (depthValue == 1.0f || depthValue < 0.1 / _ProjectionParams.z) // 0.1 means the Near. 
				{
					depth_m = 0;
				}
				else 
				{
					depth_m = depthValue * _ProjectionParams.z;
				}
		
				return depth_m;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
