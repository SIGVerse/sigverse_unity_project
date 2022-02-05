using SIGVerse.Common;
using UnityEngine;

namespace SIGVerse.ExampleScenes.OvrHandTracking
{
	public class ConversionAngleFileCreator : MonoBehaviour
	{
		public Transform avatar;
		public Transform ovrLeftHand;
		public Transform ovrRightHand;

		public string fileName = "OvrHandConversionAngleForVRoid";

		//-----------

		void Start()
		{
			OvrHandTrackingUtils.CreateConversionAngleFile(this.avatar.GetComponent<Animator>(), this.ovrLeftHand, this.ovrRightHand, this.fileName);
		}
	}
}

