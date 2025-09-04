using UnityEngine;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using SIGVerse.Common;

using JointType = SIGVerse.TurtleBot3.TurtleBot3JointInfo.JointType;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3SubJointTrajectory : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory>, ITurtleBot3GraspedObjectHandler
	{
		public class TrajectoryInfo
		{
			public float       StartTime       { get; set; }
			public List<float> Durations       { get; set; }
			public List<float> GoalPositions   { get; set; }
			public float       CurrentTime     { get; set; }
			public float       CurrentPosition { get; set; }

			public TrajectoryInfo(List<float> durations, List<float> goalPositions, float currentPosition)
			{
				this.StartTime       = Time.time;
				this.Durations       = durations;
				this.GoalPositions   = goalPositions;
				this.CurrentTime     = Time.time;
				this.CurrentPosition = currentPosition;
			}
		}

		private Transform joint1Link;
		private Transform joint2Link;
		private Transform joint3Link;
		private Transform joint4Link;
		private Transform gripJointLink;
		private Transform gripJointSubLink;

		private Dictionary<string, TrajectoryInfo> trajectoryInfoMap;
		private List<string> trajectoryKeyList;

		private GameObject graspedObject;

		void Awake()
		{
			this.joint1Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint1);
			this.joint2Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint2);
			this.joint3Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint3);
			this.joint4Link = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.Joint4);

			this.gripJointLink    = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.GripJoint);
			this.gripJointSubLink = TurtleBot3Common.FindGameObjectFromChild(this.transform.root, JointType.GripJointSub);

			this.trajectoryInfoMap = new Dictionary<string, TrajectoryInfo>();
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.Joint1], null);
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.Joint2], null);
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.Joint3], null);
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.Joint4], null);
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.GripJoint   ], null);
			this.trajectoryInfoMap.Add(TurtleBot3Common.jointNameMap[JointType.GripJointSub], null);

			this.trajectoryKeyList = new List<string>(trajectoryInfoMap.Keys);
		}


		protected override void Start()
		{
			base.Start();
			
			this.graspedObject = null;
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.msg.JointTrajectory jointTrajectory)
		{
			if(jointTrajectory.joint_names.Count != jointTrajectory.points[0].positions.Count)
			{
				SIGVerseLogger.Warn("joint_names.Count != points.positions.Count  topicName = "+this.topicName);
				return;
			}

			for(int i=0; i < jointTrajectory.joint_names.Count; i++)
			{
				string name = jointTrajectory.joint_names[i];

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();

				for(int pointIndex=0; pointIndex<jointTrajectory.points.Count; pointIndex++)
				{
					positions.Add(TurtleBot3Common.GetClampedPosition(name, (float)jointTrajectory.points[pointIndex].positions[i]));
					durations.Add((float)jointTrajectory.points[pointIndex].time_from_start.sec + (float)jointTrajectory.points[pointIndex].time_from_start.nanosec * 1.0e-9f);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.Joint1])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions.ConvertAll(p => -p), TurtleBot3Common.GetCorrectedJointsEulerAngle(name, this.joint1Link.localEulerAngles.z) * Mathf.Deg2Rad);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.Joint2])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions.ConvertAll(p => -p), TurtleBot3Common.GetCorrectedJointsEulerAngle(name, this.joint2Link.localEulerAngles.y) * Mathf.Deg2Rad);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.Joint3])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions.ConvertAll(p => -p), TurtleBot3Common.GetCorrectedJointsEulerAngle(name, this.joint3Link.localEulerAngles.y) * Mathf.Deg2Rad);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.Joint4])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions.ConvertAll(p => -p), TurtleBot3Common.GetCorrectedJointsEulerAngle(name, this.joint4Link.localEulerAngles.y) * Mathf.Deg2Rad);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.GripJoint])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions.ConvertAll(p => -p), this.gripJointLink.localPosition.y);
				}

				if(name == TurtleBot3Common.jointNameMap[JointType.GripJointSub])
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(durations, positions, -this.gripJointLink.localPosition.y);
				}
			}
		}


		protected override void Update()
		{
			base.Update();

			foreach(string jointName in this.trajectoryKeyList)
			{
				if (this.trajectoryInfoMap[jointName] != null)
				{
					if (jointName == TurtleBot3Common.jointNameMap[JointType.Joint1])
					{
						float newPos = TurtleBot3Common.GetCorrectedJointsEulerAngle(jointName, GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeedRad, TurtleBot3Common.MaxSpeedArm) * Mathf.Rad2Deg);
						
						this.joint1Link.localEulerAngles = new Vector3(this.joint1Link.localEulerAngles.x, this.joint1Link.localEulerAngles.y, newPos);
					}

					if (jointName == TurtleBot3Common.jointNameMap[JointType.Joint2])
					{
						float newPos = TurtleBot3Common.GetCorrectedJointsEulerAngle(jointName, GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeedRad, TurtleBot3Common.MaxSpeedArm) * Mathf.Rad2Deg);

						this.joint2Link.localEulerAngles = new Vector3(this.joint2Link.localEulerAngles.x, newPos, this.joint2Link.localEulerAngles.z);
					}

					if (jointName == TurtleBot3Common.jointNameMap[JointType.Joint3])
					{
						float newPos = TurtleBot3Common.GetCorrectedJointsEulerAngle(jointName, GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeedRad, TurtleBot3Common.MaxSpeedArm) * Mathf.Rad2Deg);

						this.joint3Link.localEulerAngles = new Vector3(this.joint3Link.localEulerAngles.x, newPos, this.joint3Link.localEulerAngles.z);
					}

					if (jointName == TurtleBot3Common.jointNameMap[JointType.Joint4])
					{
						float newPos = TurtleBot3Common.GetCorrectedJointsEulerAngle(jointName, GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeedRad, TurtleBot3Common.MaxSpeedArm) * Mathf.Rad2Deg);

						this.joint4Link.localEulerAngles = new Vector3(this.joint4Link.localEulerAngles.x, newPos, this.joint4Link.localEulerAngles.z);
					}

					if (jointName == TurtleBot3Common.jointNameMap[JointType.GripJoint])
					{
						float newPos = GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeed, TurtleBot3Common.MaxSpeedHand);

						// Grasping and hand closing
						if(this.graspedObject!=null && newPos < this.gripJointLink.localPosition.y)
						{
							// Have to stop
							this.trajectoryInfoMap[jointName] = null;
						}
						// Otherwise
						else
						{
							this.gripJointLink.localPosition = new Vector3(this.gripJointLink.localPosition.x, newPos, this.gripJointLink.localPosition.z);
						}
					}

					if (jointName == TurtleBot3Common.jointNameMap[JointType.GripJointSub])
					{
						float newPos = GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, TurtleBot3Common.MinSpeed, TurtleBot3Common.MaxSpeedHand);

						// Grasping and hand closing
						if(this.graspedObject!=null && newPos > this.gripJointSubLink.localPosition.y)
						{
							// Have to stop
							this.trajectoryInfoMap[jointName] = null;
						}
						// Otherwise
						else
						{
							this.gripJointSubLink.localPosition = new Vector3(this.gripJointSubLink.localPosition.x, newPos, this.gripJointSubLink.localPosition.z);
						}
					}
				}
			}

			this.webSocketConnection.Render();
		}


		public static float GetPositionAndUpdateTrajectory(Dictionary<string, TrajectoryInfo> trajectoryInfoMap, string jointName, float minSpeed, float maxSpeed)
		{
			TrajectoryInfo trajectoryInfo = trajectoryInfoMap[jointName];

			int targetPointIndex = GetTargetPointIndex(trajectoryInfo);

			// Calculate move speed
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
				trajectoryInfoMap[jointName] = null;
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

		public void OnChangeGraspedObject(GameObject graspedObject)
		{
			this.graspedObject = graspedObject;
		}
	}
}
