using UnityEngine;

using System.Collections;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.Common;
using SIGVerse.SIGVerseROSBridge;
using SIGVerse.ROSBridge.geometry_msgs;
using System.Collections.Generic;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3PubTf : MonoBehaviour
	{
		public string rosBridgeIP;
		public int sigverseBridgePort;

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

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		private SIGVerseROSBridgeMessage<TransformStamped[]> transformStampedMsg = null;

		private List<TfInfo> localTfInfoList = new List<TfInfo>();

		private float elapsedTime = 0.0f;


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
			}
		}

		void Start()
		{
			if (!ConfigManager.Instance.configInfo.rosbridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP        = ConfigManager.Instance.configInfo.rosbridgeIP;
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}


			this.tcpClient = new System.Net.Sockets.TcpClient(rosBridgeIP, sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;


			this.transformStampedMsg = new SIGVerseROSBridgeMessage<TransformStamped[]>("publish", this.topicName, "sigverse/TfList", null);
		}

		void OnDestroy()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient     != null) { this.tcpClient.Close(); }
		}

		void Update()
		{
			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

//			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//			sw.Start();

			TransformStamped[] transformStampedArray = new TransformStamped[localTfInfoList.Count];

			// Add local tf infos
			for (int i=0; i<localTfInfoList.Count; i++)
			{
				localTfInfoList[i].UpdateTransformForLocal();

				localTfInfoList[i].transformStamped.header.Update();

				transformStampedArray[i] = localTfInfoList[i].transformStamped;
			}

			this.transformStampedMsg.msg = transformStampedArray;
			
			this.transformStampedMsg.SendMsg(this.networkStream);

//			sw.Stop();
//			Debug.Log("tf sending time="+sw.Elapsed);
		}
	}
}

