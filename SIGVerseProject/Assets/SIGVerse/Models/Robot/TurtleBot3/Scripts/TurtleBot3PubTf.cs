using UnityEngine;

using System.Collections;
using SIGVerse.SIGVerseRosBridge;
using SIGVerse.RosBridge.geometry_msgs;
using System.Collections.Generic;
using System.Threading;
using SIGVerse.RosBridge;
using SIGVerse.Common;

namespace SIGVerse.TurtleBot3
{
	[RequireComponent(typeof (RobotPubSynchronizer))]

	public class TurtleBot3PubTf : SIGVerseRosBridgePubMessage
	{
		public string topicName = "/sigverse/TurtleBot3/tf";

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private class TfInfo
		{
			public UnityEngine.Transform linkTransform;
			public TransformStamped      transformStamped;

			public TfInfo(UnityEngine.Transform linkTransform, TransformStamped transformStamped)
			{
				this.linkTransform    = linkTransform;
				this.transformStamped = transformStamped;
			}

			public void UpdateTransformForLocal()
			{
				UnityEngine.Vector3    pos = linkTransform.localPosition;
				UnityEngine.Quaternion qua = linkTransform.localRotation;

				this.transformStamped.transform.translation = new UnityEngine.Vector3(-pos.x, pos.y, pos.z);
				this.transformStamped.transform.rotation    = new UnityEngine.Quaternion(qua.x, -qua.y, -qua.z, qua.w);
			}

			public void UpdateTransformForGlobal()
			{
				UnityEngine.Vector3 pos = linkTransform.localPosition;
				UnityEngine.Quaternion qua = linkTransform.localRotation;

				this.transformStamped.transform.translation = new UnityEngine.Vector3(pos.z, -pos.x, pos.y);
				this.transformStamped.transform.rotation    = new UnityEngine.Quaternion(-qua.z, qua.x, -qua.y, qua.w);
			}
		}

		private RobotPubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		private SIGVerseRosBridgeMessage<TransformStamped[]> transformStampedMsg = null;

		private List<TfInfo> localTfInfoList = new List<TfInfo>();

		private float elapsedTime = 0.0f;

		private bool isPublishing = false;

		private bool shouldSendMessage = false;

		private bool isUsingThread;


		void Awake()
		{
			List<UnityEngine.Transform> localLinkList = TurtleBot3Common.GetLinksInChildren(this.transform.root);

			foreach(UnityEngine.Transform localLink in localLinkList)
			{
				TransformStamped localTransformStamped = new TransformStamped();

				localTransformStamped.header.frame_id = localLink.parent.name;
				localTransformStamped.child_frame_id  = localLink.name;

				TfInfo localTfInfo = new TfInfo(localLink, localTransformStamped);

				this.localTfInfoList.Add(localTfInfo);

				this.synchronizer = this.GetComponent<RobotPubSynchronizer>();

				this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();

				this.isUsingThread = this.synchronizer.useThread;
			}
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

			this.transformStampedMsg = new SIGVerseRosBridgeMessage<TransformStamped[]>("publish", this.topicName, "sigverse/TfList", null);
		}

		//void OnDestroy()
		//{
		//	if (this.networkStream != null) { this.networkStream.Close(); }
		//	if (this.tcpClient     != null) { this.tcpClient.Close(); }
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

				this.PubTF();
			}
		}

		private void PubTF()
		{
//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			this.isPublishing = true;

			TransformStamped[] transformStampedArray = new TransformStamped[localTfInfoList.Count];

			// Add local TF infos
			for (int i=0; i<localTfInfoList.Count; i++)
			{
				localTfInfoList[i].UpdateTransformForLocal();

				localTfInfoList[i].transformStamped.header.Update();

				transformStampedArray[i] = localTfInfoList[i].transformStamped;
			}

			this.transformStampedMsg.msg = transformStampedArray;

			if(this.isUsingThread)
			{
				Thread thread = new Thread(new ThreadStart(SendTF));
				thread.Start();
			}
			else
			{
				this.SendTF();
			}

//			sw.Stop();
//			Debug.Log("tf sending time="+sw.Elapsed);
		}


		private void SendTF()
		{
			this.transformStampedMsg.SendMsg(this.networkStream);
			this.isPublishing = false;
		}


		public override bool IsConnected()
		{
			return this.networkStream !=null && this.tcpClient.Connected;
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

