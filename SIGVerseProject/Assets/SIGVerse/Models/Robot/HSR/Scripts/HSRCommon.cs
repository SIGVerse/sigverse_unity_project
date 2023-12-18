using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ToyotaHSR
{
	public class HSRCommon
	{
		public const float MaxSpeedBase    = 0.3f;  // [m/s]
		public const float MaxSpeedBaseRad = 1.1f;  // [rad/s]

		public const string OdomName   = "odom";

		public const string BaseName   = "Base";
		public const string BumperName = "Bumper";

		// Link Names for Noise
		public const string BaseFootPrintPosNoiseName  = "base_footprint_pos_noise";
		public const string BaseFootPrintRotNoiseName  = "base_footprint_rot_noise";
		public const string BaseFootPrintRigidbodyName = "base_footprint_rigidbody";

		public enum Link
		{
			base_footprint,
			base_link,

			arm_lift_link,
			arm_flex_link,
			arm_roll_link,
			wrist_flex_link,
			wrist_roll_link,
			hand_palm_link,
			hand_camera_frame,
			hand_camera_rgb_frame,
			hand_l_proximal_link,
			hand_l_spring_proximal_link,
			hand_l_mimic_distal_link,
			hand_l_distal_link,
			hand_l_finger_tip_frame,
			hand_l_finger_vacuum_frame,
			hand_motor_dummy_link,
			hand_r_proximal_link,
			hand_r_spring_proximal_link,
			hand_r_mimic_distal_link,
			hand_r_distal_link,
			hand_r_finger_tip_frame,
			wrist_ft_sensor_frame,

			base_imu_frame,
			base_range_sensor_link,
			base_roll_link,
			base_l_drive_wheel_link,
			base_l_passive_wheel_x_frame,
			base_l_passive_wheel_y_frame,
			base_l_passive_wheel_z_link,
			base_r_drive_wheel_link,
			base_r_passive_wheel_x_frame,
			base_r_passive_wheel_y_frame,
			base_r_passive_wheel_z_link,

			torso_lift_link,

			head_pan_link,
			head_tilt_link,
			head_center_camera_frame,
			head_center_camera_rgb_frame,
			head_l_stereo_camera_link,
			head_l_stereo_camera_rgb_frame,
			head_r_stereo_camera_link,
			head_r_stereo_camera_rgb_frame,
			head_rgbd_sensor_link,
			head_rgbd_sensor_rgb_frame,
			head_rgbd_sensor_depth_frame,
		}

		public enum Joint
		{
			base_roll_joint,
			base_r_drive_wheel_joint,
			base_l_drive_wheel_joint,
			base_r_passive_wheel_x_frame_joint,
			base_r_passive_wheel_y_frame_joint,
			base_r_passive_wheel_z_joint,
			base_l_passive_wheel_x_frame_joint,
			base_l_passive_wheel_y_frame_joint,
			base_l_passive_wheel_z_joint,
			base_f_bumper_joint,
			base_b_bumper_joint,
			arm_lift_joint,
			arm_flex_joint,
			arm_roll_joint,
			wrist_flex_joint,
			wrist_roll_joint,
			wrist_ft_sensor_frame_joint,
			head_pan_joint,
			head_tilt_joint,
			torso_lift_joint,
			hand_motor_joint,
			hand_l_proximal_joint,
			hand_l_spring_proximal_joint,
			hand_l_mimic_distal_joint,
			hand_l_distal_joint,
			hand_r_proximal_joint,
			hand_r_spring_proximal_joint,
			hand_r_mimic_distal_joint,
			hand_r_distal_joint,
			odom_x,
			odom_y,
			odom_r,
			odom_t,
		}

		public static Dictionary<Joint, Vector3> JointAxis = new Dictionary<Joint, Vector3>()
		{
			{ Joint.arm_lift_joint,               Vector3.forward },
			{ Joint.arm_flex_joint,               Vector3.up },
			{ Joint.arm_roll_joint,               Vector3.back },
			{ Joint.wrist_flex_joint,             Vector3.up },
			{ Joint.wrist_roll_joint,             Vector3.back },
			{ Joint.head_pan_joint,               Vector3.back },
			{ Joint.head_tilt_joint,              Vector3.up },
			{ Joint.torso_lift_joint,             Vector3.forward },
			{ Joint.hand_motor_joint,             Vector3.right },
			{ Joint.hand_l_proximal_joint,        Vector3.right },
			{ Joint.hand_l_spring_proximal_joint, Vector3.right },
			{ Joint.hand_l_mimic_distal_joint,    Vector3.right },
			{ Joint.hand_l_distal_joint,          Vector3.right },
			{ Joint.hand_r_proximal_joint,        Vector3.left },
			{ Joint.hand_r_spring_proximal_joint, Vector3.left },
			{ Joint.hand_r_mimic_distal_joint,    Vector3.left },
			{ Joint.hand_r_distal_joint,          Vector3.left },
			{ Joint.odom_x,                       Vector3.right },
			{ Joint.odom_y,                       Vector3.up },
			{ Joint.odom_t,                       Vector3.forward },
		};

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
			{ Joint.arm_lift_joint,   new JointRange( +0.000f, +0.690f) },
			{ Joint.arm_flex_joint,   new JointRange( -2.617f, +0.000f) },
			{ Joint.arm_roll_joint,   new JointRange( -1.919f, +3.665f) },
			{ Joint.wrist_flex_joint, new JointRange( -1.919f, +1.221f) },
			{ Joint.wrist_roll_joint, new JointRange( -1.919f, +3.665f) },
			{ Joint.head_pan_joint,   new JointRange( -3.839f, +1.745f) },
			{ Joint.head_tilt_joint,  new JointRange( -1.570f, +0.523f) },
			{ Joint.hand_motor_joint, new JointRange( -0.105f, +1.239f) },
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
				case Joint.arm_lift_joint: 
				case Joint.arm_flex_joint: 
				case Joint.arm_roll_joint: 
				case Joint.wrist_flex_joint: 
				case Joint.wrist_roll_joint: 
				case Joint.head_pan_joint: 
				case Joint.head_tilt_joint: 
				case Joint.hand_motor_joint:
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
				case Joint.arm_flex_joint: 
				case Joint.arm_roll_joint: 
				case Joint.wrist_flex_joint: 
				case Joint.wrist_roll_joint: 
				case Joint.head_pan_joint: 
				case Joint.head_tilt_joint: 
				case Joint.hand_motor_joint:
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

		public static double GetNormalizedJointRosAngle(Transform link, Joint joint)
		{
			// Check to be sure
			float unityAngle;
			Vector3 unityAxis;
			link.localRotation.ToAngleAxis(out unityAngle, out unityAxis);

			if(Mathf.Abs(unityAngle) > 1.0f && Vector3.Distance(Abs(unityAxis), Abs(JointAxis[joint]))>0.01f)
			{
				SIGVerseLogger.Warn("Illegal JointAxis. JointName=" + joint.ToString() + ", unityAngle="+unityAngle+ ", unityAxis="+unityAxis.ToString("F4")+", JointAxis="+JointAxis[joint].ToString("F4"));
			}

			float eulerAngle;

			if (Mathf.Abs(JointAxis[joint].x)==1)
			{
				eulerAngle = link.localEulerAngles.x * JointAxis[joint].x;
			}
			else if(Mathf.Abs(JointAxis[joint].y)==1)
			{
				eulerAngle = link.localEulerAngles.y * JointAxis[joint].y;
			}
			else if(Mathf.Abs(JointAxis[joint].z)==1)
			{
				eulerAngle = link.localEulerAngles.z * JointAxis[joint].z;
			}
			else
			{
				Debug.Log("Illegal JointAxis="+JointAxis[joint]);
				throw new Exception("Illegal JointAxis="+JointAxis[joint]);
			}

			return GetNormalizedJointEulerAngle(eulerAngle, joint) * Mathf.Deg2Rad;
		}

		public static Vector3 Abs(Vector3 vec)
		{
			return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
		}


		public static float GetMaxJointSpeed(Joint joint)
		{
			switch(joint)
			{
				case Joint.arm_lift_joint:   { return 0.15f; } // [m/s]
				case Joint.arm_flex_joint:
				case Joint.arm_roll_joint:
				case Joint.wrist_flex_joint:
				case Joint.wrist_roll_joint:
				case Joint.head_pan_joint:
				case Joint.head_tilt_joint:  { return 1.0f; } // [rad/s] 
				case Joint.hand_motor_joint: { return 6.0f; } // [rad/s] 
				default:                     { throw new Exception("Unknown Joint. name=" + joint.ToString());}
			}
		}

		public static float GetMinJointSpeed(Joint joint)
		{
			if(joint==Joint.arm_lift_joint)
			{
				return 0.001f; // [m/s]
			}
			else
			{
				return 0.01f;  // [rad/s]
			}
		}
	}
}

