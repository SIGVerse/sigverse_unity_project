using UnityEngine;

using System;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Collections;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.PR2
{
	[RequireComponent(typeof (PR2PubSynchronizer))]

	public class PR2PubLaserScan : SIGVerseRosBridgePubMessage
	{
		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		public Transform sensorLink;

		public LayerMask layerMask = -1;

		[HeaderAttribute("Laser Parameters")]
		[SerializeField]
		public int laserNum = 1040;
		
		[HeaderAttribute("DEBUG")]
		public bool showDebugRay = true;
		public Color debugRayColor = Color.red;

		//--------------------------------------------------

		private const double AngleInc = 0.00436332309619;  // This value is fixed for the device.
		private const double RangeMin = 0.05;
		private const double RangeMax = 60.0;

		private PR2PubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg;

		private double angleMin;
		private double angleMax;

		private LaserScanForSIGVerseBridge laserScan;

		// Message header
		private Header header;

		private float elapsedTime;

		private bool isPublishing = false;

		private bool shouldSendMessage = false;

		private bool isUsingThread;


		void Awake()
		{
			this.synchronizer = this.GetComponent<PR2PubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();

			this.isUsingThread = this.synchronizer.useThread;
		}

		protected override void Start()
		{
			base.Start();

			this.angleMin = - AngleInc * this.laserNum / 2;
			this.angleMax = + AngleInc * this.laserNum / 2 - AngleInc;


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

			this.laserScan.angle_min       = this.angleMin;
			this.laserScan.angle_max       = this.angleMax;
			this.laserScan.angle_increment = AngleInc;
			this.laserScan.time_increment  = 0.0;
			this.laserScan.scan_time       = this.sendingInterval/1000.0f;
			this.laserScan.range_min       = RangeMin;
			this.laserScan.range_max       = RangeMax;
			this.laserScan.ranges      = new double[this.laserNum];
			this.laserScan.intensities = new double[this.laserNum];

			this.laserScanMsg = new SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);

//			Debug.Log("AngleMin="+this.angleMin+", AngleMax="+this.angleMax+", AngleInc="+AngleInc);
		}

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

			for (int index = 0; index < this.laserNum; index++)
			{
				Vector3 ray = this.sensorLink.rotation * Quaternion.AngleAxis((float)(this.angleMax -1 * index * AngleInc) * Mathf.Rad2Deg, Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorLink.position, ray, out hit, (float)RangeMax, this.layerMask))
				{
					distance = hit.distance;
				}

				this.laserScan.ranges     [index] = distance;
				this.laserScan.intensities[index] = 0.0;

				if (this.showDebugRay && index % 5 == 0) // Thinned out
				{
					Debug.DrawRay(this.sensorLink.position, ray * distance, this.debugRayColor);
				}
			}

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
