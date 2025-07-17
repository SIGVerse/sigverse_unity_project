using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;

namespace SIGVerse.TIAGo
{
	public class TIAGoPubXtionDepth : RobotPubCameraImage
	{
		protected byte[]  imageByteArray;

		protected override CameraInfoForSIGVerseBridge InitializeCameraInfo(uint imageHeight, uint imageWidth)
		{
			string distortionModel = "plumb_bob";

			uint binningX = 0;
			uint binningY = 0;

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			return new CameraInfoForSIGVerseBridge(null, imageHeight, imageWidth, distortionModel, TIAGoPubXtionRGB.D, TIAGoPubXtionRGB.K, TIAGoPubXtionRGB.R, TIAGoPubXtionRGB.P, binningX, binningY, roi);
		}

		protected override ImageForSIGVerseBridge InitializeImage(uint imageHeight, uint imageWidth)
		{
			// Initialize byte array for image data
			this.imageByteArray = new byte[imageWidth * imageHeight * 2];

			for (int i = 0; i < this.imageByteArray.Length; i++)
			{
				this.imageByteArray[i] = 0;
			}

			string encoding = "16UC1";
			byte isBigendian = 0;
			uint step = imageWidth * 2;

			return new ImageForSIGVerseBridge(null, imageHeight, imageWidth, encoding, isBigendian, step, null);
		}

		protected override void DebugPrintForDepth()
		{
			int center = this.imageTexture.width * this.imageTexture.height / 2 - this.imageTexture.width / 2;

			uint depth =
				((uint)this.imageData.data[center * 2 + 1]) << 8 |
				((uint)this.imageData.data[center * 2 + 0]);

			Debug.LogWarning("depth = " + BitConverter.ToUInt16(BitConverter.GetBytes(depth), 0) + "[mm]");
		}

		//protected override byte[] GetImageData(Texture2D imageTexture)
		//{
		//	byte[] rawTextureData = this.imageTexture.GetRawTextureData();

		//	int textureWidth  = this.imageTexture.width;
		//	int textureHeight = this.imageTexture.height;

		//	for (int row = 0; row < textureHeight; row++)
		//	{
		//		for (int col = 0; col < textureWidth; col++)
		//		{
		//			int index = row * textureWidth + col;

		//			this.imageByteArray[index * 2 + 0] = rawTextureData[index * 3 + 0];
		//			this.imageByteArray[index * 2 + 1] = rawTextureData[index * 3 + 1];
		//		}
		//	}

		//	return this.imageByteArray;
		//}
	}
}
