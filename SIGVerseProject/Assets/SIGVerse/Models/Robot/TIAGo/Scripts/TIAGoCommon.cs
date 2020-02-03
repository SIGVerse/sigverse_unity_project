using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.TIAGo
{
	public class TIAGoCommon
	{
		public const float MaxSpeedBase    = 0.3f;  // [m/s] TODO
		public const float MaxSpeedBaseRad = 1.1f;  // [rad/s] TODO

		// TODO
  //<xacro:property name="arm_1_max_vel"      value="1.95" />
  //<xacro:property name="arm_2_max_vel"      value="1.95" />
  //<xacro:property name="arm_3_max_vel"      value="2.35" />
  //<xacro:property name="arm_4_max_vel"      value="2.35" />
  //<xacro:property name="wrist_1_max_vel"      value="1.95" />
  //<xacro:property name="wrist_2_max_vel"      value="1.76" />
  //<xacro:property name="wrist_3_max_vel"      value="1.76" />
  //<xacro:property name="torso_max_vel"  value="0.07" />
  //<xacro:property name="head_max_vel"  value="3.0" />
  //<xacro:property name="head_1_lower_limit" value="-75"/>
  //<xacro:property name="head_1_upper_limit" value="75"/>
  //<xacro:property name="head_2_lower_limit" value="-60"/>
  //<xacro:property name="head_2_upper_limit" value="45"/>

		public const string OdomName   = "odom";

		// Link Names for Noise
		public const string BaseFootPrintPosNoiseName  = "base_footprint_pos_noise";
		public const string BaseFootPrintRotNoiseName  = "base_footprint_rot_noise";
		public const string BaseFootPrintRigidbodyName = "base_footprint_rigidbody";

		public enum Link
		{
			base_footprint,

			base_cover_link,
			base_link,
			rgbd_laser_link,

			torso_fixed_link,
			torso_fixed_column_link,
			base_antenna_left_link,
			base_antenna_right_link,
			base_imu_link,
			base_laser_link,
			base_mic_back_left_link,
			base_mic_back_right_link,
			base_mic_front_left_link,
			base_mic_front_right_link,
			base_sonar_01_link,
			base_sonar_02_link,
			base_sonar_03_link,
			caster_back_left_1_link,
			caster_back_left_2_link,
			caster_back_right_1_link,
			caster_back_right_2_link,
			caster_front_left_1_link,
			caster_front_left_2_link,
			caster_front_right_1_link,
			caster_front_right_2_link,
			suspension_left_link,
			suspension_right_link,
			wheel_left_link,
			wheel_right_link,

			torso_lift_link,

			head_1_link,
			head_2_link,
			xtion_link,
			xtion_depth_frame,
			xtion_depth_optical_frame,
			xtion_optical_frame,
			xtion_rgb_frame,
			xtion_rgb_optical_frame,

			arm_1_link,
			arm_2_link,
			arm_3_link,
			arm_4_link,
			arm_5_link,
			arm_6_link,
			arm_7_link,
			arm_tool_link,
			wrist_ft_link,
			wrist_ft_tool_link,
			gripper_link,
			gripper_grasping_frame,
			gripper_left_finger_link,
			gripper_right_finger_link,
			gripper_tool_link,
		}

		public enum Joint
		{
			torso_lift_joint,

			head_1_joint,
			head_2_joint,

			arm_1_joint,
			arm_2_joint,
			arm_3_joint,
			arm_4_joint,
			arm_5_joint,
			arm_6_joint,
			arm_7_joint,
			gripper_right_finger_joint,
			gripper_left_finger_joint,

			suspension_right_joint,
			suspension_left_joint,
			wheel_right_joint,
			wheel_left_joint,

			caster_front_right_1_joint,
			caster_front_right_2_joint,
			caster_front_left_1_joint,
			caster_front_left_2_joint,
			caster_back_right_1_joint,
			caster_back_right_2_joint,
			caster_back_left_1_joint,
			caster_back_left_2_joint,
		}

		private struct JointRange
		{
			public float min;
			public float max;

			public JointRange(float min, float max)
			{
				this.min = min;
				this.max = max;
			}
		}

		private static Dictionary<Joint, JointRange> jointRangeMap = new Dictionary<Joint, JointRange>()
		{
			{ Joint.torso_lift_joint, new JointRange( +0.00f, +0.35f) },

			{ Joint.head_1_joint, new JointRange( -1.24f, +1.24f) },
			{ Joint.head_2_joint, new JointRange( -0.98f, +0.79f) },

			{ Joint.arm_1_joint,  new JointRange( +0.07f, +2.68f) },
			{ Joint.arm_2_joint,  new JointRange( -1.50f, +1.02f) },
			{ Joint.arm_3_joint,  new JointRange( -3.46f, +1.50f) },
			{ Joint.arm_4_joint,  new JointRange( -0.32f, +2.29f) },
			{ Joint.arm_5_joint,  new JointRange( -2.07f, +2.07f) },
			{ Joint.arm_6_joint,  new JointRange( -1.39f, +1.39f) },
			{ Joint.arm_7_joint,  new JointRange( -2.07f, +2.07f) },

			{ Joint.gripper_right_finger_joint, new JointRange( +0.00f, +0.04f) },
			{ Joint.gripper_left_finger_joint,  new JointRange( +0.00f, +0.04f) },
		};


		public static List<Transform> GetLinksInChildren(Transform root)
		{
			List<Transform> linkList = new List<Transform>();

			foreach(string linkName in Enum.GetNames(typeof(Link)))
			{
				AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, linkName));
			}

			return linkList;
		}

		private static void AddLink(List<Transform> linkList, Transform link)
		{
			if(link!=null)
			{
				linkList.Add(link);
			}
		}

		public static float GetMaxJointVal(Joint joint)
		{
			return jointRangeMap[joint].max;
		}

		public static float GetMinJointVal(Joint joint)
		{
			return jointRangeMap[joint].min;
		}

		public static float GetClampedPosition(float value, Joint joint)
		{
			switch(joint)
			{
				case Joint.torso_lift_joint: 
				case Joint.head_1_joint: 
				case Joint.head_2_joint: 
				case Joint.arm_1_joint: 
				case Joint.arm_2_joint: 
				case Joint.arm_3_joint: 
				case Joint.arm_4_joint: 
				case Joint.arm_5_joint:
				case Joint.arm_6_joint:
				case Joint.arm_7_joint:
				case Joint.gripper_right_finger_joint:
				case Joint.gripper_left_finger_joint:
				{
					return Mathf.Clamp(value, GetMinJointVal(joint), GetMaxJointVal(joint));
				}
			}

			return value;
		}

		//public static float GetClampedEulerAngle(float value, Joint joint)
		//{
		//	return HSRCommon.GetClampedPosition(value * Mathf.Deg2Rad, joint) * Mathf.Rad2Deg;
		//}

		public static float GetNormalizedJointEulerAngle(float value, Joint joint)
		{
			switch(joint)
			{
				case Joint.head_1_joint: 
				case Joint.head_2_joint: 
				case Joint.arm_1_joint: 
				case Joint.arm_2_joint: 
				case Joint.arm_3_joint: 
				case Joint.arm_4_joint: 
				case Joint.arm_5_joint:
				case Joint.arm_6_joint:
				case Joint.arm_7_joint:
				{
					return GetCorrectedEulerAngle(value, GetMinJointVal(joint) * Mathf.Rad2Deg, GetMaxJointVal(joint) * Mathf.Rad2Deg);
				}
			}

			return value;
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue)
		{
			float play = 5.0f;

			value = (value > maxValue + play) ? value - 360f : value;
			value = (value < minValue - play) ? value + 360f : value;
			
			return value;
		}

		public static float GetMaxJointSpeed(Joint joint)
		{
			switch(joint)
			{
				case Joint.torso_lift_joint:  { return 0.07f; } // [m/s] TODO
				case Joint.head_1_joint:
				case Joint.head_2_joint:  { return 3.0f; } // [rad/s] TODO
				case Joint.arm_1_joint:
				case Joint.arm_2_joint:  { return 1.95f; } // [rad/s] TODO
				case Joint.arm_3_joint:
				case Joint.arm_4_joint:  { return 2.35f; } // [rad/s] TODO
				case Joint.arm_5_joint:  { return 1.95f; } // [rad/s] TODO
				case Joint.arm_6_joint:
				case Joint.arm_7_joint:  { return 1.76f; } // [rad/s] TODO
				case Joint.gripper_right_finger_joint:
				case Joint.gripper_left_finger_joint:   { return 0.07f; } // [m/s] TODO
				default:                     { throw new Exception("Unknown Joint. name=" + joint.ToString());}
			}
		}

		public static float GetMinJointSpeed(Joint joint)
		{
			if(joint==Joint.torso_lift_joint || joint==Joint.gripper_right_finger_joint || joint==Joint.gripper_left_finger_joint)
			{
				return 0.001f; // [m/s] TODO
			}
			else
			{
				return 0.01f;  // [rad/s] TODO
			}
		}
	}
}

