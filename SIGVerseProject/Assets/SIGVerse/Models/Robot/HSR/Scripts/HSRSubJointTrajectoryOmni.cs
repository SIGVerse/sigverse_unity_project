using UnityEngine;
using System.Collections;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;

namespace SIGVerse.ToyotaHSR
{
	public class HSRSubJointTrajectoryOmni : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.JointTrajectory>
	{
		public class TrajectoryInfo
		{
			public float StartTime    { get; set; }
			public List<float> Durations { get; set; }
			public List<float> GoalPositions { get; set; }
			public float CurrentTime     { get; set; }
			public float CurrentPosition { get; set; }

			public TrajectoryInfo(float startTime, List<float> duration, List<float> goalPosition, float currentTime, float currentPosition)
			{
				this.StartTime       = startTime;
				this.Durations       = duration;
				this.GoalPositions   = goalPosition;
				this.CurrentTime     = currentTime;
				this.CurrentPosition = currentPosition;
			}
		}

		private Transform baseFootprint;
		private Transform baseFootprintRigidbody;
		private Transform baseFootprintPosNoise;
		private Transform baseFootprintRotNoise;
		
		private Dictionary<string, TrajectoryInfo> trajectoryInfoMap;
		private List<string> trajectoryKeyList;

		private Vector3 initialPosition = new Vector3();
		private Quaternion initialRotation = new Quaternion();
		private Vector3 startPosition = new Vector3();
		private Quaternion startRotation = new Quaternion();

		int targetPointIndex = 0;
		float progressTimeRatio = 0.0f;

		void Awake()
		{
			this.trajectoryInfoMap = new Dictionary<string, TrajectoryInfo>();
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomXJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomYJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomTJointName, null);

			this.baseFootprint          = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);
			this.baseFootprintRigidbody = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRigidbodyName);
			this.baseFootprintPosNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintPosNoiseName);
			this.baseFootprintRotNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRotNoiseName);
			
			this.initialPosition = this.baseFootprint.position;
			this.initialRotation = this.baseFootprint.rotation;
		}


		protected override void Start()
		{
			base.Start();
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{
			this.startPosition = this.baseFootprint.position;
			this.startRotation = this.baseFootprint.rotation;

			if(this.IsTrajectryMsgCorrect(ref jointTrajectory) == false){ return; }
			
			this.SetTrajectoryInfoMap(ref jointTrajectory);
			this.StopJointIfOverLimitSpeed();
		}


		protected override void Update()
		{
			base.Update();
			
			if (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] != null)
			{
				this.UpdateTargetPointIndex();
				this.UpdateProgressTimeRatio();
				this.UpdateStartPosition();

				if (this.progressTimeRatio > 1 && targetPointIndex == (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions.Count-1))
				{
					this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
					return;
				}

				this.UpdateOmniPosition();
				this.UpdateOmniRotation();
			}
		}


		private void UpdateOmniPosition()
		{
			TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
			
			Vector3 goalPosition = new Vector3();
			goalPosition.x = -trajectoryInfoY.GoalPositions[targetPointIndex] + this.initialPosition.x;
			goalPosition.z = trajectoryInfoX.GoalPositions[targetPointIndex] + this.initialPosition.z;

			Vector3 newPosition = Vector3.Slerp(this.startPosition, goalPosition, progressTimeRatio);
			Vector3 deltaPosition = (newPosition - this.baseFootprint.position);
			Vector3 deltaNoisePos = new Vector3();
			deltaNoisePos.x = this.GetPosNoise(deltaPosition.x);
			deltaNoisePos.z = this.GetPosNoise(deltaPosition.z);
			
			//update position
			this.baseFootprintRigidbody.position += deltaPosition;
			this.baseFootprintPosNoise.position += deltaNoisePos;
			//trajectoryInfoMap[HSRCommon.OmniOdomXJointName].CurrentPosition = deltaPosition.z;
			//trajectoryInfoMap[HSRCommon.OmniOdomYJointName].CurrentPosition = deltaPosition.x;
			//trajectoryInfoMap[HSRCommon.OmniOdomXJointName].CurrentTime = Time.time;
			//trajectoryInfoMap[HSRCommon.OmniOdomYJointName].CurrentTime = Time.time;
		}


		private void UpdateOmniRotation()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
			Quaternion goalRotation = Quaternion.Euler(new Vector3(this.initialRotation.eulerAngles.x, this.initialRotation.eulerAngles.y, (trajectoryInfo.GoalPositions[this.targetPointIndex] * Mathf.Rad2Deg) + this.initialRotation.eulerAngles.z));
			Quaternion newRotation = Quaternion.Slerp(this.startRotation, goalRotation, this.progressTimeRatio);
			float deltaAngleDeg = (newRotation.eulerAngles.y - this.baseFootprint.rotation.eulerAngles.y);

			if (deltaAngleDeg > 180) { deltaAngleDeg -= 360; }
			else if(deltaAngleDeg < -180) { deltaAngleDeg += 360; }
						
			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, 0, deltaAngleDeg));
			Quaternion deltaNoiseRot = Quaternion.Euler(new Vector3(0, 0, this.GetRotNoise(deltaAngleDeg)));

			//update rotation.
			this.baseFootprintRigidbody.rotation *= deltaRotation;
			this.baseFootprintRotNoise.rotation *= deltaNoiseRot;
			//trajectoryInfoMap[HSRCommon.OmniOdomTJointName].CurrentTime = Time.time;
			//trajectoryInfoMap[HSRCommon.OmniOdomTJointName].CurrentPosition = Rotation.eulerAngles.z;
		}


		private void UpdateTargetPointIndex()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			int tempIndex = 0;

			for (int i = 0; i < trajectoryInfo.Durations.Count; i++)
			{
				tempIndex = i;
				if (Time.time - trajectoryInfo.StartTime < trajectoryInfo.Durations[tempIndex]) { break; }
			}

			this.targetPointIndex = tempIndex;
		}


		private void UpdateProgressTimeRatio()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			if (this.targetPointIndex == 0)
			{
				this.progressTimeRatio = (Time.time - trajectoryInfo.StartTime) / (trajectoryInfo.Durations[this.targetPointIndex]);
			}
			else
			{
				this.progressTimeRatio = (Time.time - (trajectoryInfo.StartTime + trajectoryInfo.Durations[this.targetPointIndex-1])) / (trajectoryInfo.Durations[this.targetPointIndex] - trajectoryInfo.Durations[this.targetPointIndex-1]);
			}
		}


		private void UpdateStartPosition()
		{
			if(this.targetPointIndex != 0)
			{
				TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
				TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
				TrajectoryInfo trajectoryInfoT = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
				this.startPosition = new Vector3(-trajectoryInfoY.GoalPositions[this.targetPointIndex-1] + this.initialPosition.x, 0, trajectoryInfoX.GoalPositions[this.targetPointIndex-1] + this.initialPosition.z);
				this.startRotation = Quaternion.Euler(new Vector3(this.initialRotation.eulerAngles.x, this.initialRotation.eulerAngles.y, (trajectoryInfoT.GoalPositions[this.targetPointIndex-1] * Mathf.Rad2Deg) + this.initialRotation.eulerAngles.z));
			}
		}


		private bool IsTrajectryMsgCorrect(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
		{
			for(int i = 0; i < msg.points.Count; i++)
			{
				if (msg.joint_names.Count != msg.points[i].positions.Count) { return false; }
			}

			if (msg.joint_names.Count == 3)//Omni
			{
				if (msg.joint_names.Contains(HSRCommon.OmniOdomXJointName) && msg.joint_names.Contains(HSRCommon.OmniOdomYJointName) && msg.joint_names.Contains(HSRCommon.OmniOdomTJointName))
				{
					return true;
				}
			}
			return false;
		}


		private void SetTrajectoryInfoMap(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
		{
			for (int i = 0; i < msg.joint_names.Count; i++)
			{
				string name = msg.joint_names[i];

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();

				for (int pointIndex = 0; pointIndex < msg.points.Count; pointIndex++)
				{
					positions.Add(HSRCommon.GetClampedPosition((float)msg.points[pointIndex].positions[i], name));
					durations.Add((float)msg.points[pointIndex].time_from_start.secs + (float)msg.points[pointIndex].time_from_start.nsecs * 1.0e-9f);
				}

				if (name == HSRCommon.OmniOdomXJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, 0.0f);
				}

				if (name == HSRCommon.OmniOdomYJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, 0.0f);
				}

				if (name == HSRCommon.OmniOdomTJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, 0.0f);
				}
			}
		}


		private bool StopJointIfOverLimitSpeed()
		{
			TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
			TrajectoryInfo trajectoryInfoT = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
			trajectoryInfoX.GoalPositions.Insert(0, this.startPosition.z - this.initialPosition.z);
			trajectoryInfoY.GoalPositions.Insert(0, this.startPosition.x - this.initialPosition.x);
			trajectoryInfoT.GoalPositions.Insert(0, (this.startRotation.eulerAngles.y * Mathf.Deg2Rad));
			trajectoryInfoX.Durations.Insert(0, 0.0f);
			trajectoryInfoY.Durations.Insert(0, 0.0f);
			trajectoryInfoT.Durations.Insert(0, 0.0f);
			
			for (int i = 1; i < trajectoryInfoX.GoalPositions.Count; i++)
			{
                double deltaTime = (trajectoryInfoX.Durations[i] - trajectoryInfoX.Durations[i-1]);
                double deltaDistance = Math.Sqrt(Math.Pow(trajectoryInfoX.GoalPositions[i] - trajectoryInfoX.GoalPositions[i-1], 2) + Math.Pow(trajectoryInfoY.GoalPositions[i] - trajectoryInfoY.GoalPositions[i-1], 2));
                double deltaAngle = Math.Abs(trajectoryInfoT.GoalPositions[i] - trajectoryInfoT.GoalPositions[i - 1]);
                if (deltaAngle < -Math.PI) { deltaAngle += (2 * Math.PI); }
                if (deltaAngle > Math.PI) { deltaAngle -= (2 * Math.PI); }
                
                double linearSpeed = deltaDistance / deltaTime;            
                double angularSpeed = deltaAngle / deltaTime;

                if (linearSpeed > HSRCommon.MaxSpeedBase || angularSpeed > HSRCommon.MaxSpeedBaseRad)
				{
					trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
					trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
					trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
					SIGVerseLogger.Warn("Omni speed error. (linear_speed = " + linearSpeed + ", angular_speed = " + angularSpeed);
					return true;
				}
			}

			trajectoryInfoX.GoalPositions.RemoveAt(0);
			trajectoryInfoY.GoalPositions.RemoveAt(0);
			trajectoryInfoT.GoalPositions.RemoveAt(0);
			trajectoryInfoX.Durations.RemoveAt(0);
			trajectoryInfoY.Durations.RemoveAt(0);
			trajectoryInfoT.Durations.RemoveAt(0);
			return false;
		}


		private float GetPosNoise(float val)
		{
			float randomNumber = SIGVerseUtils.GetRandomNumberFollowingNormalDistribution(0.6f);
			return val * Mathf.Clamp(randomNumber, -3.0f, +3.0f); // plus/minus 3.0 is sufficiently large.
		}


		private float GetRotNoise(float val)
		{
			float randomNumber = SIGVerseUtils.GetRandomNumberFollowingNormalDistribution(0.3f);
			return val * Mathf.Clamp(randomNumber, -3.0f, +3.0f); // plus/minus 3.0 is sufficiently large.
		}
	}
}

