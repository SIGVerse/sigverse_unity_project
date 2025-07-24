using SIGVerse.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace SIGVerse.Common
{
	public abstract class BaseRobotSensorRenderFeature<TPass>: ScriptableRendererFeature
		where TPass : BaseRobotSensorRenderFeature<TPass>.SensorPassBase, new()
	{
		[SerializeField] protected Shader shader;
		private Material material;
		private TPass pass;

		private readonly Dictionary<Camera, RobotCameraSettings> cameraSettingsCache = new();

		public override void Create()
		{
			if (this.shader == null)
			{
				Debug.LogError("Shader is null. Cannot create material and pass for " + typeof(TPass).Name);
				return;
			}

			CreateMaterialAndPass();
		}

		public virtual void CreateMaterialAndPass()
		{
			this.material = new Material(this.shader);
			this.pass = new TPass();
			this.pass.Initialize(GetRenderPassEvent(), this.material, typeof(TPass).Name);
		}

		protected virtual RenderPassEvent GetRenderPassEvent()
		{
			return RenderPassEvent.AfterRenderingPostProcessing; // Default
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game) { return; }

			if (this.material == null || this.pass == null)
			{
				Debug.Log("Material or pass is null. Recreating material and pass for " + typeof(TPass).Name);
				CreateMaterialAndPass();
			}

			Camera cam = renderingData.cameraData.camera;

			if (!this.cameraSettingsCache.TryGetValue(cam, out RobotCameraSettings settings))
			{
				if (!cam.TryGetComponent(out settings))
				{
					Debug.LogError("RobotCameraSettings is missing on " + SIGVerseUtils.GetHierarchyPath(cam.transform));
					return;
				}

				this.cameraSettingsCache[cam] = settings;
			}

			if (!ShouldEnqueuePass(settings)) { return; }

			this.pass.Setup(settings, this.material);

			renderer.EnqueuePass(pass);
		}

		protected abstract bool ShouldEnqueuePass(RobotCameraSettings settings);

		protected override void Dispose(bool disposing)
		{
			if (Application.isPlaying)
			{
				Destroy(this.material);
			}
			else
			{
				DestroyImmediate(this.material);
			}
		}

		public interface ISensorPass
		{
			public void Setup(RobotCameraSettings settings, Material material);
		}

		/// <summary>
		/// Base class for sensor render passes in robotic sensors.
		/// </summary>
		public abstract class SensorPassBase: ScriptableRenderPass, ISensorPass
		{
			protected Material material;
			protected string blitPassName;

			public void Initialize(RenderPassEvent renderPassEvent, Material mat, string name)
			{
				base.renderPassEvent = renderPassEvent;
				this.material = mat;
				this.blitPassName = name;
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				RecordBlitPass(renderGraph, frameData, this.material, this.blitPassName);
			}

			protected virtual void RecordBlitPass(RenderGraph renderGraph, ContextContainer frameData, Material blitMat, string name)
			{
				UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

				if (resourceData.isActiveTargetBackBuffer) { return; }

				TextureHandle source      = GetSourceTexture     (renderGraph, frameData);
				TextureHandle destination = GetDestinationTexture(renderGraph, frameData);

				if (!source.IsValid() || !destination.IsValid()) { return; }

				BlitMaterialParameters passParams = new (source, destination, blitMat, 0);
				renderGraph.AddBlitPass(passParams, name);
			}

			protected virtual TextureHandle GetSourceTexture(RenderGraph renderGraph, ContextContainer frameData)
			{
				return frameData.Get<UniversalResourceData>().activeDepthTexture; // Default
			}

			protected virtual TextureHandle GetDestinationTexture(RenderGraph renderGraph, ContextContainer frameData)
			{
				return frameData.Get<UniversalResourceData>().activeColorTexture; // Default
			}

			public abstract void Setup(RobotCameraSettings settings, Material material);
		}
	}
}
