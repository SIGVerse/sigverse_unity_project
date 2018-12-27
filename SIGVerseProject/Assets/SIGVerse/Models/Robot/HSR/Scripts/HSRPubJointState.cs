using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;
using static SIGVerse.ToyotaHSR.HSRCommon;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubJointState : RosPubMessage<JointState>
	{
		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------
		private Transform armLiftLink;
		private Transform armFlexLink;
		private Transform armRollLink;
		private Transform wristFlexLink;
		private Transform wristRollLink;
		private Transform headPanLink;
		private Transform headTiltLink;
		private Transform handLProximalLink;
		private Transform handRProximalLink;

		private float armLiftLinkIniPosZ;

		private JointState jointState;

		private float elapsedTime;


		void Awake()
		{
			this.armLiftLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_lift_link       .ToString());
			this.armFlexLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_flex_link       .ToString());
			this.armRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_roll_link       .ToString());
			this.wristFlexLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_flex_link     .ToString());
			this.wristRollLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_roll_link     .ToString());
			this.headPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link       .ToString());
			this.headTiltLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link      .ToString());
			this.handLProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_proximal_link.ToString());
			this.handRProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_proximal_link.ToString());

			this.armLiftLinkIniPosZ = this.armLiftLink.localPosition.z;
		}

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "hsrb_joint_states");

			this.jointState.name = new List<string>()
			{
				{ HSRCommon.Joint.arm_lift_joint  .ToString() },  // 1
				{ HSRCommon.Joint.arm_flex_joint  .ToString() },  // 2
				{ HSRCommon.Joint.arm_roll_joint  .ToString() },  // 3
				{ HSRCommon.Joint.wrist_flex_joint.ToString() },  // 4
				{ HSRCommon.Joint.wrist_roll_joint.ToString() },  // 5
				{ HSRCommon.Joint.head_pan_joint  .ToString() },  // 6
				{ HSRCommon.Joint.head_tilt_joint .ToString() },  // 7
				{ HSRCommon.Joint.hand_l_spring_proximal_joint.ToString() }, // 8 Convert name from hand_l_proximal_joint to hand_l_spring_proximal_joint
				{ HSRCommon.Joint.hand_r_spring_proximal_joint.ToString() }, // 9 Convert name from hand_r_proximal_joint to hand_r_spring_proximal_joint
			};

			this.jointState.position = null;
			this.jointState.velocity = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
			this.jointState.effort   = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
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
				{ this.armLiftLink.localPosition.z - this.armLiftLinkIniPosZ }, //1 arm_lift_joint
				{ HSRCommon.GetNormalizedJointEulerAngle(+this.armFlexLink      .localEulerAngles.y, HSRCommon.Joint.arm_flex_joint)        * Mathf.Deg2Rad}, // 2 
				{ HSRCommon.GetNormalizedJointEulerAngle(-this.armRollLink      .localEulerAngles.z, HSRCommon.Joint.arm_roll_joint)        * Mathf.Deg2Rad}, // 3 
				{ HSRCommon.GetNormalizedJointEulerAngle(+this.wristFlexLink    .localEulerAngles.y, HSRCommon.Joint.wrist_flex_joint)      * Mathf.Deg2Rad}, // 4 
				{ HSRCommon.GetNormalizedJointEulerAngle(-this.wristRollLink    .localEulerAngles.z, HSRCommon.Joint.wrist_roll_joint)      * Mathf.Deg2Rad}, // 5 
				{ HSRCommon.GetNormalizedJointEulerAngle(-this.headPanLink      .localEulerAngles.z, HSRCommon.Joint.head_pan_joint)        * Mathf.Deg2Rad}, // 6 
				{ HSRCommon.GetNormalizedJointEulerAngle(+this.headTiltLink     .localEulerAngles.y, HSRCommon.Joint.head_tilt_joint)       * Mathf.Deg2Rad}, // 7 
				{ HSRCommon.GetNormalizedJointEulerAngle(+this.handLProximalLink.localEulerAngles.x, HSRCommon.Joint.hand_l_proximal_joint) * Mathf.Deg2Rad}, // 8 hand_l_spring_proximal_joint
				{ HSRCommon.GetNormalizedJointEulerAngle(+this.handRProximalLink.localEulerAngles.x, HSRCommon.Joint.hand_r_proximal_joint) * Mathf.Deg2Rad}, // 9 hand_r_spring_proximal_joint
			};

			this.jointState.header.Update();
			this.jointState.position = positions;

			this.publisher.Publish(this.jointState);
		}
	}
}

