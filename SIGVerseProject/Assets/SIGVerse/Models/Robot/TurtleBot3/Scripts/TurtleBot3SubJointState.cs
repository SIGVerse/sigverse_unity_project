using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.ROSBridge;
using SIGVerse.Common;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3SubJointState : MonoBehaviour
	{
		public string rosbridgeIP;
		public int    rosbridgePort;

		public string topicName;

		//--------------------------------------------------

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		private Dictionary<string, Transform> transformMap;  // key:joint name, value:link transform

		// Use this for initialization
		void Start ()
		{
			if (this.rosbridgeIP.Equals(string.Empty))
			{
				this.rosbridgeIP = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.rosbridgePort==0)
			{
				this.rosbridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}

			this.transformMap = TurtleBot3Common.GetJointNameToLinkMap(this.transform);

			this.webSocketConnection = new SIGVerse.ROSBridge.ROSBridgeWebSocketConnection(this.rosbridgeIP, this.rosbridgePort);

			this.webSocketConnection.Subscribe<SIGVerse.ROSBridge.sensor_msgs.JointState>(topicName, this.JointStateCallback);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();
		}
	
		public void JointStateCallback(SIGVerse.ROSBridge.sensor_msgs.JointState jointState)
		{
			for(int i=0; i<jointState.name.Count; i++)
			{
				string name = jointState.name[i];

				TurtleBot3Common.UpdateJoint(name, this.transformMap[name], (float)jointState.position[i]);
			}
		}

		
		// Update is called once per frame
		void Update ()
		{
			this.webSocketConnection.Render();
		}

		void OnApplicationQuit()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Disconnect();
			}
		}

		private Vector3 GetTargetVector3(string name, Transform transform)
		{
			if(TurtleBot3Common.jointInfoMap[name].movementType == TurtleBot3JointInfo.MovementType.Angular)
			{
				return transform.localEulerAngles;
			}
			else
			{
				return transform.localPosition;
			}
		}
	}
}

