using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubXtionDepth : MonoBehaviour
	{
		//--------------------------------------------------

		private System.Net.Sockets.TcpClient tcpClientCameraInfo = null;
		private System.Net.Sockets.TcpClient tcpClientImage      = null;

		private System.Net.Sockets.NetworkStream networkStreamCameraInfo = null;
		private System.Net.Sockets.NetworkStream networkStreamImage      = null;

		private SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		private SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>      imageMsg      = null;

		private GameObject cameraFrameObj;

		// Xtion
		private Camera xtionDepthCamera;
		private Texture2D imageTexture;
		byte[]  byteArray; 


		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private float elapsedTime;

		private bool isPublishingCameraInfo = false;
		private bool isPublishingImage      = false;

		private bool shouldSendMessage = false;

		private bool isUsingThread;


		void Awake()
		{
			this.cameraFrameObj = this.transform.parent.gameObject;
		}

		public void Initialize(string rosBridgeIP, int sigverseBridgePort, string topicNameCameraInfo, string topicNameImage, bool isUsingThread)
		{
			if(!RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.ContainsKey(topicNameCameraInfo))
			{
				this.tcpClientCameraInfo = SIGVerseRosBridgeConnection.GetConnection(rosBridgeIP, sigverseBridgePort);

				RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.Add(topicNameCameraInfo, this.tcpClientCameraInfo);
			}
			else
			{
				this.tcpClientCameraInfo = RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap[topicNameCameraInfo];
			}
			
			if(!RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.ContainsKey(topicNameImage))
			{
				this.tcpClientImage = SIGVerseRosBridgeConnection.GetConnection(rosBridgeIP, sigverseBridgePort);

				RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.Add(topicNameImage, this.tcpClientImage);
			}
			else
			{
				this.tcpClientImage = RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap[topicNameImage];
			}

			this.networkStreamCameraInfo = this.tcpClientCameraInfo.GetStream();
			this.networkStreamCameraInfo.ReadTimeout  = 100000;
			this.networkStreamCameraInfo.WriteTimeout = 100000;

			this.networkStreamImage = this.tcpClientImage.GetStream();
			this.networkStreamImage.ReadTimeout  = 100000;
			this.networkStreamImage.WriteTimeout = 100000;


			// Depth Camera
			this.xtionDepthCamera = this.cameraFrameObj.GetComponentInChildren<Camera>();

			int imageWidth  = this.xtionDepthCamera.targetTexture.width;
			int imageHeight = this.xtionDepthCamera.targetTexture.height;

			this.byteArray = new byte[imageWidth * imageHeight * 2];

			for (int i = 0; i < this.byteArray.Length; i++)
			{
				this.byteArray[i] = 0;
			}

			this.imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);


			//  [camera/depth/CameraInfo]
			string distortionModel = "plumb_bob";

			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 554, 0.0, 320, 0.0, 554, 240, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 554, 0.0, 320, 0.0, 0.0, 554, 240, 0.0, 0.0, 0.0, 1.0, 0.0 };

			//double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			//double[] K = { 554.3827128226441, 0.0, 320.5, 0.0, 554.3827128226441, 240.5, 0.0, 0.0, 1.0 };
			//double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			//double[] P = { 554.3827128226441, 0.0, 320.5, 0.0, 0.0, 554.3827128226441, 240.5, 0.0, 0.0, 0.0, 1.0, 0.0 };

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);

			//  [camera/depth/Image_raw]
			string encoding = "16UC1";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 2;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), this.cameraFrameObj.name);

			this.cameraInfoMsg = new SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge>("publish", topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>     ("publish", topicNameImage     , ImageForSIGVerseBridge.GetMessageType(),      this.imageData);

			this.isUsingThread = isUsingThread;
		}

		//void OnDestroy()
		//{
		//	if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
		//	if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

		//	if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
		//	if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		//}

		public void SendMessageInThisFrame()
		{
			this.shouldSendMessage = true;
		}

		public bool IsConnected()
		{
			return this.tcpClientCameraInfo.Connected && this.tcpClientImage.Connected && this.networkStreamCameraInfo != null && this.networkStreamImage != null;
		}

		public bool IsPublishing()
		{
			return this.isPublishingCameraInfo || this.isPublishingImage;
		}

		public void Close()
		{
			if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
			if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

			if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
			if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		}


		//void Update()
		//{
		//}

		void OnPostRender()
		{
			if(this.shouldSendMessage)
			{
				this.shouldSendMessage = false;

				this.PubImage();
			}
		}


		private void PubImage()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			this.isPublishingCameraInfo = true;
			this.isPublishingImage      = true;

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.xtionDepthCamera.targetTexture;

			// Apply depth information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0, false);
			this.imageTexture.Apply();

			// Convert pixel values to depth buffer for ROS message
			byte[] depthBytes = this.imageTexture.GetRawTextureData();

//			yield return null;

			this.header.Update();

			//  [camera/depth/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			if(this.isUsingThread)
			{
				Thread threadCameraInfo = new Thread(new ThreadStart(SendCameraInfo));
				threadCameraInfo.Start();
			}
			else
			{
				this.SendCameraInfo();
			}

//			yield return null;

			// [camera/depth/Image_raw]
			int textureWidth = this.imageTexture.width;
			int textureHeight = this.imageTexture.height;

			for (int row = 0; row < textureHeight; row++)
			{
				for (int col = 0; col < textureWidth; col++)
				{
					int index = row * textureWidth + col;
					this.byteArray[index * 2 + 0] = depthBytes[index * 3 + 0];
					this.byteArray[index * 2 + 1] = depthBytes[index * 3 + 1];
				}
			}

//			yield return null;

			this.imageData.header = this.header;
			this.imageData.data = this.byteArray;
			this.imageMsg.msg = this.imageData;

			if(this.isUsingThread)
			{
				Thread threadImage = new Thread(new ThreadStart(SendImage));
				threadImage.Start();
			}
			else
			{
				this.SendImage();
			}

//			sw.Stop();
//			UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}


		private void SendCameraInfo()
		{
			this.cameraInfoMsg.SendMsg(this.networkStreamCameraInfo);
			this.isPublishingCameraInfo = false;
		}

		private void SendImage()
		{
			this.imageMsg.SendMsg(this.networkStreamImage);
			this.isPublishingImage = false;
		}
	}
}
