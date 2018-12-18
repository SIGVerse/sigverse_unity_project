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

			public TrajectoryInfo(float startTime, List<float> duration, List<float> goalPosition)
			{
				this.StartTime       = startTime;
				this.Durations       = duration;
				this.GoalPositions   = goalPosition;
			}
		}

		private Transform baseFootprint;
		private Transform baseFootprintRigidbody;
		private Transform baseFootprintPosNoise;
		private Transform baseFootprintRotNoise;
		
		private Dictionary<string, TrajectoryInfo> trajectoryInfoMap;
		private List<string> trajectoryKeyList;

		private Vector3 initialPosition = new Vector3();
		private Vector3 startPosition   = new Vector3();
		private float initialRotation;
		private float startRotation;

		int targetPointIndex    = 0;
		int targetPointIndexOld = 0;
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
			this.initialRotation = this.baseFootprint.rotation.eulerAngles.y * Mathf.Deg2Rad;
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{
			this.startPosition = this.getNowRosPosition();
			this.startRotation = this.getNowRosRotation();

			this.targetPointIndex    = 0;
			this.targetPointIndexOld = 0;

			if (IsTrajectryMsgCorrect(ref jointTrajectory) == false){ return; }
			
			this.SetTrajectoryInfoMap(ref jointTrajectory);
			this.StopJointIfOverLimitSpeed();
		}


		protected void FixedUpdate()
		{
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
			goalPosition.x = trajectoryInfoX.GoalPositions[targetPointIndex];
			goalPosition.y = trajectoryInfoY.GoalPositions[targetPointIndex];
						
			Vector3 newPosition   = Vector3.Lerp(this.startPosition, goalPosition, progressTimeRatio);
			Vector3 deltaPosition = newPosition - this.getNowRosPosition();
			
			float deltaLinearSpeed = Mathf.Sqrt(Mathf.Pow(deltaPosition.x, 2) + Mathf.Pow(deltaPosition.z, 2));
			if (deltaLinearSpeed > HSRCommon.MaxSpeedBase * Time.fixedDeltaTime)
			{
				float deltaLinearSpeedClamped = Mathf.Clamp(deltaLinearSpeed, 0.0f, HSRCommon.MaxSpeedBase * Time.fixedDeltaTime);
				deltaPosition.x = deltaPosition.x * deltaLinearSpeedClamped / deltaLinearSpeed;
				deltaPosition.y = deltaPosition.y * deltaLinearSpeedClamped / deltaLinearSpeed;
			}
			
			Vector3 deltaNoisePos = new Vector3();
			deltaNoisePos.x = this.GetPosNoise(deltaPosition.x);
			deltaNoisePos.y = this.GetPosNoise(deltaPosition.y);

			//update position
			this.baseFootprintRigidbody.position += unityPositionFromRosPosition(deltaPosition);
			this.baseFootprintPosNoise.position  += unityPositionFromRosPosition(deltaNoisePos);
		}


		private void UpdateOmniRotation()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
			
			Quaternion startRotation = Quaternion.AngleAxis(this.startRotation * Mathf.Rad2Deg, Vector3.forward);
			Quaternion goalRotation  = Quaternion.AngleAxis(-trajectoryInfo.GoalPositions[this.targetPointIndex] * Mathf.Rad2Deg, Vector3.forward);
			Quaternion newRotation   = Quaternion.Lerp(startRotation, goalRotation, this.progressTimeRatio);

			float deltaAngle = (newRotation.eulerAngles.z * Mathf.Deg2Rad) - this.getNowRosRotation();
			if (deltaAngle < -Math.PI) { deltaAngle += (float)(2 * Math.PI); }
			if (deltaAngle >  Math.PI) { deltaAngle -= (float)(2 * Math.PI); }
			
			if (Math.Abs(deltaAngle) > HSRCommon.MaxSpeedBaseRad * Time.fixedDeltaTime)
			{
				float maxSpeedBaseRadAtFixedDeltaTime = HSRCommon.MaxSpeedBaseRad * Time.fixedDeltaTime;
				deltaAngle = Mathf.Clamp(deltaAngle, -maxSpeedBaseRadAtFixedDeltaTime, +maxSpeedBaseRadAtFixedDeltaTime);
			}
			
			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, deltaAngle * Mathf.Rad2Deg));
			Quaternion deltaNoiseRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, this.GetRotNoise(deltaAngle * Mathf.Rad2Deg)));
			
			//update rotation.
			this.baseFootprintRigidbody.rotation *= deltaRotation;
			this.baseFootprintRotNoise.rotation  *= deltaNoiseRot;
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
			if(this.targetPointIndex != this.targetPointIndexOld)
			{
				this.startPosition       = this.getNowRosPosition();
				this.startRotation       = this.getNowRosRotation();
				this.targetPointIndexOld = this.targetPointIndex;
			}
		}


		private static bool IsTrajectryMsgCorrect(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
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


		private float getNowRosRotation()
		{
			float nowRosRotation = this.baseFootprint.rotation.eulerAngles.y * Mathf.Deg2Rad - this.initialRotation;
			if (nowRosRotation > Math.PI) { nowRosRotation -= (float)(2 * Math.PI); }

			return nowRosRotation;
		}


		private Vector3 getNowRosPosition()
		{
			return rosPositionFromUnityPosition(this.baseFootprint.position - this.initialPosition);
		}


		private static Vector3 rosPositionFromUnityPosition(Vector3 unityPosition)
		{
			Vector3 rosPosition = new Vector3(unityPosition.z, -unityPosition.x, 0.0f);
			return rosPosition;
		}


		private static Vector3 unityPositionFromRosPosition(Vector3 rosPsition)
		{
			Vector3 unityPosition = new Vector3(-rosPsition.y, 0.0f, rosPsition.x);
			return unityPosition;
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
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions);
				}

				if (name == HSRCommon.OmniOdomYJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions);
				}

				if (name == HSRCommon.OmniOdomTJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions);
				}
			}
		}


		private bool StopJointIfOverLimitSpeed()
		{
			List<float> trajectoryInfoDurations      = new List<float>(this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].Durations);
			List<float> trajectoryInfoXGoalPositions = new List<float>(this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions);
			List<float> trajectoryInfoYGoalPositions = new List<float>(this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName].GoalPositions);
			List<float> trajectoryInfoTGoalPositions = new List<float>(this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName].GoalPositions);
			trajectoryInfoDurations.Insert(0, 0.0f);
			trajectoryInfoXGoalPositions.Insert(0, this.startPosition.x);
			trajectoryInfoYGoalPositions.Insert(0, this.startPosition.y);
			trajectoryInfoTGoalPositions.Insert(0, this.startRotation);
			
			for (int i = 1; i < trajectoryInfoDurations.Count; i++)
			{
				double deltaTime       = (trajectoryInfoDurations[i] - trajectoryInfoDurations[i-1]);
				double deltaDistanceX  = trajectoryInfoXGoalPositions[i] - trajectoryInfoXGoalPositions[i-1];
				double deltaDistanceY  = trajectoryInfoYGoalPositions[i] - trajectoryInfoYGoalPositions[i-1];
				double deltaDistanceXY = Math.Sqrt(Math.Pow(deltaDistanceX, 2) + Math.Pow(deltaDistanceY, 2));
				double deltaAngle      = Math.Abs(trajectoryInfoTGoalPositions[i] - trajectoryInfoTGoalPositions[i-1]);
				if (deltaAngle < -Math.PI) { deltaAngle += (2 * Math.PI); }
				if (deltaAngle >  Math.PI) { deltaAngle -= (2 * Math.PI); }
				
				double linearSpeed  = deltaDistanceXY / deltaTime;
				double angularSpeed = deltaAngle / deltaTime;

				if (linearSpeed > HSRCommon.MaxSpeedBase || angularSpeed > HSRCommon.MaxSpeedBaseRad)
				{
					this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
					SIGVerseLogger.Warn("Omni speed error. (linear_speed = " + linearSpeed + " [m/s], angular_speed = " + angularSpeed + "[rad/s].");
					return true;
				}
			}
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

