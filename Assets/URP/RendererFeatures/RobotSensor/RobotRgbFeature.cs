using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace SIGVerse.Common
{
	public class RobotRgbFeature: BaseRobotSensorRenderFeature<RobotRgbFeature.RgbPass>
	{
		protected override bool ShouldEnqueuePass(RobotCameraSettings settings)
		{
			return settings.sensorType == RobotCameraSettings.SensorType.RGB;
		}

		/// <summary>
		/// Render pass for generating RGB camera output for robotic sensors.
		/// </summary>
		public class RgbPass: SensorPassBase
		{
			protected override void RecordBlitPass(RenderGraph renderGraph, ContextContainer frameData, Material blitMat, string name)
			{
				// Source: https://docs.unity3d.com/Manual/urp/render-graph-optimize.html

				UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

				if (resourceData.isActiveTargetBackBuffer) { return; }

				TextureHandle source = resourceData.activeColorTexture;

				// Create a destination texture, with the same dimensions as the source
				TextureDesc destinationDescriptor = renderGraph.GetTextureDesc(source);
				destinationDescriptor.name = "DestinationTexture";
				destinationDescriptor.clearBuffer = false;
				TextureHandle destination = renderGraph.CreateTexture(destinationDescriptor);

				if (!source.IsValid() || !destination.IsValid()) { return; }

				BlitMaterialParameters passParams = new (source, destination, blitMat, 0);
				renderGraph.AddBlitPass(passParams, name);

				// Set the main color texture for the camera as the destination texture
				resourceData.cameraColor = destination;
			}

			public override void Setup(RobotCameraSettings settings, Material material)
			{
				// No specific settings
			}
		}
	}
}
