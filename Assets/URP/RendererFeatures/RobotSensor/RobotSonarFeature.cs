using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SIGVerse.Common
{
	public class RobotSonarFeature: BaseRobotSensorRenderFeature<RobotSonarFeature.SonarPass>
	{
		protected override RenderPassEvent GetRenderPassEvent()
		{
			return RenderPassEvent.BeforeRenderingPostProcessing;
		}

		protected override bool ShouldEnqueuePass(RobotCameraSettings settings)
		{
			return settings.sensorType == RobotCameraSettings.SensorType.Sonar;
		}

		/// <summary>
		/// Render pass for generating sonar sensor data for robotic sensors.
		/// </summary>
		public class SonarPass: SensorPassBase
		{
			public override void Setup(RobotCameraSettings settings, Material material)
			{
				material.SetFloat("_SensorNear", settings.sonarMinValue);
			}
		}
	}
}
