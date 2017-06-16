// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SIGVerse/XtionDepth" 
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
			half4 frag(v2f i) : COLOR
			{
				float depthValue = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

				float depth_mm;
				if (depthValue == 1.0f) {
					depth_mm = 0;
				}
				else {
					depth_mm = depthValue * 4000 /*+ 300*/;
				}

				float upperVal = trunc(depth_mm / 256);
				float lowerVal = depth_mm - upperVal * 256;

				half4 depth;
				depth.r = (float)lowerVal / 256.0f;
				depth.g = (float)upperVal / 256.0f;
				depth.b = 0;
				depth.a = 1;
		
				return depth;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
