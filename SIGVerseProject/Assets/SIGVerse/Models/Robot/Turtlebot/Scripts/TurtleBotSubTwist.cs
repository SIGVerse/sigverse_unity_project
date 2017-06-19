using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.Common;


namespace SIGVerse.TurtleBot
{
	public class TurtleBotSubTwist : MonoBehaviour
	{
		public string rosbridgeIP;
		public int    rosbridgePort;

		public string topicName;

		//--------------------------------------------------

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		void Start()
		{
			if (this.rosbridgeIP.Equals(string.Empty))
			{
				this.rosbridgeIP = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.rosbridgePort==0)
			{
				this.rosbridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}

			this.webSocketConnection = new SIGVerse.ROSBridge.ROSBridgeWebSocketConnection(this.rosbridgeIP, this.rosbridgePort);

			this.webSocketConnection.Subscribe<SIGVerse.ROSBridge.geometry_msgs.Twist>(topicName, this.TwistCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		public void TwistCallback(SIGVerse.ROSBridge.geometry_msgs.Twist twist)
		{
			UnityEngine.Vector3 linearVel  = new UnityEngine.Vector3((float)twist.linear.x,  (float)twist.linear.y,  (float)twist.linear.z);
			UnityEngine.Vector3 angularVel = new UnityEngine.Vector3((float)twist.angular.x, (float)twist.angular.y, (float)twist.angular.z);

			UnityEngine.Vector3 robotLocalPosition = this.transform.forward * linearVel.x * UnityEngine.Time.fixedDeltaTime;

			this.transform.position = this.transform.position + robotLocalPosition;
			this.transform.Rotate(0.0f, angularVel.z / Mathf.PI * 180 * UnityEngine.Time.fixedDeltaTime * -1, 0.0f);
		}

		void OnApplicationQuit()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
			this.webSocketConnection.Render();
		}
	}
}

