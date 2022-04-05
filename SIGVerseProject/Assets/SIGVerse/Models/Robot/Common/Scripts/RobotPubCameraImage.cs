using UnityEngine;

using System;
using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.Common
{
	[RequireComponent(typeof (Camera))]
	public class RobotPubCameraImage : MonoBehaviour
	{
		public bool debugPrint = false;

		protected System.Net.Sockets.TcpClient tcpClientCameraInfo = null;
		protected System.Net.Sockets.TcpClient tcpClientImage      = null;

		protected System.Net.Sockets.NetworkStream networkStreamCameraInfo = null;
		protected System.Net.Sockets.NetworkStream networkStreamImage      = null;

		protected SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		protected SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>      imageMsg      = null;

		protected GameObject cameraFrameObj;

		public TextureFormat textureFormat = TextureFormat.RGB24;
		
		// Camera
		protected Camera targetCamera;
		protected Texture2D imageTexture;

		// ROS message header
		protected Header header;

		protected CameraInfoForSIGVerseBridge cameraInfoData;
		protected ImageForSIGVerseBridge imageData;

		protected bool isPublishingCameraInfo = false;
		protected bool isPublishingImage      = false;

		protected bool shouldSendMessage = false;

		protected bool isUsingThread;


		protected virtual void Awake()
		{
			this.cameraFrameObj = this.transform.parent.gameObject;
		}

		public virtual void Initialize(string rosBridgeIP, int sigverseBridgePort, string topicNameCameraInfo, string topicNameImage, bool isUsingThread)
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


			// Camera
			this.targetCamera = this.GetComponent<Camera>();

			int imageWidth  = this.targetCamera.targetTexture.width;
			int imageHeight = this.targetCamera.targetTexture.height;

			this.imageTexture = new Texture2D(imageWidth, imageHeight, this.textureFormat, false);


			//  [CameraInfo]
			this.cameraInfoData = this.InitializeCameraInfo((uint)imageHeight, (uint)imageWidth);
			
			//  [Image_raw]
			this.imageData = this.InitializeImage((uint)imageHeight, (uint)imageWidth);


			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), this.cameraFrameObj.name);

			this.cameraInfoMsg = new SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge>("publish", topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>     ("publish", topicNameImage,      ImageForSIGVerseBridge.GetMessageType(),      this.imageData);

			this.isUsingThread = isUsingThread;
		}

		protected virtual CameraInfoForSIGVerseBridge InitializeCameraInfo(uint imageHeight, uint imageWidth)
		{
			//
			// Example (Xtion RGB parameters)
			//
			string distortionModel = "plumb_bob";

			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 554, 0.0, 320, 0.0, 554, 240, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 554, 0.0, 320, 0.0, 0.0, 554, 240, 0.0, 0.0, 0.0, 1.0, 0.0 };

			uint binningX = 0;
			uint binningY = 0;

			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			return new CameraInfoForSIGVerseBridge(null, imageHeight, imageWidth, distortionModel, D, K, R, P, binningX, binningY, roi);
		}

		protected virtual ImageForSIGVerseBridge InitializeImage(uint imageHeight, uint imageWidth)
		{
			//
			// Example (Xtion RGB parameters)
			//
			string encoding = "rgb8";
			byte isBigendian = 0;
			uint step = imageWidth * 3;

			return new ImageForSIGVerseBridge(null, imageHeight, imageWidth, encoding, isBigendian, step, null);
		}


		//protected virtual void OnDestroy()
		//{
		//	if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
		//	if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

		//	if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
		//	if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		//}

		public virtual void SendMessageInThisFrame()
		{
			this.shouldSendMessage = true;
		}

		public virtual bool IsConnected()
		{
			return this.tcpClientCameraInfo.Connected && this.tcpClientImage.Connected && this.networkStreamCameraInfo != null && this.networkStreamImage !=null;
		}

		public virtual bool IsPublishing()
		{
			return this.isPublishingCameraInfo || this.isPublishingImage;
		}
		
		public virtual void Close()
		{
			if (this.networkStreamCameraInfo != null) { this.networkStreamCameraInfo.Close(); }
			if (this.networkStreamImage      != null) { this.networkStreamImage     .Close(); }

			if (this.tcpClientCameraInfo != null) { this.tcpClientCameraInfo.Close(); }
			if (this.tcpClientImage      != null) { this.tcpClientImage     .Close(); }
		}


		//protected virtual void Update()
		//{
		//}

		protected virtual void OnPostRender()
		{
			if(this.shouldSendMessage)
			{
				this.shouldSendMessage = false;

				this.PubImage();
			}
		}


		protected virtual void PubImage()
		{
			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();

			this.isPublishingCameraInfo = true;
			this.isPublishingImage      = true;

			this.header.Update();

			//  [CameraInfo]
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

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.targetCamera.targetTexture;

			// Read image
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0, false);
			this.imageTexture.Apply();

			//  [Image_raw]
			this.imageData.header = this.header;
			this.imageData.data = this.GetImageData(this.imageTexture);
			this.imageMsg.msg = this.imageData;

			// Debug Print Center Depth
			int center = this.imageTexture.width * this.imageTexture.height / 2 - this.imageTexture.width / 2;

			if (this.debugPrint)
			{
				uint depth =
					((uint)this.imageData.data[center * 4 + 3]) << 24 |
					((uint)this.imageData.data[center * 4 + 2]) << 16 |
					((uint)this.imageData.data[center * 4 + 1]) << 8 |
					((uint)this.imageData.data[center * 4 + 0]);

				Debug.LogWarning("depth = " + BitConverter.ToSingle(BitConverter.GetBytes(depth), 0) + "[m]");

				this.debugPrint = false;
			}

			if (this.isUsingThread)
			{
				Thread threadImage = new Thread(new ThreadStart(SendImage));
				threadImage.Start();
			}
			else
			{
				this.SendImage();
			}

			//sw.Stop();
			//UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}

		protected virtual byte[] GetImageData(Texture2D imageTexture)
		{
			return imageTexture.GetRawTextureData();
		}

		protected virtual void SendCameraInfo()
		{
			this.cameraInfoMsg.SendMsg(this.networkStreamCameraInfo);
			this.isPublishingCameraInfo = false;
		}

		protected virtual void SendImage()
		{
			this.imageMsg.SendMsg(this.networkStreamImage);
			this.isPublishingImage = false;
		}
	}
}
