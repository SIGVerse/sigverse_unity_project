using UnityEngine;
using System.Collections.Generic;
using System;

namespace SIGVerse.Common
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


		public static void UpdateLinkAngle(Transform link, Vector3 axis, float newPos)
		{
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

		public static float GetPositionAndUpdateTrajectory<TJoint>(Dictionary<TJoint, TrajectoryInfo> trajectoryInfoMap, TJoint joint, float minSpeed, float maxSpeed)
		{
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


		public static bool IsOverLimitSpeed<TJoint>(List<TJoint> trajectoryKeyList, Dictionary<TJoint, TrajectoryInfo> trajectoryInfoMap, Func<TJoint, float> getMaxJointSpeedFunc)
		{
			foreach (TJoint joint in trajectoryKeyList)
			{
				if (trajectoryInfoMap[joint] == null) { continue; }

				List<float> trajectoryInfoDurations     = new List<float>(trajectoryInfoMap[joint].Durations);
				List<float> trajectoryInfoGoalPositions = new List<float>(trajectoryInfoMap[joint].GoalPositions);

				trajectoryInfoDurations    .Insert(0, 0.0f);
				trajectoryInfoGoalPositions.Insert(0, trajectoryInfoMap[joint].CurrentPosition);

				for (int i = 1; i < trajectoryInfoGoalPositions.Count; i++)
				{
					double tempDistance  = Math.Abs(trajectoryInfoGoalPositions[i] - trajectoryInfoGoalPositions[i-1]);
					double tempDurations = Math.Abs(trajectoryInfoDurations    [i] - trajectoryInfoDurations[i-1]);
					double tempSpeed     = tempDistance / tempDurations;
					
					if(tempSpeed > getMaxJointSpeedFunc(joint))
					{
						SIGVerseLogger.Warn("Trajectry speed error. Joint Name=" + joint.ToString() + ", Speed=" + tempSpeed + ", Max Speed=" + getMaxJointSpeedFunc(joint) + ")");
						return true;
					}
				}
			}

			return false;
		}


		public static int GetTargetPointIndex(TrajectoryInfo trajectoryInfo)
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


		public static bool IsAngleDecreasing(float newVal, float oldVal)
		{
			float angleDiff = GetAngleDiff(newVal, oldVal);

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

		public static float GetAngleDiff(float newVal, float oldVal)
		{
			newVal = (newVal < 0)? newVal+360 : newVal;
			oldVal = (oldVal < 0)? oldVal+360 : oldVal;
			return newVal - oldVal;
		}
	}
}

