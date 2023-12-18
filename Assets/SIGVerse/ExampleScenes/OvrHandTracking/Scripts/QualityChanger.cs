using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ExampleScenes.OvrHandTracking
{
	public class QualityChanger : MonoBehaviour
	{
		public ShadowQuality shadowQuality = ShadowQuality.All;
		public ShadowResolution shadowResolution = ShadowResolution.Medium;
		public SkinWeights skinWeights = SkinWeights.FourBones;

		void Awake()
		{
			QualitySettings.shadows = this.shadowQuality;
			QualitySettings.shadowResolution = this.shadowResolution;
			QualitySettings.skinWeights = this.skinWeights;
		}
	}
}
