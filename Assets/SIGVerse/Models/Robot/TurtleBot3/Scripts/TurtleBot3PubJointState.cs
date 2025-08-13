using UnityEngine;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs.msg;
using SIGVerse.RosBridge.sensor_msgs.msg;
using SIGVerse.Common;

using JointType = SIGVerse.TurtleBot3.TurtleBot3JointInfo.JointType;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3PubJointState : RosPubMessage<JointState>
	{
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

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(new SIGVerse.RosBridge.builtin_interfaces.msg.Time(0, 0), "tb3_omc_joint_state");

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

		protected override void Update()
		{
			base.Update();

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
			// TODO Have to check sensor data of a real turtlebot3 
			//5 grip_joint
			positions.Add(-this.gripJointLink.localPosition.y);
			//6 grip_joint_sub
			positions.Add(+this.gripJointSubLink.localPosition.y);

//			Debug.Log("Pub JointState joint1="+positions[0]);
			this.jointState.header.Update();
			this.jointState.position = positions;

//			float position = TurtleBot3Common.GetClampedPosition(value, name);

			this.publisher.Publish(this.jointState);
		}
	}
}

