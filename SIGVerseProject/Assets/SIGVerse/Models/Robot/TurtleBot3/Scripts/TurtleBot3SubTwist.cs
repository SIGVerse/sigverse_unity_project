using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;


namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3SubTwist : MonoBehaviour
	{
		public string rosbridgeIP;
		public int    rosbridgePort;

		public string topicName;

		public Transform baseFootprint;

		//--------------------------------------------------
		float linearVelX;
		float angularVelZ;
		
		// ROS bridge
		private RosBridgeWebSocketConnection webSocketConnection = null;

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

			this.webSocketConnection = new SIGVerse.RosBridge.RosBridgeWebSocketConnection(this.rosbridgeIP, this.rosbridgePort);

			this.webSocketConnection.Subscribe<SIGVerse.RosBridge.geometry_msgs.Twist>(topicName, this.TwistCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}

		public void TwistCallback(SIGVerse.RosBridge.geometry_msgs.Twist twist)
		{
			this.linearVelX  = (float)twist.linear.x;
			this.angularVelZ = (float)twist.angular.z;
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
			UnityEngine.Vector3 robotLocalPosition = -this.baseFootprint.right * this.linearVelX * UnityEngine.Time.deltaTime;

			this.baseFootprint.position = this.baseFootprint.position + robotLocalPosition;
			this.baseFootprint.Rotate(0.0f, 0.0f, -this.angularVelZ / Mathf.PI * 180 * UnityEngine.Time.deltaTime);

			this.webSocketConnection.Render();
		}
	}
}

