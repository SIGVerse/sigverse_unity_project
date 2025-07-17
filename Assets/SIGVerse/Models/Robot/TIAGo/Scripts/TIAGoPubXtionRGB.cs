using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;

namespace SIGVerse.TIAGo
{
	public class TIAGoPubXtionRGB : RobotPubCameraImage
	{
		public static readonly double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
		public static readonly double[] K = { 522.2, 0.0, 320.5, 0.0, 522.2, 240.5, 0.0, 0.0, 1.0 };
		public static readonly double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
		public static readonly double[] P = { 522.2, 0.0, 320.5, 0.0, 0.0, 522.2, 240.5, 0.0, 0.0, 0.0, 1.0, 0.0 };

		protected override CameraInfoForSIGVerseBridge InitializeCameraInfo(uint imageHeight, uint imageWidth)
		{
			string distortionModel = "plumb_bob";

			uint binningX = 0;
			uint binningY = 0;

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			return new CameraInfoForSIGVerseBridge(null, imageHeight, imageWidth, distortionModel, D, K, R, P, binningX, binningY, roi);
		}

		protected override ImageForSIGVerseBridge InitializeImage(uint imageHeight, uint imageWidth)
		{
			string encoding = "rgb8";
			byte isBigendian = 0;
			uint step = imageWidth * 3;

			return new ImageForSIGVerseBridge(null, imageHeight, imageWidth, encoding, isBigendian, step, null);
		}

		protected override void DebugPrintForDepth()
		{
			Debug.LogWarning("Not Depth");
		}
	}
}
