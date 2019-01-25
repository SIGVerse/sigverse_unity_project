using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;
using System;
using static SIGVerse.PR2.PR2Common;

namespace SIGVerse.PR2
{
	public class PR2PubJointState : RosPubMessage<JointState>
	{
		private class ContinuousJointInfo
		{
			public PR2Common.Joint Joint { get; set; }
			public int     SeqNo         { get; set; }
			public double  PreviousPos   { get; set; }
			public int     RotationCount { get; set; }

			public ContinuousJointInfo(PR2Common.Joint joint, int listNum)
			{
				this.Joint       = joint;
				this.SeqNo       = listNum;
				this.PreviousPos = 0.0;
				this.RotationCount = 0;
			}
		}

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		private List<ContinuousJointInfo> continuousJointInfoList = new List<ContinuousJointInfo>();

		private Transform torsoLiftLink;
		private Transform headPanLink;
		private Transform headTiltLink;
		private Transform laserTiltMountLink;

		private Transform rUpperArmRollLink;
		private Transform rShoulderPanLink;
		private Transform rShoulderLiftLink;
		private Transform rForearmRollLink;
		private Transform rElbowFlexLink;
		private Transform rWristFlexLink;
		private Transform rWristRollLink;
		private Transform rGripperPalmLink;
		private Transform rGripperLFingerLink;
		private Transform rGripperRFingerLink;
		private Transform rGripperRFingerTipLink;
		private Transform rGripperLFingerTipLink;

		private Transform lUpperArmRollLink;
		private Transform lShoulderPanLink;
		private Transform lShoulderLiftLink;
		private Transform lForearmRollLink;
		private Transform lElbowFlexLink;
		private Transform lWristFlexLink;
		private Transform lWristRollLink;
		private Transform lGripperPalmLink;
		private Transform lGripperLFingerLink;
		private Transform lGripperRFingerLink;
		private Transform lGripperRFingerTipLink;
		private Transform lGripperLFingerTipLink;

		private float torsoLiftLinkIniPosZ;

		private JointState jointState;

		private float elapsedTime;


		void Awake()
		{
			this.torsoLiftLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link      .ToString());
			this.headPanLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link        .ToString());
			this.headTiltLink        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link       .ToString());
			this.laserTiltMountLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.laser_tilt_mount_link.ToString());

			this.rUpperArmRollLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_upper_arm_roll_link      .ToString());
			this.rShoulderPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_shoulder_pan_link        .ToString());
			this.rShoulderLiftLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_shoulder_lift_link       .ToString());
			this.rForearmRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_forearm_roll_link        .ToString());
			this.rElbowFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_elbow_flex_link          .ToString());
			this.rWristFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_wrist_flex_link          .ToString());
			this.rWristRollLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_wrist_roll_link          .ToString());
			this.rGripperPalmLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_palm_link        .ToString());
			this.rGripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_link    .ToString());
			this.rGripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_link    .ToString());
			this.rGripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_tip_link.ToString());
			this.rGripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_tip_link.ToString());

			this.lUpperArmRollLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_upper_arm_roll_link      .ToString());
			this.lShoulderPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_shoulder_pan_link        .ToString());
			this.lShoulderLiftLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_shoulder_lift_link       .ToString());
			this.lForearmRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_forearm_roll_link        .ToString());
			this.lElbowFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_elbow_flex_link          .ToString());
			this.lWristFlexLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_wrist_flex_link          .ToString());
			this.lWristRollLink         = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_wrist_roll_link          .ToString());
			this.lGripperPalmLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_palm_link        .ToString());
			this.lGripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_link    .ToString());
			this.lGripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_link    .ToString());
			this.lGripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_tip_link.ToString());
			this.lGripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_tip_link.ToString());

			this.torsoLiftLinkIniPosZ = this.torsoLiftLink.localPosition.z;
		}

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "");

			this.jointState.name = new List<string>()
			{
				{ PR2Common.Joint.fl_caster_rotation_joint.ToString() },  // 01
				{ PR2Common.Joint.fl_caster_l_wheel_joint .ToString() },  // 02
				{ PR2Common.Joint.fl_caster_r_wheel_joint .ToString() },  // 03
				{ PR2Common.Joint.fr_caster_rotation_joint.ToString() },  // 04
				{ PR2Common.Joint.fr_caster_l_wheel_joint .ToString() },  // 05
				{ PR2Common.Joint.fr_caster_r_wheel_joint .ToString() },  // 06

				{ PR2Common.Joint.bl_caster_rotation_joint.ToString() },  // 07
				{ PR2Common.Joint.bl_caster_l_wheel_joint .ToString() },  // 08
				{ PR2Common.Joint.bl_caster_r_wheel_joint .ToString() },  // 09
				{ PR2Common.Joint.br_caster_rotation_joint.ToString() },  // 10
				{ PR2Common.Joint.br_caster_l_wheel_joint .ToString() },  // 11
				{ PR2Common.Joint.br_caster_r_wheel_joint .ToString() },  // 12

				{ PR2Common.Joint.torso_lift_joint            .ToString() }, // 13
				{ PR2Common.Joint.torso_lift_motor_screw_joint.ToString() }, // 14
				{ PR2Common.Joint.head_pan_joint              .ToString() }, // 15
				{ PR2Common.Joint.head_tilt_joint             .ToString() }, // 16
				{ PR2Common.Joint.laser_tilt_mount_joint      .ToString() }, // 17

				{ PR2Common.Joint.r_upper_arm_roll_joint      .ToString() }, // 18
				{ PR2Common.Joint.r_shoulder_pan_joint        .ToString() }, // 19
				{ PR2Common.Joint.r_shoulder_lift_joint       .ToString() }, // 20
				{ PR2Common.Joint.r_forearm_roll_joint        .ToString() }, // 21
				{ PR2Common.Joint.r_elbow_flex_joint          .ToString() }, // 22
				{ PR2Common.Joint.r_wrist_flex_joint          .ToString() }, // 23
				{ PR2Common.Joint.r_wrist_roll_joint          .ToString() }, // 24
				{ PR2Common.Joint.r_gripper_joint             .ToString() }, // 25
				{ PR2Common.Joint.r_gripper_l_finger_joint    .ToString() }, // 26
				{ PR2Common.Joint.r_gripper_r_finger_joint    .ToString() }, // 27
				{ PR2Common.Joint.r_gripper_r_finger_tip_joint.ToString() }, // 28
				{ PR2Common.Joint.r_gripper_l_finger_tip_joint.ToString() }, // 29
				{ PR2Common.Joint.r_gripper_motor_screw_joint .ToString() }, // 30
				{ PR2Common.Joint.r_gripper_motor_slider_joint.ToString() }, // 31

				{ PR2Common.Joint.l_upper_arm_roll_joint      .ToString() }, // 32
				{ PR2Common.Joint.l_shoulder_pan_joint        .ToString() }, // 33
				{ PR2Common.Joint.l_shoulder_lift_joint       .ToString() }, // 34
				{ PR2Common.Joint.l_forearm_roll_joint        .ToString() }, // 35
				{ PR2Common.Joint.l_elbow_flex_joint          .ToString() }, // 36
				{ PR2Common.Joint.l_wrist_flex_joint          .ToString() }, // 37
				{ PR2Common.Joint.l_wrist_roll_joint          .ToString() }, // 38
				{ PR2Common.Joint.l_gripper_joint             .ToString() }, // 39
				{ PR2Common.Joint.l_gripper_l_finger_joint    .ToString() }, // 40
				{ PR2Common.Joint.l_gripper_r_finger_joint    .ToString() }, // 41
				{ PR2Common.Joint.l_gripper_r_finger_tip_joint.ToString() }, // 42
				{ PR2Common.Joint.l_gripper_l_finger_tip_joint.ToString() }, // 43
				{ PR2Common.Joint.l_gripper_motor_screw_joint .ToString() }, // 44
				{ PR2Common.Joint.l_gripper_motor_slider_joint.ToString() }, // 45
			};

			this.jointState.position = null;
			this.jointState.velocity = new List<double>
			{
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
				0.0, 0.0, 0.0, 0.0, 0.0,
			};
			this.jointState.effort = new List<double>
			{
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
				0.0, 0.0, 0.0, 0.0, 0.0,
			};

			foreach(PR2Common.Joint continuousJoint in PR2Common.continuousJoints)
			{
				switch(continuousJoint)
				{
					case PR2Common.Joint.r_forearm_roll_joint: { this.continuousJointInfoList.Add(new ContinuousJointInfo(continuousJoint, 21)); break; }
					case PR2Common.Joint.r_wrist_roll_joint:   { this.continuousJointInfoList.Add(new ContinuousJointInfo(continuousJoint, 24)); break; }
					case PR2Common.Joint.l_forearm_roll_joint: { this.continuousJointInfoList.Add(new ContinuousJointInfo(continuousJoint, 35)); break; }
					case PR2Common.Joint.l_wrist_roll_joint:   { this.continuousJointInfoList.Add(new ContinuousJointInfo(continuousJoint, 38)); break; }
					default: { throw new Exception("Initialization of continuousJointInfoList failed."); }
				}
			}
		}


		protected override void Update()
		{
			base.Update();

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			List<double> positions = new List<double>()
			{
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, // 01--12

				this.torsoLiftLink.localPosition.z - this.torsoLiftLinkIniPosZ , // 13 
				0.0, // 14
				PR2Common.GetRosAngleRad(PR2Common.Joint.head_pan_joint,  Vector3.back, this.headPanLink) , // 15
				PR2Common.GetRosAngleRad(PR2Common.Joint.head_tilt_joint, Vector3.down, this.headTiltLink), // 16
				0.0, // 17

				PR2Common.GetRosAngleRad(PR2Common.Joint.r_upper_arm_roll_joint,       Vector3.right, this.rUpperArmRollLink)     , // 18
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_shoulder_pan_joint,         Vector3.back,  this.rShoulderPanLink)      , // 19
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_shoulder_lift_joint,        Vector3.down,  this.rShoulderLiftLink)     , // 20
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_forearm_roll_joint,         Vector3.right, this.rForearmRollLink)      , // 21
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_elbow_flex_joint,           Vector3.down,  this.rElbowFlexLink)        , // 22
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_wrist_flex_joint,           Vector3.down,  this.rWristFlexLink)        , // 23
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_wrist_roll_joint,           Vector3.right, this.rWristRollLink)        , // 24
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_gripper_joint,              Vector3.down,  this.rGripperPalmLink)      , // 25
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_gripper_l_finger_joint,     Vector3.back,  this.rGripperLFingerLink)   , // 26
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_gripper_r_finger_joint,     Vector3.back,  this.rGripperRFingerLink)   , // 27
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_gripper_r_finger_tip_joint, Vector3.back,  this.rGripperRFingerTipLink), // 28
				PR2Common.GetRosAngleRad(PR2Common.Joint.r_gripper_l_finger_tip_joint, Vector3.back,  this.rGripperLFingerTipLink), // 29
				0.0, // 30
				0.0, // 31

				PR2Common.GetRosAngleRad(PR2Common.Joint.l_upper_arm_roll_joint,       Vector3.right, this.lUpperArmRollLink)     , // 32
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_shoulder_pan_joint,         Vector3.back,  this.lShoulderPanLink)      , // 33
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_shoulder_lift_joint,        Vector3.down,  this.lShoulderLiftLink)     , // 34
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_forearm_roll_joint,         Vector3.right, this.lForearmRollLink)      , // 35
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_elbow_flex_joint,           Vector3.down,  this.lElbowFlexLink)        , // 36
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_wrist_flex_joint,           Vector3.down,  this.lWristFlexLink)        , // 37
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_wrist_roll_joint,           Vector3.right, this.lWristRollLink)        , // 38
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_gripper_joint,              Vector3.down,  this.lGripperPalmLink)      , // 39
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_gripper_l_finger_joint,     Vector3.back,  this.lGripperLFingerLink)   , // 40
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_gripper_r_finger_joint,     Vector3.back,  this.lGripperRFingerLink)   , // 41
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_gripper_r_finger_tip_joint, Vector3.back,  this.lGripperRFingerTipLink), // 42
				PR2Common.GetRosAngleRad(PR2Common.Joint.l_gripper_l_finger_tip_joint, Vector3.back,  this.lGripperLFingerTipLink), // 43
				0.0, // 44
				0.0, // 45
			};

			this.UpdateContinuousJointVal(ref positions);

			this.jointState.header.Update();
			this.jointState.position = positions;

			this.publisher.Publish(this.jointState);
		}


		private void UpdateContinuousJointVal(ref List<double> positions)
		{
			foreach(ContinuousJointInfo continuousJoint in this.continuousJointInfoList)
			{
				double position = positions[continuousJoint.SeqNo-1];

				if(continuousJoint.PreviousPos > +Mathf.PI/2 && position < -Mathf.PI/2 ){ continuousJoint.RotationCount++; }
				if(continuousJoint.PreviousPos < -Mathf.PI/2 && position > +Mathf.PI/2 ){ continuousJoint.RotationCount--; }

				continuousJoint.PreviousPos = position;

				positions[continuousJoint.SeqNo-1] += continuousJoint.RotationCount * 2* Mathf.PI;
			}
		}
	}
}

