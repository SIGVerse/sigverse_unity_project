using System;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using Joint = SIGVerse.ToyotaHSR.HSRCommon.Joint;
using Link  = SIGVerse.ToyotaHSR.HSRCommon.Link;

namespace SIGVerse.ToyotaHSR
{
	public class HSRSubJointTrajectory : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory>, IGraspedObjectHandler
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

		private Dictionary<Joint, Transform> linkMap = new Dictionary<Joint, Transform>();

		private float armLiftLinkIniPosZ;
		private float torsoLiftLinkIniPosZ;

		private Dictionary<Joint, TrajectoryInfo> trajectoryInfoMap;
		private List<Joint> trajectoryKeyList;

		private GameObject graspedObject;


		void Awake()
		{
			this.linkMap[Joint.arm_lift_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_lift_link        .ToString());
			this.linkMap[Joint.arm_flex_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_flex_link        .ToString());
			this.linkMap[Joint.arm_roll_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_roll_link        .ToString());
			this.linkMap[Joint.wrist_flex_joint]      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_flex_link      .ToString());
			this.linkMap[Joint.wrist_roll_joint]      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_roll_link      .ToString());
			this.linkMap[Joint.head_pan_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link        .ToString());
			this.linkMap[Joint.head_tilt_joint]       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link       .ToString());
			this.linkMap[Joint.torso_lift_joint]      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link      .ToString());
			this.linkMap[Joint.hand_motor_joint]      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_motor_dummy_link.ToString());
			this.linkMap[Joint.hand_l_proximal_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_proximal_link .ToString());
			this.linkMap[Joint.hand_l_distal_joint]   = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_distal_link   .ToString());
			this.linkMap[Joint.hand_r_proximal_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_proximal_link .ToString());
			this.linkMap[Joint.hand_r_distal_joint]   = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_distal_link   .ToString());

			this.armLiftLinkIniPosZ   = this.linkMap[Joint.arm_lift_joint]  .localPosition.z * HSRCommon.JointAxis[Joint.arm_lift_joint]  .z;
			this.torsoLiftLinkIniPosZ = this.linkMap[Joint.torso_lift_joint].localPosition.z * HSRCommon.JointAxis[Joint.torso_lift_joint].z;

			this.trajectoryInfoMap = new Dictionary<Joint, TrajectoryInfo>()
			{
				{ Joint.arm_lift_joint  , null },
				{ Joint.arm_flex_joint  , null },
				{ Joint.arm_roll_joint  , null },
				{ Joint.wrist_flex_joint, null },
				{ Joint.wrist_roll_joint, null },
				{ Joint.head_pan_joint  , null },
				{ Joint.head_tilt_joint , null },
				{ Joint.hand_motor_joint, null },
			};

			this.trajectoryKeyList = new List<Joint>(trajectoryInfoMap.Keys);
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

			this.CheckOverLimitSpeed();
		}


		protected void FixedUpdate()
		{
			foreach(Joint joint in this.trajectoryKeyList)
			{
				if (this.trajectoryInfoMap[joint] == null){ continue; }

				switch(joint)
				{
					case Joint.arm_lift_joint:
					{
						float newPos = GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint);
						
						Transform armLift   = this.linkMap[Joint.arm_lift_joint];
						Transform torsoLift = this.linkMap[Joint.torso_lift_joint];

						armLift  .localPosition = new Vector3(armLift  .localPosition.x, armLift  .localPosition.y, armLiftLinkIniPosZ   + newPos);
						torsoLift.localPosition = new Vector3(torsoLift.localPosition.x, torsoLift.localPosition.y, torsoLiftLinkIniPosZ + newPos / 2.0f );
						break;
					}
					case Joint.arm_flex_joint:
					case Joint.arm_roll_joint:
					case Joint.wrist_flex_joint:
					case Joint.wrist_roll_joint:
					case Joint.head_pan_joint:
					case Joint.head_tilt_joint: { this.UpdateLinkAngle(joint); break; }
					case Joint.hand_motor_joint:
					{
						float newPos = HSRCommon.GetNormalizedJointEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint) * Mathf.Rad2Deg, joint);
						
						// Grasping and hand closing
						if (this.graspedObject!=null && this.IsAngleDecreasing(newPos, this.linkMap[Joint.hand_motor_joint].localEulerAngles.x))
						{
							// Have to stop
							this.trajectoryInfoMap[joint] = null;
						}
						// Otherwise
						else
						{
							this.UpdateLinkAngle(Joint.hand_motor_joint,      +newPos);
							this.UpdateLinkAngle(Joint.hand_l_proximal_joint, +newPos);
							this.UpdateLinkAngle(Joint.hand_l_distal_joint,   -newPos);
							this.UpdateLinkAngle(Joint.hand_r_proximal_joint, +newPos);
							this.UpdateLinkAngle(Joint.hand_r_distal_joint,   -newPos);
						}
						break;
					}
				}
			}
		}

		private void UpdateLinkAngle(Joint joint)
		{
			float newPos = HSRCommon.GetNormalizedJointEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, joint) * Mathf.Rad2Deg, joint);

			this.UpdateLinkAngle(joint, newPos);
		}

		private void UpdateLinkAngle(Joint joint, float newPos)
		{
			Transform link = this.linkMap[joint];
			Vector3 axis = HSRCommon.JointAxis[joint];

			link.localEulerAngles = newPos * axis; // Values on other axes are 0
		}


		private static float GetPositionAndUpdateTrajectory(Dictionary<Joint, TrajectoryInfo> trajectoryInfoMap, Joint joint)
		{
			float minSpeed = HSRCommon.GetMinJointSpeed(joint);
			float maxSpeed = HSRCommon.GetMaxJointSpeed(joint);

			TrajectoryInfo trajectoryInfo = trajectoryInfoMap[joint];

			int targetPointIndex = GetTargetPointIndex(trajectoryInfo);

			float speed;

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
			
			if (msg.joint_names.Count == 2) // Head
			{
				if (msg.joint_names.Contains(Joint.head_pan_joint .ToString()) && 
				    msg.joint_names.Contains(Joint.head_tilt_joint.ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 5) // Arm
			{
				if (msg.joint_names.Contains(Joint.wrist_flex_joint.ToString()) && 
				    msg.joint_names.Contains(Joint.wrist_roll_joint.ToString()) && 
				    msg.joint_names.Contains(Joint.arm_lift_joint  .ToString()) && 
				    msg.joint_names.Contains(Joint.arm_flex_joint  .ToString()) && 
				    msg.joint_names.Contains(Joint.arm_roll_joint  .ToString()))
				{
					return true;
				}
			}
			else if (msg.joint_names.Count == 1) // Hand
			{
				if (msg.joint_names.Contains(Joint.hand_motor_joint.ToString()))
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
				Joint joint = (Joint)Enum.Parse(typeof(Joint), msg.joint_names[i]);

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();

				for (int pointIndex = 0; pointIndex < msg.points.Count; pointIndex++)
				{
					positions.Add(HSRCommon.GetClampedPosition((float)msg.points[pointIndex].positions[i], joint));
					durations.Add((float)msg.points[pointIndex].time_from_start.sec + (float)msg.points[pointIndex].time_from_start.nanosec * 1.0e-9f);
				}

				switch(joint)
				{
					case Joint.arm_lift_joint: { this.SetJointTrajectoryPosition(joint, durations, positions, this.linkMap[Joint.arm_lift_joint].localPosition.z - this.armLiftLinkIniPosZ); break; }

					case Joint.arm_flex_joint:
					case Joint.arm_roll_joint:
					case Joint.wrist_flex_joint:
					case Joint.wrist_roll_joint:
					case Joint.head_pan_joint:
					case Joint.head_tilt_joint:
					case Joint.hand_motor_joint:{ this.SetJointTrajectoryRotation(joint, durations, positions); break; }
				}
			}
		}

		private void SetJointTrajectoryPosition(Joint joint, List<float> durations, List<float> goalPositions, float currentPosition)
		{
			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, currentPosition);
		}

		private void SetJointTrajectoryRotation(Joint joint, List<float> durations, List<float> goalPositions)
		{
			float currentPosition = GetEulerAngle(joint);

			this.trajectoryInfoMap[joint] = new TrajectoryInfo(durations, goalPositions, HSRCommon.GetNormalizedJointEulerAngle(currentPosition, joint) * Mathf.Deg2Rad);
		}

		private float GetEulerAngle(Joint joint)
		{
			Transform link = this.linkMap[joint];
			Vector3 axis = HSRCommon.JointAxis[joint];

			if(Mathf.Abs(axis.x)==1)
			{
				return link.localEulerAngles.x * axis.x;
			}
			else if(Mathf.Abs(axis.y)==1)
			{
				return link.localEulerAngles.y * axis.y;
			}
			else if(Mathf.Abs(axis.z)==1)
			{
				return link.localEulerAngles.z * axis.z;
			}
			else
			{
				Debug.Log("Illegal JointAxis="+HSRCommon.JointAxis[joint]);
				throw new Exception("Illegal JointAxis="+HSRCommon.JointAxis[joint]);
			}
		}


		private void CheckOverLimitSpeed()
		{
			foreach (Joint joint in this.trajectoryKeyList)
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
						SIGVerseLogger.Warn("Trajectry speed error. (" + this.topicName + ", Joint Name=" + joint.ToString() + ", Speed=" + tempSpeed + ", Max Speed=" + HSRCommon.GetMaxJointSpeed(joint) + ")");
						return;
					}
				}
			}
		}

		private static bool IsOverLimitSpeed(Joint joint, double speed)
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

