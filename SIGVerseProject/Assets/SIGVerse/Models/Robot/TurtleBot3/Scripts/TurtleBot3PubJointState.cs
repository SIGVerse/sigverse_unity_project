using UnityEngine;
using System.Collections.Generic;
using SIGVerse.ROSBridge;
using SIGVerse.ROSBridge.std_msgs;
using SIGVerse.ROSBridge.sensor_msgs;
using SIGVerse.Common;

using JointType = SIGVerse.TurtleBot3.TurtleBot3JointInfo.JointType;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3PubJointState : MonoBehaviour
	{
		public string rosBridgeIP;
		public int rosBridgePort;

		public string topicName;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private Transform joint1Link;
		private Transform joint2Link;
		private Transform joint3Link;
		private Transform joint4Link;
		private Transform gripJointLink;
		private Transform gripJointSubLink;

		private JointState jointState;

		// ROS bridge
		private ROSBridgeWebSocketConnection webSocketConnection = null;

		private ROSBridgePublisher<JointState> jointStatePublisher;

		private float elapsedTime;


		void Awake()
		{
			this.joint1Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint1);
			this.joint2Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint2);
			this.joint3Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint3);
			this.joint4Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint4);

			this.gripJointLink    = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.GripJoint);
			this.gripJointSubLink = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.GripJointSub);
		}

		void Start()
		{
			if (!ConfigManager.Instance.configInfo.rosbridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP   = ConfigManager.Instance.configInfo.rosbridgeIP;
				this.rosBridgePort = ConfigManager.Instance.configInfo.rosbridgePort;
			}
			
			this.webSocketConnection = new SIGVerse.ROSBridge.ROSBridgeWebSocketConnection(rosBridgeIP, rosBridgePort);

			this.jointStatePublisher = this.webSocketConnection.Advertise<JointState>(topicName);

			// Connect to ROSbridge server
			this.webSocketConnection.Connect();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.ROSBridge.msg_helpers.Time(0, 0), "tb3_omc_joint_state");

			this.jointState.name = new List<string>();
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.Joint1]);
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.Joint2]);
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.Joint3]);
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.Joint4]);
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.GripJoint]);
			this.jointState.name.Add(TurtleBot3Common.jointNameMap[JointType.GripJointSub]);

			this.jointState.position = null;
			this.jointState.velocity = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
			this.jointState.effort   = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
		}

		void OnDestroy()
		{
			if (this.webSocketConnection != null)
			{
				this.webSocketConnection.Unadvertise(this.jointStatePublisher);

				this.webSocketConnection.Disconnect();
			}
		}

		void Update()
		{
			this.webSocketConnection.Render();

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			List<double> positions = new List<double>();

			//1 joint1
			positions.Add(-TurtleBot3Common.GetCorrectedJointsEulerAngle(TurtleBot3Common.jointNameMap[JointType.Joint1], this.joint1Link.localEulerAngles.z) * Mathf.Deg2Rad);
			//2 joint2
			positions.Add(-TurtleBot3Common.GetCorrectedJointsEulerAngle(TurtleBot3Common.jointNameMap[JointType.Joint2], this.joint2Link.localEulerAngles.y) * Mathf.Deg2Rad);
			//3 joint3
			positions.Add(-TurtleBot3Common.GetCorrectedJointsEulerAngle(TurtleBot3Common.jointNameMap[JointType.Joint3], this.joint3Link.localEulerAngles.y) * Mathf.Deg2Rad);
			//4 joint4
			positions.Add(-TurtleBot3Common.GetCorrectedJointsEulerAngle(TurtleBot3Common.jointNameMap[JointType.Joint4], this.joint4Link.localEulerAngles.y) * Mathf.Deg2Rad);
			//5 grip_joint
			positions.Add(-this.gripJointLink.localEulerAngles.y * Mathf.Deg2Rad);
			//6 grip_joint_sub
			positions.Add(+this.gripJointSubLink.localEulerAngles.y * Mathf.Deg2Rad);

//			Debug.Log("Pub JointState joint1="+positions[0]);
			this.jointState.position = positions;

//			float position = TurtleBot3Common.GetClampedPosition(value, name);

			this.jointStatePublisher.Publish(this.jointState);
		}
	}
}

