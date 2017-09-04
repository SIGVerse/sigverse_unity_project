using UnityEngine;

using System.Collections;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;


namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3PubR200RGB : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

		public GameObject rgbCameraObj;

		public string topicNameCameraInfo = "/camera/rgb/camera_info";
		public string topicNameImage      = "/camera/rgb/image_raw";

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;


		//--------------------------------------------------

		System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge> cameraInfoMsg = null;
		SIGVerseROSBridgeMessage<ImageForSIGVerseBridge> imageMsg = null;

		// RGB camera
		private Camera    rgbCamera;
		private Texture2D imageTexture;

		// TimeStamp
		private Header header;

		private CameraInfoForSIGVerseBridge cameraInfoData;
		private ImageForSIGVerseBridge imageData;

		private float elapsedTime = 0.0f;


		void Start()
		{
			if (!ConfigManager.Instance.configInfo.rosbridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP        = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.sigverseBridgePort==0)
			{
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}


			this.tcpClient = new System.Net.Sockets.TcpClient(rosBridgeIP, sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;


			// RGB Camera
			this.rgbCamera = this.rgbCameraObj.GetComponentInChildren<Camera>();

			int imageWidth  = this.rgbCamera.targetTexture.width;
			int imageHeight = this.rgbCamera.targetTexture.height;

			this.imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);


			//  [camera/rgb/CameraInfo]
			string distortionModel = "plumb_bob";
			double[] D = { 0.0, 0.0, 0.0, 0.0, 0.0 };
			double[] K = { 618.6206665039062, 0.0, 310.1082458496094, 0.0, 624.5146484375, 230.0232696533203, 0.0, 0.0, 1.0 };
			double[] R = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };
			double[] P = { 618.6206665039062, 0.0, 310.1082458496094, 0.0, 0.0, 624.5146484375, 230.0232696533203, 0.0, 0.0, 0.0, 1.0, 0.0 };
			RegionOfInterest roi = new RegionOfInterest(0, 0, 0, 0, false);

			this.cameraInfoData = new CameraInfoForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, distortionModel, D, K, R, P, 0, 0, roi);
			
			//  [camera/rgb/Image_raw]
			string encoding = "rgb8";
			byte isBigendian = 0;
			uint step = (uint)imageWidth * 3;

			this.imageData = new ImageForSIGVerseBridge(null, (uint)imageHeight, (uint)imageWidth, encoding, isBigendian, step, null);

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), this.rgbCameraObj.name);


			this.cameraInfoMsg = new SIGVerseROSBridgeMessage<CameraInfoForSIGVerseBridge>("publish", this.topicNameCameraInfo, CameraInfoForSIGVerseBridge.GetMessageType(), this.cameraInfoData);
			this.imageMsg      = new SIGVerseROSBridgeMessage<ImageForSIGVerseBridge>     ("publish", this.topicNameImage,      ImageForSIGVerseBridge.GetMessageType(),      this.imageData);
		}

		void OnDestroy()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient != null) { this.tcpClient.Close(); }
		}

		void Update()
		{
			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			this.PubImage();
		}


		private void PubImage()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			// Set a terget texture as a target of rendering
			RenderTexture.active = this.rgbCamera.targetTexture;

			// Apply rgb information to 2D texture
			this.imageTexture.ReadPixels(new Rect(0, 0, this.imageTexture.width, this.imageTexture.height), 0, 0);

			this.imageTexture.Apply();

			// Convert pixel values to depth buffer for ROS message
			byte[] rgbBytes = this.imageTexture.GetRawTextureData();


			//  [camera/rgb/CameraInfo]
			this.cameraInfoData.header = this.header;
			this.cameraInfoMsg.msg = this.cameraInfoData;

			this.cameraInfoMsg.sendMsg(this.networkStream);

			//  [camera/rgb/Image_raw]
			this.imageData.header = this.header;
			this.imageData.data = rgbBytes;
			this.imageMsg.msg = this.imageData;

			this.imageMsg.sendMsg(this.networkStream);

//			sw.Stop();
//			UnityEngine.Debug.Log("time=" + sw.Elapsed);
		}
	}
}

