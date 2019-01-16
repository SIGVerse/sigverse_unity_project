using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;
using static SIGVerse.ToyotaHSR.HSRCommon;

namespace SIGVerse.ToyotaHSR
{
	public class HSRSubJointTrajectory : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.JointTrajectory>, IGraspedObjectHandler
	{
		public class TrajectoryInfo
		{
			public float       StartTime       { get; set; }
			public List<float> Durations       { get; set; }
			public List<float> GoalPositions   { get; set; }
			public float       CurrentTime     { get; set; }
			public float       CurrentPosition { get; set; }

			public TrajectoryInfo(float startTime, List<float> duration, List<float> goalPosition, float currentTime, float currentPosition)
			{
				this.StartTime       = startTime;
				this.Durations       = duration;
				this.GoalPositions   = goalPosition;
				this.CurrentTime     = currentTime;
				this.CurrentPosition = currentPosition;
			}

			public TrajectoryInfo(List<float> duration, List<float> goalPosition, float currentPosition)
			{
				this.StartTime       = Time.time;
				this.Durations       = duration;
				this.GoalPositions   = goalPosition;
				this.CurrentTime     = Time.time;
				this.CurrentPosition = currentPosition;
			}
		}

		private Transform armLiftLink;
		private Transform armFlexLink;
		private Transform armRollLink;
		private Transform wristFlexLink;
		private Transform wristRollLink;
		private Transform headPanLink;
		private Transform headTiltLink;
		private Transform torsoLiftLink;
		private Transform handMotorDummyLink;
		private Transform handLProximalLink;
		private Transform handRProximalLink;
		private Transform handLDistalLink;
		private Transform handRDistalLink;

		private float armLiftLinkIniPosZ;
		private float torsoLiftLinkIniPosZ;

		private Dictionary<HSRCommon.Joint, TrajectoryInfo> trajectoryInfoMap;
		private List<HSRCommon.Joint> trajectoryKeyList;

		private GameObject graspedObject;


		void Awake()
		{
			this.armLiftLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_lift_link        .ToString());
			this.armFlexLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_flex_link        .ToString());
			this.armRollLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_roll_link        .ToString());
			this.wristFlexLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_flex_link      .ToString());
			this.wristRollLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_roll_link      .ToString());
			this.headPanLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link        .ToString());
			this.headTiltLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link       .ToString());
			this.torsoLiftLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link      .ToString());
			this.handMotorDummyLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_motor_dummy_link.ToString());
			this.handLProximalLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_proximal_link .ToString());
			this.handRProximalLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_proximal_link .ToString());
			this.handLDistalLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_distal_link   .ToString());
			this.handRDistalLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_distal_link   .ToString());

			this.armLiftLinkIniPosZ   = this.armLiftLink.localPosition.z;
			this.torsoLiftLinkIniPosZ = this.torsoLiftLink.localPosition.z;

			this.trajectoryInfoMap = new Dictionary<HSRCommon.Joint, TrajectoryInfo>()
			{
				{ HSRCommon.Joint.arm_lift_joint  , null },
				{ HSRCommon.Joint.arm_flex_joint  , null },
				{ HSRCommon.Joint.arm_roll_joint  , null },
				{ HSRCommon.Joint.wrist_flex_joint, null },
				{ HSRCommon.Joint.wrist_roll_joint, null },
				{ HSRCommon.Joint.head_pan_joint  , null },
				{ HSRCommon.Joint.head_tilt_joint , null },
				{ HSRCommon.Joint.hand_motor_joint, null },
			};

			this.trajectoryKeyList = new List<HSRCommon.Joint>(trajectoryInfoMap.Keys);
		}


		protected override void Start()
		{
			base.Start();
			
			this.graspedObject = null;
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{
			if (this.IsTrajectryMsgCorrect(ref jointTrajectory) == false){ return; }

			this.SetTrajectoryInfoMap(ref jointTrajectory);

			this.CheckOverLimitSpeed();
		}


		protected void FixedUpdate()
		{
			foreach(HSRCommon.Joint joint in this.trajectoryKeyList)
			{
				if (this.trajectoryInfoMap[joint] == null){ continue; }

				switch(joint)
				{
					case HSRCommon.Joint.arm_lift_joint:
					{
						float newPos = GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint);
						
						this.armLiftLink  .localPosition = new Vector3(this.armLiftLink  .localPosition.x, this.armLiftLink.localPosition.y,   this.armLiftLinkIniPosZ   + newPos);
						this.torsoLiftLink.localPosition = new Vector3(this.torsoLiftLink.localPosition.x, this.torsoLiftLink.localPosition.y, this.torsoLiftLinkIniPosZ + newPos / 2.0f );
						break;
					}
					case HSRCommon.Joint.arm_flex_joint:   { this.UpdateLinkAngle(this.armFlexLink,   joint, Vector3.up);   break; }
					case HSRCommon.Joint.arm_roll_joint:   { this.UpdateLinkAngle(this.armRollLink,   joint, Vector3.back); break; }
					case HSRCommon.Joint.wrist_flex_joint: { this.UpdateLinkAngle(this.wristFlexLink, joint, Vector3.up);   break; }
					case HSRCommon.Joint.wrist_roll_joint: { this.UpdateLinkAngle(this.wristRollLink, joint, Vector3.back); break; }
					case HSRCommon.Joint.head_pan_joint:   { this.UpdateLinkAngle(this.headPanLink,   joint, Vector3.back); break; }
					case HSRCommon.Joint.head_tilt_joint:  { this.UpdateLinkAngle(this.headTiltLink,  joint, Vector3.up);   break; }
					case HSRCommon.Joint.hand_motor_joint:
					{
						float newPos = HSRCommon.GetNormalizedJointEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint) * Mathf.Rad2Deg, joint);
						
						// Grasping and hand closing
						if (this.graspedObject!=null && this.IsAngleDecreasing(newPos, this.handMotorDummyLink.localEulerAngles.x))
						{
							// Have to stop
							this.trajectoryInfoMap[joint] = null;
						}
						// Otherwise
						else
						{
							this.handMotorDummyLink.localEulerAngles = new Vector3(+newPos, this.handMotorDummyLink.localEulerAngles.y, this.handMotorDummyLink.localEulerAngles.z);
							this.handLProximalLink .localEulerAngles = new Vector3(+newPos, this.handLProximalLink .localEulerAngles.y, this.handLProximalLink .localEulerAngles.z);
							this.handRProximalLink .localEulerAngles = new Vector3(-newPos, this.handRProximalLink .localEulerAngles.y, this.handRProximalLink .localEulerAngles.z);
							this.handLDistalLink   .localEulerAngles = new Vector3(-newPos, this.handLDistalLink   .localEulerAngles.y, this.handLDistalLink   .localEulerAngles.z);
							this.handRDistalLink   .localEulerAngles = new Vector3(+newPos, this.handRDistalLink   .localEulerAngles.y, this.handRDistalLink   .localEulerAngles.z);
						}
						break;
					}
				}
			}
		}

		private void UpdateLinkAngle(Transform link, HSRCommon.Joint joint, Vector3 axis)
		{
			float newPos = HSRCommon.GetNormalizedJointEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint) * Mathf.Rad2Deg, joint);

			if(Mathf.Abs(axis.x)==1)
			{
				link.localEulerAngles = new Vector3(0.0f, link.localEulerAngles.y, link.localEulerAngles.z) + newPos * axis;
			}
			else if(Mathf.Abs(axis.y)==1)
			{
				link.localEulerAngles = new Vector3(link.localEulerAngles.x, 0.0f, link.localEulerAngles.z) + newPos * axis;
			}
			else if(Mathf.Abs(axis.z)==1)
			{
				link.localEulerAngles = new Vector3(link.localEulerAngles.x, link.localEulerAngles.y, 0.0f) + newPos * axis;
			}
		}


		private static float GetPositionAndUpdateTrajectory(Dictionary<HSRCommon.Joint, TrajectoryInfo> trajectoryInfoMap, HSRCommon.Joint joint)
		{
			float minSpeed = HSRCommon.GetMinJointSpeed(joint);
			float maxSpeed = HSRCommon.GetMaxJointSpeed(joint);

			TrajectoryInfo trajectoryInfo = trajectoryInfoMap[joint];

			int targetPointIndex = GetTargetPointIndex(trajectoryInfo);

			float speed = 0.0f;

			if (trajectoryInfo.CurrentTime - trajectoryInfo.StartTime >= trajectoryInfo.Durations[targetPointIndex])
			{
				speed = maxSpeed;
			}
			else
			{
				speed = Mathf.Abs((trajectoryInfo.GoalPositions[targetPointIndex] - trajectoryInfo.CurrentPosition) / (trajectoryInfo.Durations[targetPointIndex] - (trajectoryInfo.CurrentTime - trajectoryInfo.StartTime)));
				speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
			}

			// Calculate position
			float newPosition;
			float movingDistance = speed * (Time.time - trajectoryInfo.CurrentTime);

			if (movingDistance > Mathf.Abs(trajectoryInfo.GoalPositions[targetPointIndex] - trajectoryInfo.CurrentPosition))
			{
				newPosition = trajectoryInfo.GoalPositions[targetPointIndex];
				trajectoryInfoMap[joint] = null;
			}
			else
			{
				trajectoryInfo.CurrentTime = Time.time;

				if (trajectoryInfo.GoalPositions[targetPointIndex] > trajectoryInfo.CurrentPosition)
				{
					trajectoryInfo.CurrentPosition = trajectoryInfo.CurrentPosition + movingDistance;
					newPosition = trajectoryInfo.CurrentPosition;
				}
				else
				{
					trajectoryInfo.CurrentPosition = trajectoryInfo.CurrentPosition - movingDistance;
					newPosition = trajectoryInfo.CurrentPosition;
				}
			}

			return newPosition;
		}


		private bool IsTrajectryMsgCorrect(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
		{
			for (int i = 0; i < msg.points.Count; i++)
			{
				if (msg.joint_names.Count != msg.points[i].positions.Count)
				{
					SIGVerseLogger.Warn("Trajectry count error. (joint_names.Count = " + msg.joint_names.Count + ", msg.points[" + i + "].positions.Count = " + msg.points[i].positions.Count);
					return false;
				}
			}
			
			if (msg.joint_names.Count == 2) // Head
			{
				if (msg.joint_names.Contains(HSRCommon.Joint.head_pan_joint .ToString()) && 
				    msg.joint_names.Contains(HSRCommon.Joint.head_tilt_joint.ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 5) // Arm
			{
				if (msg.joint_names.Contains(HSRCommon.Joint.wrist_flex_joint.ToString()) && 
				    msg.joint_names.Contains(HSRCommon.Joint.wrist_roll_joint.ToString()) && 
				    msg.joint_names.Contains(HSRCommon.Joint.arm_lift_joint  .ToString()) && 
				    msg.joint_names.Contains(HSRCommon.Joint.arm_flex_joint  .ToString()) && 
				    msg.joint_names.Contains(HSRCommon.Joint.arm_roll_joint  .ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 1) // Hand
			{
				if (msg.joint_names.Contains(HSRCommon.Joint.hand_motor_joint.ToString()))
				{
					return true;
				}
			}

			SIGVerseLogger.Warn("Wrong joint name or points. (" + this.topicName + ")");
			return false;
		}


		private void SetTrajectoryInfoMap(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
		{
			for (int i = 0; i < msg.joint_names.Count; i++)
			{
				HSRCommon.Joint joint = (HSRCommon.Joint)Enum.Parse(typeof(HSRCommon.Joint), msg.joint_names[i]);

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();

				for (int pointIndex = 0; pointIndex < msg.points.Count; pointIndex++)
				{
					positions.Add(HSRCommon.GetClampedPosition((float)msg.points[pointIndex].positions[i], joint));
					durations.Add((float)msg.points[pointIndex].time_from_start.secs + (float)msg.points[pointIndex].time_from_start.nsecs * 1.0e-9f);
				}

				switch(joint)
				{
					case HSRCommon.Joint.arm_lift_joint: { this.SetJointTrajectoryPosition(joint, durations, positions, +this.armLiftLink.localPosition.z - this.armLiftLinkIniPosZ); break; }

					case HSRCommon.Joint.arm_flex_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, +this.armFlexLink       .localEulerAngles.y); break; }
					case HSRCommon.Joint.arm_roll_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -this.armRollLink       .localEulerAngles.z); break; }
					case HSRCommon.Joint.wrist_flex_joint:{ this.SetJointTrajectoryRotation(joint, durations, positions, +this.wristFlexLink     .localEulerAngles.y); break; }
					case HSRCommon.Joint.wrist_roll_joint:{ this.SetJointTrajectoryRotation(joint, durations, positions, -this.wristRollLink     .localEulerAngles.z); break; }
					case HSRCommon.Joint.head_pan_joint:  { this.SetJointTrajectoryRotation(joint, durations, positions, -this.headPanLink       .localEulerAngles.z); break; }
					case HSRCommon.Joint.head_tilt_joint: { this.SetJointTrajectoryRotation(joint, durations, positions, +this.headTiltLink      .localEulerAngles.y); break; }
					case HSRCommon.Joint.hand_motor_joint:{ this.SetJointTrajectoryRotation(joint, durations, positions, +this.handMotorDummyLink.localEulerAngles.x); break; }
				}
			}
		}

		private void SetJointTrajectoryPosition(HSRCommon.Joint joint, List<float> durations, List<float> goalPositions, float currentPosition)
		{
			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, currentPosition);
		}

		private void SetJointTrajectoryRotation(HSRCommon.Joint joint, List<float> durations, List<float> goalPositions, float currentPosition)
		{
			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, HSRCommon.GetNormalizedJointEulerAngle(currentPosition, joint) * Mathf.Deg2Rad);
		}


		private void CheckOverLimitSpeed()
		{
			foreach (HSRCommon.Joint joint in this.trajectoryKeyList)
			{
				if (this.trajectoryInfoMap[joint] == null) { continue; }

				List<float> trajectoryInfoDurations     = new List<float>(this.trajectoryInfoMap[joint].Durations);
				List<float> trajectoryInfoGoalPositions = new List<float>(this.trajectoryInfoMap[joint].GoalPositions);

				trajectoryInfoDurations    .Insert(0, 0.0f);
				trajectoryInfoGoalPositions.Insert(0, this.trajectoryInfoMap[joint].CurrentPosition);

				for (int i = 1; i < trajectoryInfoGoalPositions.Count; i++)
				{
					double tempDistance  = Math.Abs(trajectoryInfoGoalPositions[i] - trajectoryInfoGoalPositions[i-1]);
					double tempDurations = Math.Abs(trajectoryInfoDurations    [i] - trajectoryInfoDurations[i-1]);
					double tempSpeed     = tempDistance / tempDurations;
					
					if(IsOverLimitSpeed(joint, tempSpeed))
					{
						SIGVerseLogger.Warn("Trajectry speed error. (" + this.topicName + ")");
						return;
					}
				}
			}
		}

		private static bool IsOverLimitSpeed(HSRCommon.Joint joint, double speed)
		{
			return speed > HSRCommon.GetMaxJointSpeed(joint);
		}

		private static int GetTargetPointIndex(TrajectoryInfo trajectoryInfo)
		{
			int targetPointIndex = 0;

			for (int i = 0; i < trajectoryInfo.Durations.Count; i++)
			{
				targetPointIndex = i;

				if (Time.time - trajectoryInfo.StartTime < trajectoryInfo.Durations[targetPointIndex])
				{
					break;
				}
			}
			return targetPointIndex;
		}


		private bool IsAngleDecreasing(float newVal, float oldVal)
		{
			float angleDiff = this.GetAngleDiff(newVal, oldVal);

			if(angleDiff==0.0f) { return false; }

			if(angleDiff < 0.0f)
			{
				return Mathf.Abs(angleDiff) < 180;
			}
			else
			{
				return Mathf.Abs(angleDiff) > 180;
			}
		}

		private float GetAngleDiff(float newVal, float oldVal)
		{
			newVal = (newVal < 0)? newVal+360 : newVal;
			oldVal = (oldVal < 0)? oldVal+360 : oldVal;
			return newVal - oldVal;
		}

		public void OnChangeGraspedObject(GameObject graspedObject)
		{
			this.graspedObject = graspedObject;
		}
	}
}

