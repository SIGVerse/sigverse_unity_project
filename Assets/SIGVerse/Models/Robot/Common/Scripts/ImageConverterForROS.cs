using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Experimental.Rendering;

namespace SIGVerse.Common
{
	public class ImageConverterForROS : MonoBehaviour
	{
		[HeaderAttribute("Camera")]
		[TooltipAttribute("[m]"), Range(0.01f, 1.0f)]
		public float sensorNear = 0.4f;

		[HeaderAttribute("RenderTexture Parameters")]
		public bool useRenderTexture = true;
		public int width;
		public int height;
		public int depth;
		public RenderTextureFormat renderTextureFormat;
		public bool UseGraphicsFormat = false;
		public GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
		public FilterMode filterMode;
		public int anisoLevel;

		[HeaderAttribute("Shader")]
		public Shader shader;

		[HeaderAttribute("Debug")]
		public bool isDebugMode = false;

		//------------------

		private Material material;

		void Awake()
		{
			if(this.useRenderTexture)
			{
				RenderTexture renderTexture = new RenderTexture(this.width, this.height, this.depth, this.renderTextureFormat);
				if(this.UseGraphicsFormat)
				{
					renderTexture.graphicsFormat = this.graphicsFormat;
				}
				
				renderTexture.filterMode = this.filterMode;
				renderTexture.anisoLevel = this.anisoLevel;

				renderTexture.name = shader.name;
			
				renderTexture.Create();

				this.GetComponent<Camera>().targetTexture = renderTexture;
			}

			this.material = new Material(this.shader);

			this.material.SetFloat("_SensorNear", this.sensorNear);
			this.material.SetFloat("_IsDebug", Convert.ToSingle(isDebugMode));
		}

		void Start()
		{
			this.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, material);
		}
	}
}
