using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using System;
using static SIGVerse.TIAGo.TIAGoCommon;

namespace SIGVerse.TIAGo
{
	public class TIAGoSubJointTrajectory : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory>, IGraspedObjectHandler
	{
		private Transform torsoLiftLink;
		private Transform head1Link;
		private Transform head2Link;
		private Transform arm1Link;
		private Transform arm2Link;
		private Transform arm3Link;
		private Transform arm4Link;
		private Transform arm5Link;
		private Transform arm6Link;
		private Transform arm7Link;
		private Transform gripperLeftFingerLink;
		private Transform gripperRightFingerLink;

		private float torsoLiftLinkIniPosZ;
		private float arm1LinkIniRotZ;
		private float arm3LinkIniRotZ;
		private float arm4LinkIniRotY;
		private float arm6LinkIniRotY;
		private float arm7LinkIniRotZ;
		private float gripperLeftFingerLinkIniPosX;
		private float gripperRightFingerLinkIniPosX;

		private Dictionary<TIAGoCommon.Joint, TrajectoryInfo> trajectoryInfoMap;
		private List<TIAGoCommon.Joint> trajectoryKeyList;

		private GameObject graspedObject;


		void Awake()
		{
			this.torsoLiftLink          = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link          .ToString());
			this.head1Link              = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_1_link              .ToString());
			this.head2Link              = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_2_link              .ToString());
			this.arm1Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_1_link               .ToString());
			this.arm2Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_2_link               .ToString());
			this.arm3Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_3_link               .ToString());
			this.arm4Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_4_link               .ToString());
			this.arm5Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_5_link               .ToString());
			this.arm6Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_6_link               .ToString());
			this.arm7Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_7_link               .ToString());
			this.gripperLeftFingerLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.gripper_left_finger_link .ToString());
			this.gripperRightFingerLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.gripper_right_finger_link.ToString());

			this.torsoLiftLinkIniPosZ = this.torsoLiftLink.localPosition.z;
			this.arm1LinkIniRotZ = this.arm1Link.localEulerAngles.z;
			this.arm3LinkIniRotZ = this.arm3Link.localEulerAngles.z;
			this.arm4LinkIniRotY = this.arm4Link.localEulerAngles.y;
			this.arm6LinkIniRotY = this.arm6Link.localEulerAngles.y;
			this.arm7LinkIniRotZ = this.arm7Link.localEulerAngles.z;
			this.gripperLeftFingerLinkIniPosX  = this.gripperLeftFingerLink .localPosition.x;
			this.gripperRightFingerLinkIniPosX = this.gripperRightFingerLink.localPosition.x;

			this.trajectoryInfoMap = new Dictionary<TIAGoCommon.Joint, TrajectoryInfo>()
			{
				{ TIAGoCommon.Joint.torso_lift_joint, null },
				{ TIAGoCommon.Joint.head_1_joint, null },
				{ TIAGoCommon.Joint.head_2_joint, null },
				{ TIAGoCommon.Joint.arm_1_joint, null },
				{ TIAGoCommon.Joint.arm_2_joint, null },
				{ TIAGoCommon.Joint.arm_3_joint, null },
				{ TIAGoCommon.Joint.arm_4_joint, null },
				{ TIAGoCommon.Joint.arm_5_joint, null },
				{ TIAGoCommon.Joint.arm_6_joint, null },
				{ TIAGoCommon.Joint.arm_7_joint, null },
				{ TIAGoCommon.Joint.gripper_left_finger_joint,  null },
				{ TIAGoCommon.Joint.gripper_right_finger_joint, null },
			};

			this.trajectoryKeyList = new List<TIAGoCommon.Joint>(trajectoryInfoMap.Keys);
		}


		protected override void Start()
		{
			base.Start();
			
			this.graspedObject = null;
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory jointTrajectory)
		{
			if (this.IsTrajectryMsgCorrect(ref jointTrajectory) == false){ return; }

			this.SetTrajectoryInfoMap(ref jointTrajectory);

			TrajectoryInfo.IsOverLimitSpeed<TIAGoCommon.Joint>(this.trajectoryKeyList, this.trajectoryInfoMap, TIAGoCommon.GetMaxJointSpeed);
		}


		protected void FixedUpdate()
		{
			foreach(TIAGoCommon.Joint joint in this.trajectoryKeyList)
			{
				if (this.trajectoryInfoMap[joint] == null){ continue; }

				switch(joint)
				{
					case TIAGoCommon.Joint.torso_lift_joint:
					{
						float newPos = TrajectoryInfo.GetPositionAndUpdateTrajectory<TIAGoCommon.Joint>(this.trajectoryInfoMap, joint, GetMinJointSpeed(joint), GetMaxJointSpeed(joint));
						
						this.torsoLiftLink.localPosition = new Vector3(this.torsoLiftLink.localPosition.x, this.torsoLiftLink.localPosition.y, this.torsoLiftLinkIniPosZ + newPos);
						break;
					}
					case TIAGoCommon.Joint.head_1_joint:{ this.UpdateLinkAngle(this.head1Link, joint, 0.0f, Vector3.back); break; }
					case TIAGoCommon.Joint.head_2_joint:{ this.UpdateLinkAngle(this.head2Link, joint, 0.0f, Vector3.up); break; }

					case TIAGoCommon.Joint.arm_1_joint: { this.UpdateLinkAngle(this.arm1Link, joint, this.arm1LinkIniRotZ, Vector3.back); break; }
					case TIAGoCommon.Joint.arm_2_joint: { this.UpdateLinkAngle(this.arm2Link, joint, 0.0f,                 Vector3.up  ); break; }
					case TIAGoCommon.Joint.arm_3_joint: { this.UpdateLinkAngle(this.arm3Link, joint, this.arm3LinkIniRotZ, Vector3.back); break; }
					case TIAGoCommon.Joint.arm_4_joint: { this.UpdateLinkAngle(this.arm4Link, joint, this.arm4LinkIniRotY, Vector3.down); break; }
					case TIAGoCommon.Joint.arm_5_joint: { this.UpdateLinkAngle(this.arm5Link, joint, 0.0f,                 Vector3.back); break; }
					case TIAGoCommon.Joint.arm_6_joint: { this.UpdateLinkAngle(this.arm6Link, joint, this.arm6LinkIniRotY, Vector3.down); break; }
					case TIAGoCommon.Joint.arm_7_joint: { this.UpdateLinkAngle(this.arm7Link, joint, this.arm7LinkIniRotZ, Vector3.back); break; }

					case TIAGoCommon.Joint.gripper_left_finger_joint: { this.UpdateGripperPosition(this.gripperLeftFingerLink,  joint, this.gripperLeftFingerLinkIniPosX,  true);  break; }
					case TIAGoCommon.Joint.gripper_right_finger_joint:{ this.UpdateGripperPosition(this.gripperRightFingerLink, joint, this.gripperRightFingerLinkIniPosX, false); break; }
				}
			}
		}

		private void UpdateLinkAngle(Transform link, TIAGoCommon.Joint joint, float iniRot, Vector3 axis)
		{
			float newPosRad = TrajectoryInfo.GetPositionAndUpdateTrajectory<TIAGoCommon.Joint>(this.trajectoryInfoMap, joint, TIAGoCommon.GetMinJointSpeed(joint), TIAGoCommon.GetMaxJointSpeed(joint));

			float newPos = TIAGoCommon.GetNormalizedJointEulerAngle(newPosRad * Mathf.Rad2Deg, joint);

			TrajectoryInfo.UpdateLinkAngle(link, axis, newPos - iniRot);
		}

		private void UpdateGripperPosition(Transform link, TIAGoCommon.Joint joint, float iniPosX, bool isLeft)
		{
			float newPos = TrajectoryInfo.GetPositionAndUpdateTrajectory<TIAGoCommon.Joint>(this.trajectoryInfoMap, joint, GetMinJointSpeed(joint), GetMaxJointSpeed(joint));

			if(isLeft)
			{
				newPos = +(newPos - iniPosX);
			}
			else
			{
				newPos = -(newPos - iniPosX);
			}

			bool isGripperClosing = isLeft? (newPos < link.localPosition.x) : (newPos > link.localPosition.x);

			// Grasping and gripper closing
			if (this.graspedObject!=null && isGripperClosing)
			{
				// Have to stop
				this.trajectoryInfoMap[joint] = null;
			}
			// Otherwise
			else
			{
				link.localPosition = new Vector3(iniPosX + newPos, link.localPosition.y, link.localPosition.z);
			}
		}


		private bool IsTrajectryMsgCorrect(ref SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory msg)
		{
			for (int i = 0; i < msg.points.Count; i++)
			{
				if (msg.joint_names.Count != msg.points[i].positions.Count)
				{
					SIGVerseLogger.Warn("Trajectry count error. (joint_names.Count = " + msg.joint_names.Count + ", msg.points[" + i + "].positions.Count = " + msg.points[i].positions.Count);
					return false;
				}
			}
			
			if (msg.joint_names.Count == 1) // Torso
			{
				if (msg.joint_names.Contains(TIAGoCommon.Joint.torso_lift_joint .ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 2) // Head or Gripper
			{
				if (msg.joint_names.Contains(TIAGoCommon.Joint.head_1_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.head_2_joint.ToString()))
				{
					return true;
				}

				if (msg.joint_names.Contains(TIAGoCommon.Joint.gripper_left_finger_joint .ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.gripper_right_finger_joint.ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 7) // Arm
			{
				if (msg.joint_names.Contains(TIAGoCommon.Joint.arm_1_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_2_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_3_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_4_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_5_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_6_joint.ToString()) && 
				    msg.joint_names.Contains(TIAGoCommon.Joint.arm_7_joint.ToString()))
				{
					return true;
				}
			}

			SIGVerseLogger.Warn("Wrong joint name or points. (" + this.topicName + ")");
			return false;
		}


		private void SetTrajectoryInfoMap(ref SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory msg)
		{
			for (int i = 0; i < msg.joint_names.Count; i++)
			{
				TIAGoCommon.Joint joint = (TIAGoCommon.Joint)Enum.Parse(typeof(TIAGoCommon.Joint), msg.joint_names[i]);

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();

				for (int pointIndex = 0; pointIndex < msg.points.Count; pointIndex++)
				{
					positions.Add(TIAGoCommon.GetClampedPosition((float)msg.points[pointIndex].positions[i], joint));
					durations.Add((float)msg.points[pointIndex].time_from_start.sec + (float)msg.points[pointIndex].time_from_start.nanosec * 1.0e-9f);
				}

				switch(joint)
				{
					case TIAGoCommon.Joint.torso_lift_joint: { this.SetJointTrajectoryPosition(joint, durations, positions, +(this.torsoLiftLink.localPosition.z - this.torsoLiftLinkIniPosZ)); break; }

					case TIAGoCommon.Joint.head_1_joint: { this.SetJointTrajectoryRotation(joint, durations, positions, -this.head1Link.localEulerAngles.z); break; }
					case TIAGoCommon.Joint.head_2_joint: { this.SetJointTrajectoryRotation(joint, durations, positions, +this.head2Link.localEulerAngles.y); break; }

					case TIAGoCommon.Joint.arm_1_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm1Link.localEulerAngles.z - arm1LinkIniRotZ)); break; }
					case TIAGoCommon.Joint.arm_2_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, +(this.arm2Link.localEulerAngles.y                  )); break; }
					case TIAGoCommon.Joint.arm_3_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm3Link.localEulerAngles.z - arm3LinkIniRotZ)); break; }
					case TIAGoCommon.Joint.arm_4_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm4Link.localEulerAngles.y - arm4LinkIniRotY)); break; }
					case TIAGoCommon.Joint.arm_5_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm5Link.localEulerAngles.z                  )); break; }
					case TIAGoCommon.Joint.arm_6_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm6Link.localEulerAngles.y - arm6LinkIniRotY)); break; }
					case TIAGoCommon.Joint.arm_7_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -(this.arm7Link.localEulerAngles.z - arm7LinkIniRotZ)); break; }

					case TIAGoCommon.Joint.gripper_left_finger_joint: { this.SetJointTrajectoryPosition(joint, durations, positions, +(this.gripperLeftFingerLink .localPosition.x - gripperLeftFingerLinkIniPosX));  break; }
					case TIAGoCommon.Joint.gripper_right_finger_joint:{ this.SetJointTrajectoryPosition(joint, durations, positions, -(this.gripperRightFingerLink.localPosition.x - gripperRightFingerLinkIniPosX)); break; }
				}
			}
		}

		private void SetJointTrajectoryPosition(TIAGoCommon.Joint joint, List<float> durations, List<float> goalPositions, float currentPosition)
		{
			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, currentPosition);
		}

		private void SetJointTrajectoryRotation(TIAGoCommon.Joint joint, List<float> durations, List<float> goalPositions, float currentPosition)
		{
			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, TIAGoCommon.GetNormalizedJointEulerAngle(currentPosition, joint) * Mathf.Deg2Rad);
		}


		public void OnChangeGraspedObject(GameObject graspedObject)
		{
			this.graspedObject = graspedObject;
		}
	}
}

