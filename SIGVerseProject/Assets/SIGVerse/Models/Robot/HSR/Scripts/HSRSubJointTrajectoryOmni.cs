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
		float deltaTime = 0.0f;


		void Awake()
		{
			this.trajectoryInfoMap = new Dictionary<string, TrajectoryInfo>();
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomXJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomYJointName, null);
			this.trajectoryInfoMap.Add(HSRCommon.OmniOdomTJointName, null);

			this.baseFootprintRigidbody = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRigidbodyName);
			this.baseFootprintPosNoise = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintPosNoiseName);
			this.baseFootprintRotNoise = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRotNoiseName);
			
			this.initialPosition = this.baseFootprintRigidbody.position;
			this.initialRotation = this.baseFootprintRigidbody.rotation;
		}//Awake


		protected override void Start()
		{
			base.Start();
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{
			this.startPosition = this.baseFootprintRigidbody.position;
			this.startRotation = this.baseFootprintRigidbody.rotation;

			if(jointTrajectory.joint_names.Count != 3)
			{
				SIGVerseLogger.Warn("joint_names.Count != 3");
				return;
			}

			for (int i=0; i < jointTrajectory.joint_names.Count; i++)
			{
				string name    = jointTrajectory.joint_names[i];

				List<float> positions = new List<float>();
				List<float> durations = new List<float>();
				for (int pointIndex = 0; pointIndex < jointTrajectory.points.Count; pointIndex++)
				{
					positions.Add(HSRCommon.GetClampedPosition((float)jointTrajectory.points[pointIndex].positions[i], name));
					durations.Add((float)jointTrajectory.points[pointIndex].time_from_start.secs + (float)jointTrajectory.points[pointIndex].time_from_start.nsecs * 1.0e-9f);
				}

				if (name == HSRCommon.OmniOdomXJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, this.baseFootprintRigidbody.position.z);
				}

				if (name == HSRCommon.OmniOdomYJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, this.baseFootprintRigidbody.position.x);
				}

				if (name == HSRCommon.OmniOdomTJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, 0.0f);
				}
			}//for

			if(trajectoryInfoMap[HSRCommon.OmniOdomXJointName] == null || trajectoryInfoMap[HSRCommon.OmniOdomYJointName] == null || trajectoryInfoMap[HSRCommon.OmniOdomTJointName] == null)
			{
				trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
				trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
				trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
				SIGVerseLogger.Warn("Omni trajectry args error.");
			}

			//check limit speed
			for (int i = 0; i < trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions.Count; i++)
			{
				double linear_speed = 0;
				if (i == 0)
				{
					double temp_distance = Math.Sqrt(Math.Pow(((trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions[0] + this.initialPosition.z) - this.startPosition.z) * 100, 2) + Math.Pow(((trajectoryInfoMap[HSRCommon.OmniOdomYJointName].GoalPositions[0] + this.initialPosition.x) - this.startPosition.x) * 100, 2));
					linear_speed = (temp_distance / 100) / trajectoryInfoMap[HSRCommon.OmniOdomXJointName].Durations[0];
				}
				else
				{
					double temp_distance = Math.Sqrt(Math.Pow((trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions[i] - trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions[i-1]) * 100, 2) + Math.Pow((trajectoryInfoMap[HSRCommon.OmniOdomYJointName].GoalPositions[i] - trajectoryInfoMap[HSRCommon.OmniOdomYJointName].GoalPositions[i-1]) * 100, 2));
					linear_speed = (temp_distance / 100) / (trajectoryInfoMap[HSRCommon.OmniOdomXJointName].Durations[i] - trajectoryInfoMap[HSRCommon.OmniOdomXJointName].Durations[i-1]);
				}

				double angular_speed = 0;
				if (i == 0)
				{
					double temp_angle = Math.Abs((this.ToAngle(trajectoryInfoMap[HSRCommon.OmniOdomTJointName].GoalPositions[0]) + this.initialRotation.eulerAngles.y) - this.startRotation.eulerAngles.y);
					if(temp_angle > Math.PI) { temp_angle -= Math.PI; }
					angular_speed = temp_angle / trajectoryInfoMap[HSRCommon.OmniOdomTJointName].Durations[0];
				}
				else
				{
					double temp_angle = Math.Abs(this.ToAngle(trajectoryInfoMap[HSRCommon.OmniOdomTJointName].GoalPositions[i]) - trajectoryInfoMap[HSRCommon.OmniOdomTJointName].GoalPositions[i-1]);
					if (temp_angle > Math.PI) { temp_angle -= Math.PI; }
					angular_speed = temp_angle / (trajectoryInfoMap[HSRCommon.OmniOdomTJointName].Durations[i] - trajectoryInfoMap[HSRCommon.OmniOdomTJointName].Durations[i-1]);
				}

				if (linear_speed > HSRCommon.MaxSpeedBase || angular_speed > this.ToAngle(HSRCommon.MaxSpeedBaseRad))//exceed limit 
				{
					trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
					trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
					trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
					SIGVerseLogger.Warn("Omni trajectry args error. (exceed limit)");
					return;
				}
			}//for


		}//SubscribeMessageCallback


		protected override void Update()
		{
			base.Update();
			
			if (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] != null)
			{
				this.UpdateTargetPointIndex();
				this.UpdateDeltaTime();
				this.UpdateStartPsition();

				if (this.deltaTime > 1 && (targetPointIndex + 1) == this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions.Count)
				{
					this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
					this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
					return;
				}

				this.UpdateOmniPosition();               
				this.UpdateOmniRotation();
			}

		}//Update


		private void UpdateOmniPosition()
		{
			TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
			
			Vector3 goalPosition = new Vector3();
			goalPosition.x = trajectoryInfoY.GoalPositions[targetPointIndex] + this.initialPosition.x;
			goalPosition.z = trajectoryInfoX.GoalPositions[targetPointIndex] + this.initialPosition.z;

			Vector3 slerpedPosition = Vector3.Slerp(this.startPosition, goalPosition, deltaTime);
			Vector3 position = (slerpedPosition - this.baseFootprintRigidbody.position);
			Vector3 noisePosition = new Vector3();
			noisePosition.x = this.GetPosNoise(position.x);
			noisePosition.z = this.GetPosNoise(position.z);

			//update position
			this.baseFootprintRigidbody.position += position;
			this.baseFootprintPosNoise.position += noisePosition;
			trajectoryInfoMap[HSRCommon.OmniOdomXJointName].CurrentPosition = position.z;
			trajectoryInfoMap[HSRCommon.OmniOdomYJointName].CurrentPosition = position.x;
			trajectoryInfoMap[HSRCommon.OmniOdomXJointName].CurrentTime = Time.time;
			trajectoryInfoMap[HSRCommon.OmniOdomYJointName].CurrentTime = Time.time;
			
			return;
		}//UpdateOmniPosition


		private void UpdateOmniRotation()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];         
			Quaternion goalRotation = Quaternion.Euler(new Vector3(this.initialRotation.eulerAngles.x, this.initialRotation.eulerAngles.y, this.ToAngle(trajectoryInfo.GoalPositions[this.targetPointIndex]) + this.initialRotation.eulerAngles.z));
			Quaternion sleapedRotation = Quaternion.Slerp(this.startRotation, goalRotation, this.deltaTime);
			float subAngleDeg = (sleapedRotation.eulerAngles.y - this.baseFootprintRigidbody.rotation.eulerAngles.y);
			if (subAngleDeg > 180) { subAngleDeg -= 360; }
			else if(subAngleDeg < -180) { subAngleDeg += 360; }
						
			Quaternion Rotation = Quaternion.Euler(new Vector3(0, 0, subAngleDeg));
			Quaternion NoiseRot = Quaternion.Euler(new Vector3(0, 0, this.GetRotNoise(subAngleDeg)));

			//update rotation.
			this.baseFootprintRigidbody.rotation *= Rotation;
			this.baseFootprintRotNoise.rotation *= NoiseRot;
			trajectoryInfoMap[HSRCommon.OmniOdomTJointName].CurrentTime = Time.time;
			trajectoryInfoMap[HSRCommon.OmniOdomTJointName].CurrentPosition = Rotation.eulerAngles.z;
			
			return;
		}//UpdateOmniRotation


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
		}//GetTargetPointIndex


		private void UpdateDeltaTime()
		{
			TrajectoryInfo trajectoryInfo = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
			if (this.targetPointIndex == 0)
			{                
				this.deltaTime = (Time.time - trajectoryInfo.StartTime) / (trajectoryInfo.Durations[this.targetPointIndex]);
			}
			else
			{
				this.deltaTime = (Time.time - (trajectoryInfo.StartTime + trajectoryInfo.Durations[this.targetPointIndex-1])) / (trajectoryInfo.Durations[this.targetPointIndex] - trajectoryInfo.Durations[this.targetPointIndex-1]);
			}
		}//GetDeltaTime


		private void UpdateStartPsition()
		{
			if(this.targetPointIndex != 0)
			{
				TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
				TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
				TrajectoryInfo trajectoryInfoT = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
				this.startPosition = new Vector3(trajectoryInfoY.GoalPositions[this.targetPointIndex-1] + this.initialPosition.x, 0, trajectoryInfoX.GoalPositions[this.targetPointIndex-1] + this.initialPosition.z);
				this.startRotation = Quaternion.Euler(new Vector3(this.initialRotation.eulerAngles.x, this.initialRotation.eulerAngles.y, this.ToAngle(trajectoryInfoT.GoalPositions[this.targetPointIndex-1]) + this.initialRotation.eulerAngles.z));
			}
		}//UpdateStartPsition


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


		public float ToAngle(double radian)
		{
			return (float)(radian * 180 / Math.PI);
		}

	}
}

