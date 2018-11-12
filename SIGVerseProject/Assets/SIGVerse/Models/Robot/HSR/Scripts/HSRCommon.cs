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
		public const float MaxSpeedTorso   = 0.15f; // [m/s]
		public const float MaxSpeedArm     = 1.0f;  // [rad/s] 
		public const float MaxSpeedHead    = 1.0f;  // [rad/s]
		public const float MaxSpeedHand    = 6.0f;  // [rad/s] 
		public const float MinSpeed        = 0.001f;// [m/s]
		public const float MinSpeedRad     = 0.01f; // [rad/s]

		// Link names
		// TODO Want to change into Enum.
//		public const string MapName           = "map";
		public const string OdomName          = "odom";
		public const string BaseFootPrintName = "base_footprint";
		public const string BaseLinkName      = "base_link";

		public const string BaseName   = "Base";
		public const string BumperName = "Bumper";

		public const string ArmLiftLinkName             = "arm_lift_link";
		public const string ArmFlexLinkName             = "arm_flex_link";
		public const string ArmRollLinkName             = "arm_roll_link";
		public const string WristFlexLinkName           = "wrist_flex_link";
		public const string WristRollLinkName           = "wrist_roll_link";
		public const string HandPalmLinkName            = "hand_palm_link";
		public const string HandCameraFrameName         = "hand_camera_frame";
//		public const string HandCameraGazeboFrameName   = "hand_camera_gazebo_frame";
		public const string HandCameraRgbFrameName      = "hand_camera_rgb_frame";
		public const string HandLProximalLinkName       = "hand_l_proximal_link";
		public const string HandLSpringProximalLinkName = "hand_l_spring_proximal_link";
		public const string HandLMimicDistalLinkName    = "hand_l_mimic_distal_link";
		public const string HandLDistalLinkName         = "hand_l_distal_link";
		public const string HandLFingerTipFrameName     = "hand_l_finger_tip_frame";
		public const string HandLFingerVacuumFrameName  = "hand_l_finger_vacuum_frame";
		public const string HandMotorDummyLinkName      = "hand_motor_dummy_link";
		public const string HandRProximalLinkName       = "hand_r_proximal_link";
		public const string HandRSpringProximalLinkName = "hand_r_spring_proximal_link";
		public const string HandRMimicDistalLinkName    = "hand_r_mimic_distal_link";
		public const string HandRDistalLinkName         = "hand_r_distal_link";
		public const string HandRFingerTipFrameName     = "hand_r_finger_tip_frame";
		public const string WristFtSensorFrameName      = "wrist_ft_sensor_frame";

		public const string BaseImuFrameName    = "base_imu_frame";
		public const string BaseRangeSensorLink = "base_range_sensor_link";

		public const string BaseRollLlinkName           = "base_roll_link";
		public const string BaseLDriveWheelLinkName     = "base_l_drive_wheel_link";
		public const string BaseLPassiveWheelXFrameName = "base_l_passive_wheel_x_frame";
		public const string BaseLPassiveWheelYFrameName = "base_l_passive_wheel_y_frame";
		public const string BaseLPassiveWheelZLinkName  = "base_l_passive_wheel_z_link";
		public const string BaseRDriveWheelLinkName     = "base_r_drive_wheel_link";
		public const string BaseRPassiveWheelXFramName  = "base_r_passive_wheel_x_frame";
		public const string BaseRPassiveWheelYFrameName = "base_r_passive_wheel_y_frame";
		public const string BaseRPassiveWheelZLinkName  = "base_r_passive_wheel_z_link";

		public const string TorsoLiftLinkName                = "torso_lift_link";
		public const string HeadPanLinkName                  = "head_pan_link";
		public const string HeadTiltLinkName                 = "head_tilt_link";
		public const string HeadCenterCameraFrameName        = "head_center_camera_frame";
//		public const string HeadCenterCameraGazeboFrameName  = "head_center_camera_gazebo_frame";
		public const string HeadCenterCameraRgbFrameName     = "head_center_camera_rgb_frame";
		public const string HeadLStereoCameraLinkName        = "head_l_stereo_camera_link";
//		public const string HeadLStereoCameraGazeboFrameName = "head_l_stereo_camera_gazebo_frame";
		public const string HeadLStereoCameraRgbFrameName    = "head_l_stereo_camera_rgb_frame";
		public const string HeadRStereoCameraLinkName        = "head_r_stereo_camera_link";
//		public const string HeadRStereoCameraGazeboFrameName = "head_r_stereo_camera_gazebo_frame";
		public const string HeadRStereoCameraRgbFrameName    = "head_r_stereo_camera_rgb_frame";
		public const string HeadRgbdSensorLinkName           = "head_rgbd_sensor_link";
//		public const string HeadRgbdSensorGazeboFrameName    = "head_rgbd_sensor_gazebo_frame";
		public const string HeadRgbdSensorRgbFrameName       = "head_rgbd_sensor_rgb_frame";
		public const string HeadRgbdSensorDepthFrameName     = "head_rgbd_sensor_depth_frame";

		// Link Names for Noise
		public const string BaseFootPrintPosNoiseName  = "base_footprint_pos_noise";
		public const string BaseFootPrintRotNoiseName  = "base_footprint_rot_noise";
		public const string BaseFootPrintRigidbodyName = "base_footprint_rigidbody";

		// Joint names
		public const string ArmLiftJointName             = "arm_lift_joint";
		public const string ArmFlexJointName             = "arm_flex_joint";
		public const string ArmRollJointName             = "arm_roll_joint";
		public const string WristFlexJointName           = "wrist_flex_joint";
		public const string WristRollJointName           = "wrist_roll_joint";
		public const string HeadPanJointName             = "head_pan_joint";
		public const string HeadTiltJointName            = "head_tilt_joint";
		public const string TorsoLiftJointName           = "torso_lift_joint";
		public const string HandLProximalJointName       = "hand_l_proximal_joint";
		public const string HandRProximalJointName       = "hand_r_proximal_joint";
		public const string HandLSpringProximalJointName = "hand_l_spring_proximal_joint";
		public const string HandRSpringProximalJointName = "hand_r_spring_proximal_joint";
		public const string HandMotorJointName           = "hand_motor_joint";
		public const string OmniOdomXJointName           = "odom_x";
		public const string OmniOdomYJointName           = "odom_y";
		public const string OmniOdomTJointName           = "odom_t";

		public static List<Transform> GetLinksInChildren(Transform root)
		{
			List<Transform> linkList = new List<Transform>();

			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseFootPrintName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, ArmLiftLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, ArmFlexLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, ArmRollLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, WristFlexLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, WristRollLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandPalmLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandCameraFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandCameraRgbFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLProximalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLSpringProximalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLMimicDistalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLDistalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLFingerTipFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandLFingerVacuumFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandMotorDummyLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandRProximalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandRSpringProximalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandRMimicDistalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandRDistalLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HandRFingerTipFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, WristFtSensorFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseImuFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRangeSensorLink));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRollLlinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseLDriveWheelLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseLPassiveWheelXFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseLPassiveWheelYFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseLPassiveWheelZLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRDriveWheelLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRPassiveWheelXFramName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRPassiveWheelYFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, BaseRPassiveWheelZLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, TorsoLiftLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadPanLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadTiltLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadCenterCameraFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadCenterCameraRgbFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadLStereoCameraLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadLStereoCameraRgbFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadRStereoCameraLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadRStereoCameraRgbFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadRgbdSensorLinkName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadRgbdSensorRgbFrameName));
			AddLink(linkList, SIGVerseUtils.FindTransformFromChild(root, HeadRgbdSensorDepthFrameName));

			return linkList;
		}

		private static void AddLink(List<Transform> linkList, Transform link)
		{
			if(link!=null)
			{
				linkList.Add(link);
			}
		}

		public static float GetClampedPosition(float value, string name)
		{
			if (name == HSRCommon.ArmLiftJointName)       { return Mathf.Clamp(value, +0.000f, +0.690f); }
			if (name == HSRCommon.ArmFlexJointName)       { return Mathf.Clamp(value, -2.617f, +0.000f); }
			if (name == HSRCommon.ArmRollJointName)       { return Mathf.Clamp(value, -1.919f, +3.665f); }
			if (name == HSRCommon.WristFlexJointName)     { return Mathf.Clamp(value, -1.919f, +1.221f); }
			if (name == HSRCommon.WristRollJointName)     { return Mathf.Clamp(value, -1.919f, +3.665f); }
			if (name == HSRCommon.HeadPanJointName)       { return Mathf.Clamp(value, -3.839f, +1.745f); }
			if (name == HSRCommon.HeadTiltJointName)      { return Mathf.Clamp(value, -1.570f, +0.523f); }
			if (name == HSRCommon.HandMotorJointName)     { return Mathf.Clamp(value, -0.1f, +1.24f); }

			return value;
		}

		public static float GetCorrectedJointsEulerAngle(float value, string name)
		{
			if (name == HSRCommon.ArmFlexJointName)       { value = GetCorrectedEulerAngle(value, -150f,   0f); }
			if (name == HSRCommon.ArmRollJointName)       { value = GetCorrectedEulerAngle(value, -110f, 210f); }
			if (name == HSRCommon.WristFlexJointName)     { value = GetCorrectedEulerAngle(value, -110f,  70f); }
			if (name == HSRCommon.WristRollJointName)     { value = GetCorrectedEulerAngle(value, -110f, 210f); }
			if (name == HSRCommon.HeadPanJointName)       { value = GetCorrectedEulerAngle(value, -220f, 100f); }
			if (name == HSRCommon.HeadTiltJointName)      { value = GetCorrectedEulerAngle(value, -90f,   30f); }
			if (name == HSRCommon.HandMotorJointName)     { value = GetCorrectedEulerAngle(value, -5f,    71f); }
			return value;
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue)
		{
			float Play = 5.0f;
			value = (value > maxValue + Play) ? value - 360f : value;
			value = (value < minValue - Play) ? value + 360f : value;
//			Debug.Log("roll=" + value);
			return value;
		}

		//public static Vector3 ConvertPositionHSRToUnity(Vector3 position)
		//{
		//	return new Vector3(-position.y, position.z, position.x);
		//}

		//public static Vector3 ConvertPositionUnityToHSR(Vector3 position)
		//{
		//	return new Vector3(position.z, -position.x, position.y);
		//}
	}
}

