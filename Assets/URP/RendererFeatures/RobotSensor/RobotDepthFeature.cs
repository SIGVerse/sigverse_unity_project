using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SIGVerse.Common
{
	public class RobotDepthFeature: BaseRobotSensorRenderFeature<RobotDepthFeature.DepthPass>
	{
		protected override RenderPassEvent GetRenderPassEvent()
		{
			return RenderPassEvent.BeforeRenderingPostProcessing;
		}

		protected override bool ShouldEnqueuePass(RobotCameraSettings settings)
		{
			return settings.sensorType == RobotCameraSettings.SensorType.Depth;
		}

		/// <summary>
		/// Render pass for generating depth sensor data for robotic sensors.
		/// </summary>
		public class DepthPass: SensorPassBase
		{
			public override void Setup(RobotCameraSettings settings, Material material)
			{
				material.SetFloat("_MinValidDepth", settings.depthMinValue);

				switch (settings.depthEncoding)
				{
					case RobotCameraSettings.DepthEncoding.U16C1:
						material.SetFloat("_IsMillimeter", 1);      //[mm]
						material.SetFloat("_DepthEncodingMode", 0); //RG
						break;
					case RobotCameraSettings.DepthEncoding.F32C1:
						material.SetFloat("_IsMillimeter", 0);      //[m]
						material.SetFloat("_DepthEncodingMode", 1); //RGBA
						break;
				}
			}
		}
	}
}
