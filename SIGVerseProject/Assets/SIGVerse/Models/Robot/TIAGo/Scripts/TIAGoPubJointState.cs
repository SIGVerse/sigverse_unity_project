using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;
using System;
using static SIGVerse.TIAGo.TIAGoCommon;

namespace SIGVerse.TIAGo
{
	public class TIAGoPubJointState : RosPubMessage<JointState>
	{
		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private Transform torsoLiftLink;
		private Transform head1Link;
		private Transform head2Link;
		private Transform arm1Link;
		private Transform arm2Link;
		private Transform arm3Link;
		private Transform arm4Link;
		private Transform arm5Link;
		private Transform arm6Link;
		private Transform arm7Link;
		private Transform gripperLeftFingerLink;
		private Transform gripperRightFingerLink;

		private float torsoLiftLinkIniPosZ;
		private float arm1LinkIniRotZ;
		private float arm3LinkIniRotZ;
		private float arm4LinkIniRotY;
		private float arm6LinkIniRotY;
		private float arm7LinkIniRotZ;
		private float gripperLeftFingerLinkIniPosX;
		private float gripperRightFingerLinkIniPosX;

		private JointState jointState;

		private float elapsedTime;


		void Awake()
		{
			this.torsoLiftLink          = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link          .ToString());
			this.head1Link              = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_1_link              .ToString());
			this.head2Link              = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_2_link              .ToString());
			this.arm1Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_1_link               .ToString());
			this.arm2Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_2_link               .ToString());
			this.arm3Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_3_link               .ToString());
			this.arm4Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_4_link               .ToString());
			this.arm5Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_5_link               .ToString());
			this.arm6Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_6_link               .ToString());
			this.arm7Link               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_7_link               .ToString());
			this.gripperLeftFingerLink  = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.gripper_left_finger_link .ToString());
			this.gripperRightFingerLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.gripper_right_finger_link.ToString());

			this.torsoLiftLinkIniPosZ = this.torsoLiftLink.localPosition.z;
			this.arm1LinkIniRotZ = this.arm1Link.localEulerAngles.z;
			this.arm3LinkIniRotZ = this.arm3Link.localEulerAngles.z;
			this.arm4LinkIniRotY = this.arm4Link.localEulerAngles.y;
			this.arm6LinkIniRotY = this.arm6Link.localEulerAngles.y;
			this.arm7LinkIniRotZ = this.arm7Link.localEulerAngles.z;
			this.gripperLeftFingerLinkIniPosX  = this.gripperLeftFingerLink .localPosition.x;
			this.gripperRightFingerLinkIniPosX = this.gripperRightFingerLink.localPosition.x;

			Debug.Log("this.arm4LinkIniRotZ="+this.arm4Link.localEulerAngles.y+", "+this.arm4Link.localEulerAngles.z);
		}

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "");

			this.jointState.name = new List<string>()
			{
				{ TIAGoCommon.Joint.arm_1_joint               .ToString() }, //  1
				{ TIAGoCommon.Joint.arm_2_joint               .ToString() }, //  2
				{ TIAGoCommon.Joint.arm_3_joint               .ToString() }, //  3
				{ TIAGoCommon.Joint.arm_4_joint               .ToString() }, //  4
				{ TIAGoCommon.Joint.arm_5_joint               .ToString() }, //  5
				{ TIAGoCommon.Joint.arm_6_joint               .ToString() }, //  6
				{ TIAGoCommon.Joint.arm_7_joint               .ToString() }, //  7
				{ TIAGoCommon.Joint.gripper_left_finger_joint .ToString() }, //  8
				{ TIAGoCommon.Joint.gripper_right_finger_joint.ToString() }, //  9
				{ TIAGoCommon.Joint.head_1_joint              .ToString() }, // 10
				{ TIAGoCommon.Joint.head_2_joint              .ToString() }, // 11
				{ TIAGoCommon.Joint.torso_lift_joint          .ToString() }, // 12
				{ TIAGoCommon.Joint.wheel_left_joint          .ToString() }, // 13
				{ TIAGoCommon.Joint.wheel_right_joint         .ToString() }, // 14
				{ TIAGoCommon.Joint.caster_back_left_1_joint  .ToString() }, // 15
				{ TIAGoCommon.Joint.caster_back_left_2_joint  .ToString() }, // 16
				{ TIAGoCommon.Joint.caster_front_left_1_joint .ToString() }, // 17
				{ TIAGoCommon.Joint.caster_front_left_2_joint .ToString() }, // 18
				{ TIAGoCommon.Joint.caster_back_right_1_joint .ToString() }, // 19
				{ TIAGoCommon.Joint.caster_back_right_2_joint .ToString() }, // 20
				{ TIAGoCommon.Joint.caster_front_right_1_joint.ToString() }, // 21
				{ TIAGoCommon.Joint.caster_front_right_2_joint.ToString() }, // 22
				{ TIAGoCommon.Joint.suspension_left_joint     .ToString() }, // 23
				{ TIAGoCommon.Joint.suspension_right_joint    .ToString() }, // 24
			};

			this.jointState.position = null;
			this.jointState.velocity = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
			this.jointState.effort   = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
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
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm1Link .localEulerAngles.z - this.arm1LinkIniRotZ), TIAGoCommon.Joint.arm_1_joint) * Mathf.Deg2Rad, //  1
				TIAGoCommon.GetNormalizedJointEulerAngle(+(this.arm2Link .localEulerAngles.y                       ), TIAGoCommon.Joint.arm_2_joint) * Mathf.Deg2Rad, //  2
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm3Link .localEulerAngles.z - this.arm3LinkIniRotZ), TIAGoCommon.Joint.arm_3_joint) * Mathf.Deg2Rad, //  3
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm4Link .localEulerAngles.y - this.arm4LinkIniRotY), TIAGoCommon.Joint.arm_4_joint) * Mathf.Deg2Rad, //  4
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm5Link .localEulerAngles.z                       ), TIAGoCommon.Joint.arm_5_joint) * Mathf.Deg2Rad, //  5
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm6Link .localEulerAngles.y - this.arm6LinkIniRotY), TIAGoCommon.Joint.arm_6_joint) * Mathf.Deg2Rad, //  6
				TIAGoCommon.GetNormalizedJointEulerAngle(-(this.arm7Link .localEulerAngles.z - this.arm7LinkIniRotZ), TIAGoCommon.Joint.arm_7_joint) * Mathf.Deg2Rad, //  7
				+(this.gripperLeftFingerLink .localPosition.x - this.gripperLeftFingerLinkIniPosX),  //  8
				-(this.gripperRightFingerLink.localPosition.x - this.gripperRightFingerLinkIniPosX), //  9
				TIAGoCommon.GetNormalizedJointEulerAngle(-this.head1Link.localEulerAngles.z, TIAGoCommon.Joint.head_1_joint) * Mathf.Deg2Rad, // 10
				TIAGoCommon.GetNormalizedJointEulerAngle(+this.head2Link.localEulerAngles.y, TIAGoCommon.Joint.head_2_joint) * Mathf.Deg2Rad, // 11
				+(this.torsoLiftLink.localPosition.z - this.torsoLiftLinkIniPosZ),  // 12
				0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0  // 13 -- 24
			};

			this.jointState.header.Update();
			this.jointState.position = positions;

			this.publisher.Publish(this.jointState);
		}
	}
}
