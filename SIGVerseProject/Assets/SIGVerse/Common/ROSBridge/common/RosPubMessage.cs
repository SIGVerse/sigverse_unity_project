using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.RosBridge
{
	abstract public class RosPubMessage<Tmsg> : MonoBehaviour, IRosConnection where Tmsg : RosMessage
	{
		public string rosBridgeIP;
		public int    rosBridgePort;

		public string topicName;

		//--------------------------------------------------
		protected RosBridgeWebSocketConnection webSocketConnection = null;

		protected RosBridgePublisher<Tmsg> publisher;


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

				this.publisher = this.webSocketConnection.Advertise<Tmsg>(topicName);

				// Connect to ROSbridge server
				this.webSocketConnection.Connect();
			}
			else
			{
				this.webSocketConnection = RosConnectionManager.Instance.rosConnections.rosBridgeWebSocketConnectionMap[topicName];

				this.publisher = this.webSocketConnection.Advertise<Tmsg>(topicName);
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
		}

		public virtual bool IsConnected()
		{
			return this.webSocketConnection!=null && this.webSocketConnection.IsConnected;
		}

		public virtual void Clear()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.publisher);
			}
		}

		public virtual void Close()
		{
			this.Clear();

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

