using UnityEngine;

using System;
using SIGVerse.RosBridge.sensor_msgs.msg;
using SIGVerse.RosBridge.std_msgs.msg;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;
using System.Collections;
using System.Threading;
using SIGVerse.RosBridge;

namespace SIGVerse.Common
{
	[RequireComponent(typeof (RobotPubSynchronizer))]
	public abstract class RobotPubLaserScan : SIGVerseRosBridgePubMessage
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

		protected RobotPubSynchronizer synchronizer;

		protected int publishSequenceNumber;

		protected System.Net.Sockets.TcpClient tcpClient = null;
		protected System.Net.Sockets.NetworkStream networkStream = null;

		protected SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge> laserScanMsg;

		protected LaserScanForSIGVerseBridge laserScan;

		// Message header
		protected Header header;

		protected float elapsedTime;

		protected bool isPublishing = false;

		protected bool shouldSendMessage = false;

		protected bool isUsingThread;


		protected virtual void Awake()
		{
			this.header = new Header(new SIGVerse.RosBridge.builtin_interfaces.msg.Time(0, 0), this.sensorLink.name);

			this.InitializeVariables();

			this.synchronizer = this.GetComponent<RobotPubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();

			this.isUsingThread = this.synchronizer.useThread;
		}

		/// <summary>
		/// Initialize laserScan
		/// </summary>
		protected abstract void InitializeVariables();

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

			this.laserScanMsg = new SIGVerseRosBridgeMessage<LaserScanForSIGVerseBridge>("publish", this.topicName, LaserScanForSIGVerseBridge.GetMessageType(), this.laserScan);

//			Debug.Log("this.layerMask.value = "+this.layerMask.value);
		}

		//void OnDestroy()
		//{
		//	if (this.networkStream != null) { this.networkStream.Close(); }
		//	if (this.tcpClient != null) { this.tcpClient.Close(); }
		//}

		protected virtual void Update()
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

		protected virtual void LateUpdate()
		{
			if(this.shouldSendMessage)
			{
				this.shouldSendMessage = false;

				this.PubSensorData();
			}
		}

		protected virtual void PubSensorData()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			this.isPublishing = true;

			// Set current time to the header
			this.laserScan.header.Update();

			float laserAngle = (float)(this.laserScan.angle_max - this.laserScan.angle_min) * Mathf.Rad2Deg;
			float laserAngularResolution = (float)this.laserScan.angle_increment * Mathf.Rad2Deg;

			for (int index = 0; index < this.laserScan.ranges.Length; index++)
			{
				Vector3 ray = this.sensorLink.rotation * Quaternion.AngleAxis(laserAngle * 0.5f + (-1 * index * laserAngularResolution), Vector3.forward) * -Vector3.right;

				float distance = 0.0f;
				RaycastHit hit;

				if (Physics.Raycast(this.sensorLink.position, ray, out hit, (float)this.laserScan.range_max, this.layerMask))
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

		protected virtual void SendSensorData()
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

		protected virtual void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
