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
		public class TrajectoryData
		{
			public float StartTime    { get; set; }
			public List<float> Durations { get; set; }
			public List<float> GoalPositions { get; set; }

			public TrajectoryData(float startTime, List<float> duration, List<float> goalPosition)
			{
				this.StartTime       = startTime;
				this.Durations       = duration;
				this.GoalPositions   = goalPosition;
			}
		}

//		private const float wheelInclinationThreshold = 0.985f; // 80[deg]
		private const float wheelInclinationThreshold = 0.965f; // 75[deg]
//		private const float wheelInclinationThreshold = 0.940f; // 70[deg]

		private Transform odom;
		private Transform baseFootprint;
		private Transform baseFootprintRigidbody;
		private Transform baseFootprintPosNoise;
		private Transform baseFootprintRotNoise;

		private Dictionary<string, TrajectoryData> trajectoryInfoMap;
				
		private Vector3 world2OdomPositionUnity = new Vector3();
		private float world2OdomRotationUnity;
		private Vector3 startPosition = new Vector3();
		private float startRotation;

		int targetPointIndex    = 0;
		int targetPointIndexOld = 0;
		float progressTimeRatio = 0.0f;


		void Awake()
		{
			this.trajectoryInfoMap = new Dictionary<string, TrajectoryData>();
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomXJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomYJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomTJointName, null);

			this.odom                   = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.OdomName);
			this.baseFootprint          = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);
			this.baseFootprintRigidbody = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRigidbodyName);
			this.baseFootprintPosNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintPosNoiseName);
			this.baseFootprintRotNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRotNoiseName);
			
			this.world2OdomPositionUnity = this.odom.position;
			this.world2OdomRotationUnity = this.odom.rotation.eulerAngles.y * Mathf.Deg2Rad;
		}


		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{
			this.startPosition = this.GetCurrentRosPosition();
			this.startRotation = this.GetCurrentRosRotation();

			this.targetPointIndex    = 0;
			this.targetPointIndexOld = 0;

			if (IsTrajectryMsgCorrect(ref jointTrajectory) == false){ return; }
			
			this.SetTrajectoryInfoMap(ref jointTrajectory);
			this.StopOmniIfOverLimitSpeed();
		}


		protected void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootprint.forward.y) < wheelInclinationThreshold) { return; }
			
			if (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] != null)
			{
				this.UpdateTargetPointIndex();
				this.UpdateProgressTimeRatio();
				this.UpdateStartPosition();

				if (this.progressTimeRatio > 1 && this.targetPointIndex == (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions.Count-1))
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
            TrajectoryData trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
            TrajectoryData trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
			
			Vector3 goalPosition = new Vector3();
			goalPosition.x = trajectoryInfoX.GoalPositions[this.targetPointIndex];
			goalPosition.y = trajectoryInfoY.GoalPositions[this.targetPointIndex];
						
			Vector3 newPosition   = Vector3.Lerp(this.startPosition, goalPosition, this.progressTimeRatio);
			Vector3 deltaPosition = newPosition - this.GetCurrentRosPosition();
			
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
			this.baseFootprintRigidbody.position += GetUnityPositionFromRosPosition(deltaPosition);
			this.baseFootprintPosNoise.position  += GetUnityPositionFromRosPosition(deltaNoisePos);
		}


		private void UpdateOmniRotation()
		{
            TrajectoryData trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
			
			Quaternion startRotQua = Quaternion.AngleAxis(this.startRotation * Mathf.Rad2Deg, Vector3.forward);
			Quaternion goalRotQua = Quaternion.AngleAxis(-trajectoryInfo.GoalPositions[this.targetPointIndex] * Mathf.Rad2Deg, Vector3.forward);
			Quaternion newRotQua   = Quaternion.Lerp(startRotQua, goalRotQua, this.progressTimeRatio);

			float deltaAngle = (newRotQua.eulerAngles.z * Mathf.Deg2Rad) - this.GetCurrentRosRotation();
			if (deltaAngle < -Math.PI) { deltaAngle += (float)(2 * Math.PI); }
			if (deltaAngle >  Math.PI) { deltaAngle -= (float)(2 * Math.PI); }
			
			if (Math.Abs(deltaAngle) > HSRCommon.MaxSpeedBaseRad * Time.fixedDeltaTime)
			{
				float maxSpeedBaseRadAtFixedDeltaTime = HSRCommon.MaxSpeedBaseRad * Time.fixedDeltaTime;
				deltaAngle = Mathf.Clamp(deltaAngle, -maxSpeedBaseRadAtFixedDeltaTime, +maxSpeedBaseRadAtFixedDeltaTime);
			}
			
			Quaternion deltaRotQua      = Quaternion.Euler(new Vector3(0.0f, 0.0f, deltaAngle * Mathf.Rad2Deg));
			Quaternion deltaNoiseRotQua = Quaternion.Euler(new Vector3(0.0f, 0.0f, this.GetRotNoise(deltaAngle * Mathf.Rad2Deg)));
			
			//update rotation.
			this.baseFootprintRigidbody.rotation *= deltaRotQua;
			this.baseFootprintRotNoise.rotation  *= deltaNoiseRotQua;
		}


		private void UpdateTargetPointIndex()
		{
            TrajectoryData trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
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
            TrajectoryData trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
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
				this.startPosition       = this.GetCurrentRosPosition();
				this.startRotation       = this.GetCurrentRosRotation();
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


		private float GetCurrentRosRotation()
		{
			float nowRosRotation = this.baseFootprint.rotation.eulerAngles.y * Mathf.Deg2Rad - this.world2OdomRotationUnity;
			if (nowRosRotation > Math.PI) { nowRosRotation -= (float)(2 * Math.PI); }

			return nowRosRotation;
		}


		private Vector3 GetCurrentRosPosition()
		{
			return GetRosPositionFromUnityPosition(this.baseFootprint.position - this.world2OdomPositionUnity);
		}


		private static Vector3 GetRosPositionFromUnityPosition(Vector3 unityPosition)
		{
            return new Vector3(unityPosition.z, -unityPosition.x, unityPosition.y);
		}


		private static Vector3 GetUnityPositionFromRosPosition(Vector3 rosPsition)
		{
            return new Vector3(-rosPsition.y, rosPsition.z, rosPsition.x);
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
					this.trajectoryInfoMap[name] = new TrajectoryData(Time.time, durations, positions);
				}

				if (name == HSRCommon.OmniOdomYJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryData(Time.time, durations, positions);
				}

				if (name == HSRCommon.OmniOdomTJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryData(Time.time, durations, positions);
				}
			}
		}


		private bool StopOmniIfOverLimitSpeed()
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

