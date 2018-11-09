using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;

namespace SIGVerse.ToyotaHSR
{
	public class HSRSubJointTrajectory : RosSubMessage<SIGVerse.RosBridge.trajectory_msgs.JointTrajectory>, IHSRGraspedObjectHandler
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

        private Transform armLiftLink;
        private Transform armFlexLink;
        private Transform armRollLink;
        private Transform wristFlexLink;
        private Transform wristRollLink;
        private Transform headPanLink;
        private Transform headTiltLink;
        private Transform torsoLiftLink;
        private Transform handLProximalLink;
        private Transform handRProximalLink;

        private float armLiftLinkIniPosZ;
        private float torsoLiftLinkIniPosZ;

        private Dictionary<string, TrajectoryInfo> trajectoryInfoMap;
        private List<string> trajectoryKeyList;

        private GameObject graspedObject;


		void Awake()
		{
            this.armLiftLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmLiftLinkName );
            this.armFlexLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmFlexLinkName );
            this.armRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmRollLinkName );
            this.wristFlexLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.WristFlexLinkName );
            this.wristRollLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.WristRollLinkName );
            this.headPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HeadPanLinkName );
            this.headTiltLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HeadTiltLinkName );
            this.torsoLiftLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.TorsoLiftLinkName );
            this.handLProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HandLProximalLinkName );
            this.handRProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HandRProximalLinkName );

            this.armLiftLinkIniPosZ   = this.armLiftLink.localPosition.z;
            this.torsoLiftLinkIniPosZ = this.torsoLiftLink.localPosition.z;

            this.trajectoryInfoMap = new Dictionary<string, TrajectoryInfo>();
            this.trajectoryInfoMap.Add(HSRCommon.ArmLiftJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.ArmFlexJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.ArmRollJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.WristFlexJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.WristRollJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.HeadPanJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.HeadTiltJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.HandLProximalJointName, null);
            this.trajectoryInfoMap.Add(HSRCommon.HandRProximalJointName, null);


            this.trajectoryKeyList = new List<string>(trajectoryInfoMap.Keys);
        }


		protected override void Start()
		{
			base.Start();
			
			this.graspedObject = null;
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.trajectory_msgs.JointTrajectory jointTrajectory)
		{

			if(jointTrajectory.joint_names.Count != jointTrajectory.points[0].positions.Count)
			{
				SIGVerseLogger.Warn("joint_names.Count != points.positions.Count  topicName = "+this.topicName);
				return;
			}

			for(int i=0; i < jointTrajectory.joint_names.Count; i++)
			{
				string name    = jointTrajectory.joint_names[i];

                List<float> positions = new List<float>();
                List<float> durations = new List<float>();
                for (int pointIndex = 0; pointIndex < jointTrajectory.points.Count; pointIndex++)
                {
                    positions.Add(HSRCommon.GetClampedPosition((float)jointTrajectory.points[pointIndex].positions[i], name));
                    durations.Add((float)jointTrajectory.points[pointIndex].time_from_start.secs + (float)jointTrajectory.points[pointIndex].time_from_start.nsecs * 1.0e-9f);
                }
                                

                if (name == HSRCommon.ArmLiftJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, this.armLiftLink.localPosition.z - this.armLiftLinkIniPosZ);
				}

				if(name == HSRCommon.ArmFlexJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(this.armFlexLink.localEulerAngles.y, name) * Mathf.Deg2Rad);
				}

				if (name == HSRCommon.ArmRollJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(-this.armRollLink.localEulerAngles.z, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.WristFlexJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(this.wristFlexLink.localEulerAngles.y, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.WristRollJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(-this.wristRollLink.localEulerAngles.z, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.HeadPanJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(-this.headPanLink.localEulerAngles.z, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.HeadTiltJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(this.headTiltLink.localEulerAngles.y, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.HandLProximalJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(this.handLProximalLink.localEulerAngles.x, name) * Mathf.Deg2Rad);
				}

				if(name == HSRCommon.HandRProximalJointName)
				{
					this.trajectoryInfoMap[name] = new TrajectoryInfo(Time.time, durations, positions, Time.time, HSRCommon.GetCorrectedJointsEulerAngle(this.handRProximalLink.localEulerAngles.x, name) * Mathf.Deg2Rad);
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
					if (jointName == HSRCommon.ArmLiftJointName)
					{
						float newPos = GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeed, HSRCommon.MaxSpeedTorso);

						this.armLiftLink  .localPosition = new Vector3(this.armLiftLink  .localPosition.x, this.armLiftLink.localPosition.y,   this.armLiftLinkIniPosZ   + newPos        );
						this.torsoLiftLink.localPosition = new Vector3(this.torsoLiftLink.localPosition.x, this.torsoLiftLink.localPosition.y, this.torsoLiftLinkIniPosZ + newPos / 2.0f );
					}

					if (jointName == HSRCommon.ArmFlexJointName)
					{
						float newPos = HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);
						
						this.armFlexLink.localEulerAngles = new Vector3(this.armFlexLink.localEulerAngles.x, newPos, this.armFlexLink.localEulerAngles.z);
					}

					if (jointName == HSRCommon.ArmRollJointName)
					{
						float newPos = -HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);

						this.armRollLink.localEulerAngles = new Vector3(this.armRollLink.localEulerAngles.x, this.armRollLink.localEulerAngles.y, newPos);
					}

					if (jointName == HSRCommon.WristFlexJointName)
					{
						float newPos = HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);

						this.wristFlexLink.localEulerAngles = new Vector3(this.wristFlexLink.localEulerAngles.x, newPos, this.wristFlexLink.localEulerAngles.z);
					}

					if (jointName == HSRCommon.WristRollJointName)
					{
						float newPos = -HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);

						this.wristRollLink.localEulerAngles = new Vector3(this.wristRollLink.localEulerAngles.x, this.wristRollLink.localEulerAngles.y, newPos);
					}

					if (jointName == HSRCommon.HeadPanJointName)
					{
						float newPos = -HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedHead) * Mathf.Rad2Deg, jointName);

						this.headPanLink.localEulerAngles = new Vector3(this.headPanLink.localEulerAngles.x, this.headPanLink.localEulerAngles.y, newPos);
					}

					if (jointName == HSRCommon.HeadTiltJointName)
                    {
						float newPos = HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedHead) * Mathf.Rad2Deg, jointName);

						this.headTiltLink.localEulerAngles = new Vector3(this.headTiltLink.localEulerAngles.x, newPos, this.headTiltLink.localEulerAngles.z);
					}

					if (jointName == HSRCommon.HandLProximalJointName)
					{
						float newPos = HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);

						// Grasping and hand closing
						if(this.graspedObject!=null && this.IsAngleDecreasing(newPos, this.handLProximalLink.localEulerAngles.x))
						{
							// Have to stop
							this.trajectoryInfoMap[jointName] = null;
						}
						// Otherwise
						else
						{
							this.handLProximalLink.localEulerAngles = new Vector3(newPos, this.handLProximalLink.localEulerAngles.y, this.handLProximalLink.localEulerAngles.z);
						}
					}

					if (jointName == HSRCommon.HandRProximalJointName)
					{
						float newPos = HSRCommon.GetCorrectedJointsEulerAngle(GetPositionAndUpdateTrajectory(this.trajectoryInfoMap, jointName, HSRCommon.MinSpeedRad, HSRCommon.MaxSpeedArm) * Mathf.Rad2Deg, jointName);

						// Grasping and hand closing
						if(this.graspedObject!=null && this.IsAngleIncreasing(newPos, this.handRProximalLink.localEulerAngles.x))
						{
							// Have to stop
							this.trajectoryInfoMap[jointName] = null;
						}
						// Otherwise
						else
						{
							this.handRProximalLink.localEulerAngles = new Vector3(newPos, this.handRProximalLink.localEulerAngles.y, this.handRProximalLink.localEulerAngles.z);
						}
					}
                }
			}
		}

        

        private static float GetPositionAndUpdateTrajectory(Dictionary<string, TrajectoryInfo> trajectoryInfoMap, string jointName, float minSpeed, float maxSpeed)
		{
			TrajectoryInfo trajectoryInfo = trajectoryInfoMap[jointName];

			// Calculate move speed
			float speed = 0.0f;

            int targetPointIndex = 0;
            // Select current trajectory target point 
            for (int i = 0; i < trajectoryInfo.Durations.Count; i++)
            {
                targetPointIndex = i;
                if (Time.time - trajectoryInfo.StartTime < trajectoryInfo.Durations[targetPointIndex])
                {
                    break;
                }
            }

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


		private bool IsAngleIncreasing(float newVal, float oldVal)
		{
			float angleDiff = this.GetAngleDiff(newVal, oldVal);

			if(angleDiff==0.0f) { return false; }

			if(angleDiff > 0.0f)
			{
				return Mathf.Abs(angleDiff) < 180;
			}
			else
			{
				return Mathf.Abs(angleDiff) > 180;
			}
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

