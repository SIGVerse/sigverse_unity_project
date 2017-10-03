using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.ROSBridge
{
	public interface IRosBridgeStringHandler : IEventSystemHandler
	{
		void OnReceiveMessage(string message);
	}

	public class RosBridgePubString : MonoBehaviour, IRosBridgeStringHandler
	{
		public string rosBridgeIP;
		public int rosBridgePort;

		public string topicName;

		//--------------------------------------------------
		private std_msgs.String stringMessage;

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		private ROSBridgePublisher<std_msgs.String> stringPublisher;


		void Start()
		{
			if (!ConfigManager.Instance.configInfo.rosbridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP   = ConfigManager.Instance.configInfo.rosbridgeIP;
				this.rosBridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}
			
			this.webSocketConnection = new ROSBridgeWebSocketConnection(rosBridgeIP, rosBridgePort);

			this.stringPublisher = this.webSocketConnection.Advertise<std_msgs.String>(topicName);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();

			this.stringMessage = new std_msgs.String();
		}

		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.stringPublisher);

				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
		}

		public void OnReceiveMessage(string message)
		{
			this.stringMessage.data = message;

			this.stringPublisher.Publish(this.stringMessage);
		}
	}
}

