using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;

namespace SIGVerse.ZEDMini
{
	public class ZEDMiniPubRGB : RobotPubCameraImage
	{
		public static readonly double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
		public static readonly double[] K = { 335.67, 0.0, 333.32, 0.0, 335.67, 192.03, 0.0, 0.0, 1.0 };
		public static readonly double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
		public static readonly double[] P = { 335.67, 0.0, 333.32, 0.0, 0.0, 335.67, 192.03, 0.0, 0.0, 0.0, 1.0, 0.0 };

		// D: [0.0, 0.0, 0.0, 0.0, 0.0]
		// K: [335.6714782714844, 0.0, 333.3177490234375, 0.0, 335.6714782714844, 192.02883911132812, 0.0, 0.0, 1.0]
		// R: [1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0]
		// P: [335.6714782714844, 0.0, 333.3177490234375, 0.0, 0.0, 335.6714782714844, 192.02883911132812, 0.0, 0.0, 0.0, 1.0, 0.0]

		protected override void Awake()
		{
			base.Awake();
//			this.textureFormat = TextureFormat.BGRA32;
		}

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
			string encoding = "bgra8"; //"rgb8"
			byte isBigendian = 0;
			uint step = imageWidth * 4; // *3

			return new ImageForSIGVerseBridge(null, imageHeight, imageWidth, encoding, isBigendian, step, null);
		}
	}
}
