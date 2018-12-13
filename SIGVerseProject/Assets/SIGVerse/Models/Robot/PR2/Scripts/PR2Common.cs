using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.PR2
{
	public class PR2Common
	{
		public const float MaxSpeedBase    = 1.0f;  // [m/s]
		public const float MaxSpeedBaseRad = 3.3f;  // [rad/s] Tentative 

		public enum Link
		{
			base_footprint,
			base_link,

			bl_caster_rotation_link,
			br_caster_rotation_link,
			fl_caster_rotation_link,
			fr_caster_rotation_link,
			bl_caster_l_wheel_link,
			bl_caster_r_wheel_link,
			br_caster_l_wheel_link,
			br_caster_r_wheel_link,
			fl_caster_l_wheel_link,
			fl_caster_r_wheel_link,
			fr_caster_l_wheel_link,
			fr_caster_r_wheel_link,

			torso_lift_link,
			torso_lift_motor_screw_link,
			base_bellow_link,
			base_laser_link,
			head_pan_link,
			head_tilt_link,
			head_plate_frame,

			projector_wg6802418_frame,
			projector_wg6802418_child_frame,
			sensor_mount_link,
			double_stereo_link,
			narrow_stereo_link,
			narrow_stereo_optical_frame,
			narrow_stereo_l_stereo_camera_frame,
			narrow_stereo_r_stereo_camera_frame,
			narrow_stereo_l_stereo_camera_optical_frame,
			narrow_stereo_r_stereo_camera_optical_frame,
			wide_stereo_link,
			wide_stereo_optical_frame,
			wide_stereo_l_stereo_camera_frame,
			wide_stereo_r_stereo_camera_frame,
			wide_stereo_l_stereo_camera_optical_frame,
			wide_stereo_r_stereo_camera_optical_frame,
			high_def_frame,
			high_def_optical_frame,

			imu_link,
			l_torso_lift_side_plate_link,
			r_torso_lift_side_plate_link,
			laser_tilt_mount_link,
			laser_tilt_link,

			r_shoulder_pan_link,
			r_shoulder_lift_link,
			r_upper_arm_roll_link,
			r_upper_arm_link,
			r_elbow_flex_link,
			r_forearm_roll_link,
			r_forearm_cam_frame,
			r_forearm_cam_optical_frame,
			r_forearm_link,

			r_wrist_flex_link,
			r_wrist_roll_link,
			r_gripper_palm_link,
			r_gripper_l_finger_link,
			r_gripper_l_finger_tip_link,
			r_gripper_r_finger_link,
			r_gripper_r_finger_tip_link,
			r_gripper_tool_frame,
			r_gripper_led_frame,
			r_gripper_motor_accelerometer_link,
			r_gripper_motor_slider_link,
			r_gripper_motor_screw_link,

			l_shoulder_pan_link,
			l_shoulder_lift_link,
			l_upper_arm_roll_link,
			l_upper_arm_link,
			l_elbow_flex_link,
			l_forearm_roll_link,
			l_forearm_cam_frame,
			l_forearm_cam_optical_frame,
			l_forearm_link,

			l_wrist_flex_link,
			l_wrist_roll_link,
			l_gripper_palm_link,
			l_gripper_l_finger_link,
			l_gripper_l_finger_tip_link,
			l_gripper_r_finger_link,
			l_gripper_r_finger_tip_link,
			l_gripper_tool_frame,
			l_gripper_led_frame,
			l_gripper_motor_accelerometer_link,
			l_gripper_motor_slider_link,
			l_gripper_motor_screw_link,
		}

		public enum Joint
		{
			bl_caster_rotation_joint,
			bl_caster_l_wheel_joint,
			bl_caster_r_wheel_joint,
			br_caster_rotation_joint,
			br_caster_l_wheel_joint,
			br_caster_r_wheel_joint,
			fl_caster_rotation_joint,
			fl_caster_l_wheel_joint,
			fl_caster_r_wheel_joint,
			fr_caster_rotation_joint,
			fr_caster_l_wheel_joint,
			fr_caster_r_wheel_joint,

			torso_lift_joint,
			torso_lift_motor_screw_joint,
			head_pan_joint,
			head_tilt_joint,
			laser_tilt_mount_joint,

			r_shoulder_pan_joint,
			r_shoulder_lift_joint,
			r_upper_arm_roll_joint,
			r_elbow_flex_joint,
			r_forearm_roll_joint,
			r_wrist_flex_joint,
			r_wrist_roll_joint,
			r_gripper_motor_slider_joint,
			r_gripper_motor_screw_joint,
			r_gripper_l_finger_joint,
			r_gripper_joint,

			l_shoulder_pan_joint,
			l_shoulder_lift_joint,
			l_upper_arm_roll_joint,
			l_elbow_flex_joint,
			l_forearm_roll_joint,
			l_wrist_flex_joint,
			l_wrist_roll_joint,
			l_gripper_motor_slider_joint,
			l_gripper_motor_screw_joint,
			l_gripper_l_finger_joint,
			l_gripper_joint,
		}


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

		public static float GetClampedPosition(float value, Joint joint)
		{
			switch(joint)
			{
				case Joint.torso_lift_joint:            { return Mathf.Clamp(value, +0.01f, +0.33f); }
				case Joint.torso_lift_motor_screw_joint:{ return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf??
				case Joint.head_pan_joint:              { return Mathf.Clamp(value, -2.86f, +2.86f); }
				case Joint.head_tilt_joint:             { return Mathf.Clamp(value, -0.37f, +1.30f); }
				case Joint.laser_tilt_mount_joint:      { return Mathf.Clamp(value, -0.74f, +1.43f); }
				case Joint.r_shoulder_pan_joint:        { return Mathf.Clamp(value, -2.14f, +0.56f); }
				case Joint.r_shoulder_lift_joint:       { return Mathf.Clamp(value, -0.35f, +1.30f); }
				case Joint.r_upper_arm_roll_joint:      { return Mathf.Clamp(value, -3.75f, +0.65f); }
				case Joint.r_elbow_flex_joint:          { return Mathf.Clamp(value, -2.12f, -0.15f); }
				case Joint.r_forearm_roll_joint:        { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf
				case Joint.r_wrist_flex_joint:          { return Mathf.Clamp(value, -2.00f, -0.10f); }
				case Joint.r_wrist_roll_joint:          { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf
				case Joint.r_gripper_motor_slider_joint:{ return Mathf.Clamp(value, -0.10f, +0.10f); }
				case Joint.r_gripper_motor_screw_joint: { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf??
				case Joint.r_gripper_l_finger_joint:    { return Mathf.Clamp(value, +0.00f, +0.55f); }
				case Joint.r_gripper_joint:             { return Mathf.Clamp(value, +0.00f, +0.09f); }
				case Joint.l_shoulder_pan_joint:        { return Mathf.Clamp(value, -0.56f, +2.14f); }
				case Joint.l_shoulder_lift_joint:       { return Mathf.Clamp(value, -0.35f, +1.30f); }
				case Joint.l_upper_arm_roll_joint:      { return Mathf.Clamp(value, -0.65f, +3.75f); }
				case Joint.l_elbow_flex_joint:          { return Mathf.Clamp(value, -2.12f, -0.15f); }
				case Joint.l_forearm_roll_joint:        { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf
				case Joint.l_wrist_flex_joint:          { return Mathf.Clamp(value, -2.00f, -0.10f); }
				case Joint.l_wrist_roll_joint:          { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf
				case Joint.l_gripper_motor_slider_joint:{ return Mathf.Clamp(value, -0.10f, +0.10f); }
				case Joint.l_gripper_motor_screw_joint: { return Mathf.Clamp(value, -3.15f, +3.15f); } // Inf??
				case Joint.l_gripper_l_finger_joint:    { return Mathf.Clamp(value, +0.00f, +0.55f); }
				case Joint.l_gripper_joint:             { return Mathf.Clamp(value, +0.00f, +0.09f); }
				default: { throw new Exception("Unknown Joint. name=" + joint.ToString());}
			}
		}

		public static float GetClampedEulerAngle(float value, Joint joint)
		{
			return PR2Common.GetClampedPosition(value * Mathf.Deg2Rad, joint) * Mathf.Rad2Deg;
		}

		public static float GetMaxJointSpeed(Joint joint)
		{
			if(joint==Joint.torso_lift_joint)
			{
				return 0.001f;
			}
			else
			{
				return 0.01f;
			}
		}

		public static float GetMinJointSpeed(Joint joint)
		{
			switch(joint)
			{
				case Joint.torso_lift_joint:            { return 0.013f; }

				case Joint.head_pan_joint:              { return 6.0f; }
				case Joint.head_tilt_joint:             { return 5.0f; }
				case Joint.laser_tilt_mount_joint:      { return 10.0f; } // mount??
				
				case Joint.r_shoulder_pan_joint:        { return 2.1f; }
				case Joint.r_shoulder_lift_joint:       { return 2.1f; }
				case Joint.r_upper_arm_roll_joint:      { return 3.27f; }
				case Joint.r_elbow_flex_joint:          { return 3.3f; }
				case Joint.r_forearm_roll_joint:        { return 3.6f; } // Inf
				case Joint.r_wrist_flex_joint:          { return 3.1f; }
				case Joint.r_wrist_roll_joint:          { return 3.6f; } // Inf
//				case Joint.r_gripper_motor_slider_joint:{ return ; }
//				case Joint.r_gripper_motor_screw_joint: { return ; }
//				case Joint.r_gripper_l_finger_joint:    { return ; }
				case Joint.r_gripper_joint:             { return 0.2f; }
				
				case Joint.l_shoulder_pan_joint:        { return 2.1f; }
				case Joint.l_shoulder_lift_joint:       { return 2.1f; }
				case Joint.l_upper_arm_roll_joint:      { return 3.27f; }
				case Joint.l_elbow_flex_joint:          { return 3.3f; }
				case Joint.l_forearm_roll_joint:        { return 3.6f; }
				case Joint.l_wrist_flex_joint:          { return 3.1f; }
				case Joint.l_wrist_roll_joint:          { return 3.6f; }
//				case Joint.l_gripper_motor_slider_joint:{ return ; }
//				case Joint.l_gripper_motor_screw_joint: { return ; }
//				case Joint.l_gripper_l_finger_joint:    { return ; }
				case Joint.l_gripper_joint:             { return 0.2f; }
				default: { throw new Exception("Unknown Joint. name=" + joint.ToString());}
			}
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue)
		{
			float Play = 5.0f;
			value = (value > maxValue + Play) ? value - 360f : value;
			value = (value < minValue - Play) ? value + 360f : value;
//			Debug.Log("roll=" + value);
			return value;
		}
	}
}

