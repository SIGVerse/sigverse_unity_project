using UnityEngine;
using SIGVerse.Common;
using System.Collections.Generic;

namespace SIGVerse.RosBridge
{
	abstract public class RosSubMessage<Tmsg> : MonoBehaviour, IRosConnection where Tmsg : RosMessage, new()
	{
		public string rosBridgeIP;
		public int    rosBridgePort;

		public string topicName;

		//--------------------------------------------------
		protected RosBridgeWebSocketConnection webSocketConnection = null;

		protected RosBridgeSubscriber<Tmsg> subscriber = null;


		protected virtual void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP   = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.rosBridgePort == 0)
			{
				this.rosBridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}

			if(!RosConnectionManager.Instance.rosConnections.rosBridgeWebSocketConnectionMap.ContainsKey(topicName))
			{
				this.webSocketConnection = new SIGVerse.RosBridge.RosBridgeWebSocketConnection(rosBridgeIP, rosBridgePort);

				RosConnectionManager.Instance.rosConnections.rosBridgeWebSocketConnectionMap.Add(topicName, this.webSocketConnection);

				this.subscriber = this.webSocketConnection.Subscribe<Tmsg>(topicName, this.SubscribeMessageCallback);

				// Connect to ROSbridge server
				this.webSocketConnection.Connect();
			}
			else
			{
				this.webSocketConnection = RosConnectionManager.Instance.rosConnections.rosBridgeWebSocketConnectionMap[topicName];

				this.subscriber = this.webSocketConnection.Subscribe<Tmsg>(topicName, this.SubscribeMessageCallback);
			}
		}

		//protected virtual void OnDestroy()
		//{
		//	this.Clear();
		//	this.Close();
		//}


		protected virtual void Update()
		{
			if(!this.IsConnected()) { return; }

			this.webSocketConnection.Render();
		}

		abstract protected void SubscribeMessageCallback(Tmsg rosMsg);


		public virtual bool IsConnected()
		{
			return this.webSocketConnection!=null && this.webSocketConnection.IsConnected;
		}

		public virtual void Clear()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unsubscribe(this.subscriber);
			}
		}

		public virtual void Close()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Disconnect();
			}
		}

		void OnApplicationQuit()
		{
			this.Clear();
			this.Close();
		}
	}
}
