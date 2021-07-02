using UnityEngine;

using System.Collections;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;


namespace SIGVerse.TurtleBot
{
	public class TurtleBot2PubDepth : MonoBehaviour
	{
		public string rosbridgeIP;
		public int sigverseBridgePort;

		public string topicNameCameraInfo = "/camera/depth/camera_info";
		public string topicNameImage      = "/camera/depth/image_raw";

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;
		//--------------------------------------------------

		System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		SIGVerseRosBridgeMessage<ImageForSIGVerseBridge> imageMsg = null;

		// Xtion
		private Camera xtionDepthCamera;
		private Texture2D imageTexture;
		byte[]  byteArray; 


		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private float elapsedTime = 0.0f;


		void Start()
		{
			if (this.rosbridgeIP.Equals(string.Empty))
			{
				this.rosbridgeIP = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.sigverseBridgePort==0)
			{
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}


			this.tcpClient = new System.Net.Sockets.TcpClient(this.rosbridgeIP, this.sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;


			// Depth Camera
			this.xtionDepthCamera = SIGVerseUtils.FindTransformFromChild(this.transform.root, "camera_depth_optical_frame").GetComponentInChildren<Camera>();

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
			double[] K = { 570.3422241210938, 0.0, 314.5, 0.0, 570.3422241210938, 235.5, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 570.3422241210938, 0.0, 314.5, 0.0, 0.0, 570.3422241210938, 235.5, 0.0, 0.0, 0.0, 1.0, 0.0 };
			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);

			//  [camera/depth/Image_raw]
			string encoding = "16UC1";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 2;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "camera_depth_optical_frame");


			this.cameraInfoMsg = new SIGVerseRosBridgeMessage<CameraInfoForSIGVerseBridge>("publish", this.topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseRosBridgeMessage<ImageForSIGVerseBridge>     ("publish", this.topicNameImage     , ImageForSIGVerseBridge.GetMessageType(),      this.imageData);
		}

		void OnApplicationQuit()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient != null) { this.tcpClient.Close(); }
		}

		void Update()
		{
			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			base.StartCoroutine(GenerateDepthBuffer());
		}


		private bool canStartCoroutine = true;

		public IEnumerator GenerateDepthBuffer()
		{
			if (!this.canStartCoroutine)
			{
				yield break;
			}

			this.canStartCoroutine = false;


			// Set a terget texture as a target of rendering
			RenderTexture.active = this.xtionDepthCamera.targetTexture;

//			yield return new WaitForEndOfFrame();

			// Apply depth information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0);

			this.imageTexture.Apply();

//			yield return null;

			// Convert pixel values to depth buffer for ROS message
			byte[] depthBytes = this.imageTexture.GetRawTextureData();

//			yield return null;

			this.header.Update();

			//  [camera/depth/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			this.cameraInfoMsg.SendMsg(this.networkStream);


//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

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

//			sw.Stop();
//			UnityEngine.Debug.Log("time="+sw.Elapsed);

			this.imageData.header = this.header;
			this.imageData.data = this.byteArray;
			this.imageMsg.msg = this.imageData;

//			yield return null;

			this.imageMsg.SendMsg(this.networkStream);

			this.canStartCoroutine = true;
		}

		//unsafe private void texture2depthData(byte[] depthBytes)
		//{
		//	fixed (byte* dataArray0 = &this.byteArray[0])
		//	{
		//		fixed (byte* texArray0 = &depthBytes[0])
		//		{
		//			byte* dataArray = dataArray0;
		//			byte* texArray = texArray0;

		//			for (int i = 0; i < this.imageTexture.width * this.imageTexture.height; i++)
		//			{
		//				* dataArray = * texArray; dataArray++; texArray++;
		//				* dataArray = * texArray; dataArray++; texArray++;
		//				texArray++;
		//			}
		//		}
		//	}
		//}
	}
}
