using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;

namespace SIGVerse.ZEDMini
{
	public class ZEDMiniPubDepth : RobotPubCameraImage
	{
		//protected byte[]  imageByteArray;

		protected override void Awake()
		{
			base.Awake();
//			this.textureFormat = TextureFormat.RFloat;
		}

		protected override CameraInfoForSIGVerseBridge InitializeCameraInfo(uint imageHeight, uint imageWidth)
		{
			string distortionModel = "plumb_bob";

			uint binningX = 0;
			uint binningY = 0;

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			return new CameraInfoForSIGVerseBridge(null, imageHeight, imageWidth, distortionModel, ZEDMiniPubRGB.D, ZEDMiniPubRGB.K, ZEDMiniPubRGB.R, ZEDMiniPubRGB.P, binningX, binningY, roi);
		}

		protected override ImageForSIGVerseBridge InitializeImage(uint imageHeight, uint imageWidth)
		{
			string encoding = "32FC1";
			byte isBigendian = 0;
			uint step = imageWidth * 4;

			return new ImageForSIGVerseBridge(null, imageHeight, imageWidth, encoding, isBigendian, step, null);
		}
	}
}
