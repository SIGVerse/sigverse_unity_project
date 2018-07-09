using UnityEngine;
using System;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Collections;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.TurtleBot3
{
	[RequireComponent(typeof (TurtleBot3PubSynchronizer))]

	public class TurtleBot3PubLDS : SIGVerseRosBridgePubMessage
	{
		private static readonly int SendingIntervalMin = 200;

		public string topicName = "/scan";

		[TooltipAttribute("[ms]  at least 200")]
		public float sendingInterval = SendingIntervalMin;

		public Transform sensorLink;

		public LayerMask layerMask = -1;

		[HeaderAttribute("DEBUG")]
		public bool showDebugRay = true;
		public Color debugRayColor = Color.red;

		//--------------------------------------------------

		private const float AngleMin       = 0;
		private const float AngleMax       = 360;
		private const float AngleIncrement = 1.0f;
		private const float RangeMin       = 0.12f;
		private const float RangeMax       = 3.5f;
		private const int   NumLines       = 360; // LaserAngle / AngleIncrement;

//		private const float LaserAngle     = 360;
//		private const int   ScanRate       = 300; //[rpm]

		private Transform sensorPivot;

		private TurtleBot3PubSynchronizer synchronizer;

		private int publishSequenceNumber;


		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg = null;

		private LaserScanForSIGVerseBridge laserScan;

		// Message header
		private Header header;

		private float elapsedTime;

		private bool isPublishing = false;

		private bool shouldSendMessage = false;

		private bool isUsingThread;



		private void OnValidate()
		{
			if(this.sendingInterval < SendingIntervalMin){ this.sendingInterval = SendingIntervalMin; }
		}

		void Awake()
		{
			this.sensorPivot = this.sensorLink.Find("LDS_sensor_pivot");

			this.synchronizer = this.GetComponent<TurtleBot3PubSynchronizer>();

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

			this.laserScan.header = this.header;

			this.laserScan.angle_min       = AngleMin * Mathf.Deg2Rad;
			this.laserScan.angle_max       = AngleMax * Mathf.Deg2Rad;
			this.laserScan.angle_increment = AngleIncrement * Mathf.Deg2Rad;
			this.laserScan.time_increment  = 0.0; // (1.0f / (ScanRate / 60.0f)) / NumLines;
			this.laserScan.scan_time       = 0.0; //  1.0f / (ScanRate / 60.0f);
			this.laserScan.range_min       = RangeMin;
			this.laserScan.range_max       = RangeMax;
			this.laserScan.ranges          = new double[NumLines];
			this.laserScan.intensities     = new double[NumLines];

			this.laserScanMsg = new SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);
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

			for (int index = 0; index < NumLines; index++)
			{
				Vector3 ray = this.sensorPivot.rotation * Quaternion.AngleAxis(-index * AngleIncrement, Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorPivot.position, ray, out hit, RangeMax, this.layerMask))
				{
					distance = hit.distance;
				}

				this.laserScan.ranges     [index] = distance;
				this.laserScan.intensities[index] = 0.0;

				if (this.showDebugRay)
				{
					Debug.DrawRay(this.sensorPivot.position, ray * distance, this.debugRayColor);
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
