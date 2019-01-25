using UnityEngine;
using SIGVerse.Common;
using System.Collections.Generic;
using System;
using static SIGVerse.PR2.PR2Common;

namespace SIGVerse.PR2
{
	public class PR2PoseInitializer : MonoBehaviour
	{
		private Transform torsoLiftLink;

		private Transform headPanLink;
		private Transform headTiltLink;
		//private Transform laserTiltMountLink;

		private Transform rUpperArmRollLink;
		private Transform rShoulderPanLink;
		private Transform rShoulderLiftLink;
		private Transform rForearmRollLink;
		private Transform rElbowFlexLink;
		private Transform rWristFlexLink;
		private Transform rWristRollLink;
		//private Transform rGripperPalmLink;
		//private Transform rGripperLFingerLink;
		//private Transform rGripperRFingerLink;
		//private Transform rGripperRFingerTipLink;
		//private Transform rGripperLFingerTipLink;

		private Transform lUpperArmRollLink;
		private Transform lShoulderPanLink;
		private Transform lShoulderLiftLink;
		private Transform lForearmRollLink;
		private Transform lElbowFlexLink;
		private Transform lWristFlexLink;
		private Transform lWristRollLink;
		//private Transform lGripperPalmLink;
		//private Transform lGripperLFingerLink;
		//private Transform lGripperRFingerLink;
		//private Transform lGripperRFingerTipLink;
		//private Transform lGripperLFingerTipLink;

		void Awake()
		{
			this.torsoLiftLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link      .ToString());

			this.headPanLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link        .ToString());
			this.headTiltLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link       .ToString());
			//this.laserTiltMountLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.laser_tilt_mount_link.ToString());

			this.rUpperArmRollLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_upper_arm_roll_link      .ToString());
			this.rShoulderPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_shoulder_pan_link        .ToString());
			this.rShoulderLiftLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_shoulder_lift_link       .ToString());
			this.rForearmRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_forearm_roll_link        .ToString());
			this.rElbowFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_elbow_flex_link          .ToString());
			this.rWristFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_wrist_flex_link          .ToString());
			this.rWristRollLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_wrist_roll_link          .ToString());
			//this.rGripperPalmLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_palm_link        .ToString());
			//this.rGripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_link    .ToString());
			//this.rGripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_link    .ToString());
			//this.rGripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_tip_link.ToString());
			//this.rGripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_tip_link.ToString());

			this.lUpperArmRollLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_upper_arm_roll_link      .ToString());
			this.lShoulderPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_shoulder_pan_link        .ToString());
			this.lShoulderLiftLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_shoulder_lift_link       .ToString());
			this.lForearmRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_forearm_roll_link        .ToString());
			this.lElbowFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_elbow_flex_link          .ToString());
			this.lWristFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_wrist_flex_link          .ToString());
			this.lWristRollLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_wrist_roll_link          .ToString());
			//this.lGripperPalmLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_palm_link        .ToString());
			//this.lGripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_link    .ToString());
			//this.lGripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_link    .ToString());
			//this.lGripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_tip_link.ToString());
			//this.lGripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_tip_link.ToString());
		}

		void Start()
		{
			this.InitializeLinkPosition(this.torsoLiftLink, Vector3.forward, 0.011f);

			this.InitializeLinkAngle(this.headPanLink,   Vector3.back, -0.046f);
			this.InitializeLinkAngle(this.headTiltLink,  Vector3.down, -0.099f);

			this.InitializeLinkAngle(this.lShoulderPanLink,  Vector3.back,   0.700f);
			this.InitializeLinkAngle(this.lShoulderLiftLink, Vector3.down,   1.000f);
			this.InitializeLinkAngle(this.lUpperArmRollLink, Vector3.right,  0.094f);
			this.InitializeLinkAngle(this.lElbowFlexLink,    Vector3.down,  -1.963f);
			this.InitializeLinkAngle(this.lForearmRollLink,  Vector3.right,  0.146f);
			this.InitializeLinkAngle(this.lWristFlexLink,    Vector3.down,  -0.092f);
			this.InitializeLinkAngle(this.lWristRollLink,    Vector3.right, -1.474f);

			this.InitializeLinkAngle(this.rShoulderPanLink,  Vector3.back,  -0.700f);
			this.InitializeLinkAngle(this.rShoulderLiftLink, Vector3.down,   0.973f);
			this.InitializeLinkAngle(this.rUpperArmRollLink, Vector3.right, -0.001f);
			this.InitializeLinkAngle(this.rElbowFlexLink,    Vector3.down,  -1.653f);
			this.InitializeLinkAngle(this.rForearmRollLink,  Vector3.right,  0.090f);
			this.InitializeLinkAngle(this.rWristFlexLink,    Vector3.down,  -0.090f);
			this.InitializeLinkAngle(this.rWristRollLink,    Vector3.right, -1.378f);
		}

		private void InitializeLinkPosition(Transform link, Vector3 axis, float position)
		{
			if(Mathf.Abs(axis.x)==1)
			{
				link.localPosition += position * axis;
			}
			else if(Mathf.Abs(axis.y)==1)
			{
				link.localPosition += position * axis;
			}
			else if(Mathf.Abs(axis.z)==1)
			{
				link.localPosition += position * axis;
			}
		}

		private void InitializeLinkAngle(Transform link, Vector3 axis, float eulerAngle)
		{
			if(Mathf.Abs(axis.x)==1)
			{
				link.localEulerAngles = eulerAngle * axis * Mathf.Rad2Deg;
			}
			else if(Mathf.Abs(axis.y)==1)
			{
				link.localEulerAngles = eulerAngle * axis * Mathf.Rad2Deg;
			}
			else if(Mathf.Abs(axis.z)==1)
			{
				link.localEulerAngles = eulerAngle * axis * Mathf.Rad2Deg;
			}
		}
	}
}

