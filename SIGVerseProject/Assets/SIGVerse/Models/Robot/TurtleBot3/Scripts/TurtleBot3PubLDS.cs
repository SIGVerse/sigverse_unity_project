using UnityEngine;
using System;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3PubLDS : MonoBehaviour
	{
		private static readonly int SendingIntervalMin = 200;

		public string rosBridgeIP;
		public int sigverseBridgePort;

		public string topicName = "/scan";

		[TooltipAttribute("[ms]  at least 200")]
		public float sendingInterval = SendingIntervalMin;

		public Transform sensorLink;

		[HeaderAttribute("DEBUG")]
		public bool showDebugRay = true;
		public Color debugRayColor = Color.red;

		//--------------------------------------------------

		private const float AngleMin = 0.0f;         //[rad]
		private const float AngleMax = 2 * Mathf.PI; //[rad]
		private const float RangeMin = 0.12f;
		private const float RangeMax = 3.5f;
		private const float AngleIncrement = 1.0f * Mathf.Deg2Rad;
		private const float LaserAngle = 2 * Mathf.PI;
		private const int   NumLines = 360; // LaserAngle / AngleIncrement;
		private const int   ScanRate =300; //[rpm]

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseROSBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg = null;

		private LaserScanForSIGVerseBridge laserScan;

		// Message header
		private Header header;

		private float elapsedTime = 0.0f;

		private void OnValidate()
		{
			if(this.sendingInterval < SendingIntervalMin){ this.sendingInterval = SendingIntervalMin; }
		}

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

			this.tcpClient = new System.Net.Sockets.TcpClient(this.rosBridgeIP, this.sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;

			this.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), this.sensorLink.name);

			this.laserScan = new LaserScanForSIGVerseBridge();

			this.laserScan.header = this.header;

			this.laserScan.angle_min = AngleMin;
			this.laserScan.angle_max = AngleMax;
			this.laserScan.angle_increment = AngleIncrement;
			this.laserScan.time_increment = 0.0; // (1.0f / (ScanRate / 60.0f)) / NumLines;
			this.laserScan.scan_time      = 0.0; // 1.0f / (ScanRate / 60.0f);
			this.laserScan.range_min = RangeMin;
			this.laserScan.range_max = RangeMax;
			this.laserScan.ranges      = new double[NumLines];
			this.laserScan.intensities = new double[NumLines];

			this.laserScanMsg = new SIGVerseROSBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);
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


			//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			//sw.Start();

			// Set current time to the header
			this.laserScan.header.Update();

			for (int index = 0; index < NumLines; index++)
			{
				Vector3 ray = this.sensorLink.rotation * Quaternion.AngleAxis(-index * AngleIncrement * Mathf.Rad2Deg, Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorLink.position, ray, out hit, RangeMax))
				{
					distance = hit.distance;
				}

				this.laserScan.ranges     [index] = distance;
				this.laserScan.intensities[index] = 1.0;

				if (this.showDebugRay)
				{
					Debug.DrawRay(this.sensorLink.position, ray * distance, this.debugRayColor);
				}
			}

			this.laserScanMsg.msg = this.laserScan;

			this.laserScanMsg.sendMsg(this.networkStream);

			//sw.Stop();
			//Debug.Log("LDS sending time=" + sw.Elapsed);
		}
	}
}
