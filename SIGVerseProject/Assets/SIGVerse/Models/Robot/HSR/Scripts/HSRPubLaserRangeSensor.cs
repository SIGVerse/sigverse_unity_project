using UnityEngine;

using System;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Collections;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubLaserRangeSensor : SIGVerseRosBridgePubMessage
	{
		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		public Transform sensorLink;

		public LayerMask layerMask = -1;

		[HeaderAttribute("DEBUG")]
		public bool showDebugRay = true;
		public Color debugRayColor = Color.red;

		//--------------------------------------------------

		private const float HalfOfLaserAngle = 120.0f;
		private const float LaserDistance = 20;
		private const float LaserAngle = HalfOfLaserAngle * 2.0f;
		private const float LaserAngularResolution = 0.25f;

		private HSRPubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private int numLines;

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg;

		private LaserScanForSIGVerseBridge laserScan;

		// Message header
		private Header header;

		private float elapsedTime;

		private bool isPublishing = false;

		private bool shouldSendMessage = false;

		private bool isUsingThread;


		void Awake()
		{
			this.synchronizer = this.GetComponent<HSRPubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();

			this.isUsingThread = this.synchronizer.useThread;
		}

		protected override void Start()
		{
			base.Start();

			if(!RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.ContainsKey(topicName))
			{
				this.tcpClient = SIGVerseRosBridgeConnection.GetConnection(this.rosBridgeIP, this.sigverseBridgePort);

				RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.Add(topicName, this.tcpClient);
			}
			else
			{
				this.tcpClient = RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap[topicName];
			}

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;

			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), this.sensorLink.name);

			this.laserScan = new LaserScanForSIGVerseBridge();

			this.numLines = (int)Mathf.Round(LaserAngle / LaserAngularResolution) + 1;

			this.laserScan.header = this.header;

			this.laserScan.angle_min = -HalfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_max = +HalfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_increment = LaserAngularResolution * Mathf.Deg2Rad;
			this.laserScan.time_increment = 0.0;
			this.laserScan.scan_time = 0.1; // tentative
			this.laserScan.range_min = 0.05;
			this.laserScan.range_max = 20.0;
			this.laserScan.ranges      = new double[this.numLines];
			this.laserScan.intensities = new double[this.numLines];

			this.laserScanMsg = new SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);

//			Debug.Log("this.layerMask.value = "+this.layerMask.value);
		}

		//void OnDestroy()
		//{
		//	if (this.networkStream != null) { this.networkStream.Close(); }
		//	if (this.tcpClient != null) { this.tcpClient.Close(); }
		//}

		void Update()
		{
			if(!this.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.isPublishing || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!this.synchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			this.shouldSendMessage = true;
		}

		void LateUpdate()
		{
			if(this.shouldSendMessage)
			{
				this.shouldSendMessage = false;

				this.PubSensorData();
			}
		}

		private void PubSensorData()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			this.isPublishing = true;

			// Set current time to the header
			this.laserScan.header.Update();

			for (int index = 0; index < this.numLines; index++)
			{
				Vector3 ray = this.sensorLink.rotation * Quaternion.AngleAxis(LaserAngle * 0.5f + (-1 * index * LaserAngularResolution), Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorLink.position, ray, out hit, LaserDistance, this.layerMask))
				{
					distance = hit.distance;
				}

				this.laserScan.ranges     [index] = distance;
				this.laserScan.intensities[index] = 0.0;

				if (this.showDebugRay)
				{
					Debug.DrawRay(this.sensorLink.position, ray * distance, this.debugRayColor);
				}
			}

//			yield return null;

			this.laserScanMsg.msg = this.laserScan;

			if(this.isUsingThread)
			{
				Thread thread = new Thread(new ThreadStart(SendSensorData));
				thread.Start();
			}
			else
			{
				this.SendSensorData();
			}

//			sw.Stop();
//			Debug.Log("LRF sending time=" + sw.Elapsed);
		}

		private void SendSensorData()
		{
			this.laserScanMsg.SendMsg(this.networkStream);
			this.isPublishing = false;
		}


		public override bool IsConnected()
		{
			return this.networkStream != null && this.tcpClient.Connected;
		}

		public override void Close()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient     != null) { this.tcpClient.Close(); }
		}

		void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
