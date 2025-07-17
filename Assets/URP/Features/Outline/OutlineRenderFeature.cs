using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SIGVerse.Xenon
{
	public class OutlineRenderFeature : ScriptableRendererFeature
	{
		private readonly Dictionary<Camera, bool> cameraOutlineCache = new();

		[Serializable]
		public class Settings
		{
			public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;

			public LayerMask LayerMask = 0;

			public RenderingLayerMask RenderingLayerMask = 0;

			public Material OverrideMaterial;

			public Material BlitMaterial;

			public bool ClearDepth;
		}

		[Serializable]
		public class OutlineSettings
		{
			public float OutlineScale = 1f;
			public float RobertsCrossMultiplier = 100;
			public float DepthThreshold = 10f;
			public float NormalThreshold = 0.4f;
			public float SteepAngleThreshold = 0.2f;
			public float SteepAngleMultiplier = 25f;
			public Color OutlineColor = Color.white;
		}

		public class OutlineData : ContextItem
		{
			public TextureHandle FilterTextureHandle;

			public override void Reset()
			{
				FilterTextureHandle = TextureHandle.nullHandle;
			}
		}

		public Settings FeatureSettings;
		public OutlineSettings MaterialSettings;

		private OutlineObjectMaskPass _outlinePassFilter;
		private OutlineRenderPass _outlinePassFinal;

		public override void Create()
		{
			_outlinePassFilter = new OutlineObjectMaskPass(FeatureSettings);
			_outlinePassFinal = new OutlineRenderPass(FeatureSettings, MaterialSettings);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

			Camera cam = renderingData.cameraData.camera;

			if (!this.cameraOutlineCache.TryGetValue(cam, out bool hasMarker))
			{
				hasMarker = cam.TryGetComponent<EnableSimpleOutlineRendering>(out _);

				this.cameraOutlineCache[cam] = hasMarker;
			}

			if (!hasMarker) { return; }

			renderer.EnqueuePass(_outlinePassFilter);
			renderer.EnqueuePass(_outlinePassFinal);
		}
	}
}
