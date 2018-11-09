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

            if(this.CheckTrajectryMsg(ref jointTrajectory) == false)
            {
                SIGVerseLogger.Warn("JointTrajectory args error.");
                return;
            }
            
            this.SetTrajectoryInfoMap(ref jointTrajectory);
            if (this.CheckMaxSpeed() == false){ return; }

        }//SubscribeMessageCallback


		protected override void Update()
		{
			base.Update();
			
			if (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName] != null && this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName] != null)
			{
				this.UpdateTargetPointIndex();
				this.UpdateDeltaTime();
				this.UpdateStartPsition();

				if (this.deltaTime > 1 && targetPointIndex == (this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName].GoalPositions.Count-1))
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
			goalPosition.x = -trajectoryInfoY.GoalPositions[targetPointIndex] + this.initialPosition.x;
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
				this.startPosition = new Vector3(-trajectoryInfoY.GoalPositions[this.targetPointIndex-1] + this.initialPosition.x, 0, trajectoryInfoX.GoalPositions[this.targetPointIndex-1] + this.initialPosition.z);
				this.startRotation = Quaternion.Euler(new Vector3(this.initialRotation.eulerAngles.x, this.initialRotation.eulerAngles.y, this.ToAngle(trajectoryInfoT.GoalPositions[this.targetPointIndex-1]) + this.initialRotation.eulerAngles.z));
			}
		}//UpdateStartPsition


        private bool CheckTrajectryMsg(ref SIGVerse.RosBridge.trajectory_msgs.JointTrajectory msg)
        {
            
            if(msg.joint_names.Count == 3)//Omni
            {
                if (0 <= msg.joint_names.IndexOf(HSRCommon.OmniOdomXJointName) && 0 <= msg.joint_names.IndexOf(HSRCommon.OmniOdomYJointName) && 0 <= msg.joint_names.IndexOf(HSRCommon.OmniOdomTJointName))
                {
                    return true;
                }
            }                
            return false;
        }//CheckTrajectryMsg


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
        }//SetTrajectoryInfoMap


        private bool CheckMaxSpeed()
        {
            TrajectoryInfo trajectoryInfoX = this.trajectoryInfoMap[HSRCommon.OmniOdomXJointName];
            TrajectoryInfo trajectoryInfoY = this.trajectoryInfoMap[HSRCommon.OmniOdomYJointName];
            TrajectoryInfo trajectoryInfoT = this.trajectoryInfoMap[HSRCommon.OmniOdomTJointName];
            trajectoryInfoX.GoalPositions.Insert(0, this.startPosition.z - this.initialPosition.z);
            trajectoryInfoY.GoalPositions.Insert(0, this.startPosition.x - this.initialPosition.x);
            trajectoryInfoT.GoalPositions.Insert(0, this.ToRadian(this.startRotation.eulerAngles.y));
            trajectoryInfoX.Durations.Insert(0, 0.0f);
            trajectoryInfoY.Durations.Insert(0, 0.0f);
            trajectoryInfoT.Durations.Insert(0, 0.0f);
            

            //check limit speed
            for (int i = 1; i < trajectoryInfoX.GoalPositions.Count; i++)
            {                
                double temp_distance = Math.Sqrt(Math.Pow(trajectoryInfoX.GoalPositions[i] - trajectoryInfoX.GoalPositions[i-1], 2) + Math.Pow(trajectoryInfoY.GoalPositions[i] - trajectoryInfoY.GoalPositions[i-1], 2));
                double linear_speed = temp_distance / (trajectoryInfoX.Durations[i] - trajectoryInfoX.Durations[i-1]);
                
                double temp_angle = Math.Abs(trajectoryInfoT.GoalPositions[i] - trajectoryInfoT.GoalPositions[i-1]);
                if (temp_angle < -Math.PI) { temp_angle += (2 * Math.PI); }
                if (temp_angle > Math.PI) { temp_angle -= (2 * Math.PI); }
                double angular_speed = temp_angle / (trajectoryInfoT.Durations[i] - trajectoryInfoT.Durations[i-1]);
                
                if (linear_speed > HSRCommon.MaxSpeedBase || angular_speed > HSRCommon.MaxSpeedBaseRad)//exceed limit 
                {
                    trajectoryInfoMap[HSRCommon.OmniOdomXJointName] = null;
                    trajectoryInfoMap[HSRCommon.OmniOdomYJointName] = null;
                    trajectoryInfoMap[HSRCommon.OmniOdomTJointName] = null;
                    SIGVerseLogger.Warn("Omni trajectry args error. (exceed limit)");
                    return false;
                }
            }//for

            trajectoryInfoX.GoalPositions.RemoveAt(0);
            trajectoryInfoY.GoalPositions.RemoveAt(0);
            trajectoryInfoT.GoalPositions.RemoveAt(0);
            trajectoryInfoX.Durations.RemoveAt(0);
            trajectoryInfoY.Durations.RemoveAt(0);
            trajectoryInfoT.Durations.RemoveAt(0);
            return true;
        }//CheckMaxSpeed


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


        public float ToRadian(double deg)
        {
            return (float)(deg * (Math.PI / 180));
        }

    }
}

