using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs.msg;
using SIGVerse.RosBridge.sensor_msgs.msg;
using SIGVerse.Common;
using System.Collections.Generic;
using Joint = SIGVerse.ToyotaHSR.HSRCommon.Joint;
using Link  = SIGVerse.ToyotaHSR.HSRCommon.Link;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubJointState : RosPubMessage<JointState>
	{
		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		private Dictionary<Joint, Transform> linkMap = new Dictionary<Joint, Transform>();

		private float armLiftLinkIniPosZ;
		private float torsoLiftLinkIniPosZ;

		private JointState jointState;

		private float elapsedTime;


		void Awake()
		{
			this.linkMap[Joint.arm_lift_joint]               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_lift_link              .ToString());
			this.linkMap[Joint.arm_flex_joint]               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_flex_link              .ToString());
			this.linkMap[Joint.arm_roll_joint]               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_roll_link              .ToString());
			this.linkMap[Joint.wrist_flex_joint]             = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_flex_link            .ToString());
			this.linkMap[Joint.wrist_roll_joint]             = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_roll_link            .ToString());
			this.linkMap[Joint.head_pan_joint]               = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_pan_link              .ToString());
			this.linkMap[Joint.head_tilt_joint]              = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.head_tilt_link             .ToString());
			this.linkMap[Joint.torso_lift_joint]             = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link            .ToString());
			this.linkMap[Joint.hand_motor_joint]             = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_motor_dummy_link      .ToString());
			this.linkMap[Joint.hand_l_proximal_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_proximal_link       .ToString());
			this.linkMap[Joint.hand_l_spring_proximal_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_spring_proximal_link.ToString());
			this.linkMap[Joint.hand_l_mimic_distal_joint]    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_mimic_distal_link   .ToString());
			this.linkMap[Joint.hand_l_distal_joint]          = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_l_distal_link         .ToString());
			this.linkMap[Joint.hand_r_proximal_joint]        = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_proximal_link       .ToString());
			this.linkMap[Joint.hand_r_spring_proximal_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_spring_proximal_link.ToString());
			this.linkMap[Joint.hand_r_mimic_distal_joint]    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_mimic_distal_link   .ToString());
			this.linkMap[Joint.hand_r_distal_joint]          = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.hand_r_distal_link         .ToString());

			this.armLiftLinkIniPosZ   = this.linkMap[Joint.arm_lift_joint]  .localPosition.z;
			this.torsoLiftLinkIniPosZ = this.linkMap[Joint.torso_lift_joint].localPosition.z;
		}

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(new SIGVerse.RosBridge.builtin_interfaces.msg.Time(0, 0), "hsrb_joint_states");

			this.jointState.name = new List<string>()
			{
				{ Joint.arm_lift_joint              .ToString() }, // 1
				{ Joint.arm_flex_joint              .ToString() }, // 2
				{ Joint.arm_roll_joint              .ToString() }, // 3
				{ Joint.wrist_flex_joint            .ToString() }, // 4
				{ Joint.wrist_roll_joint            .ToString() }, // 5
				{ Joint.head_pan_joint              .ToString() }, // 6
				{ Joint.head_tilt_joint             .ToString() }, // 7
				{ Joint.torso_lift_joint            .ToString() }, // 8
				{ Joint.hand_motor_joint            .ToString() }, // 9
				{ Joint.hand_l_proximal_joint       .ToString() }, // 10
				{ Joint.hand_l_spring_proximal_joint.ToString() }, // 11
				{ Joint.hand_l_mimic_distal_joint   .ToString() }, // 12
				{ Joint.hand_l_distal_joint         .ToString() }, // 13
				{ Joint.hand_r_proximal_joint       .ToString() }, // 14
				{ Joint.hand_r_spring_proximal_joint.ToString() }, // 15
				{ Joint.hand_r_mimic_distal_joint   .ToString() }, // 16
				{ Joint.hand_r_distal_joint         .ToString() }, // 17
			};

			this.jointState.position = null;
			this.jointState.velocity = new List<double>();
			this.jointState.effort   = new List<double>();

			for(int i=0; i<this.jointState.name.Count; i++)
			{
				this.jointState.velocity.Add(0.0);
				this.jointState.effort  .Add(0.0);
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
				{ this.linkMap[Joint.arm_lift_joint].localPosition.z - this.armLiftLinkIniPosZ },     // 1 arm_lift_joint
				{ GetNormalizedJointRosAngle(Joint.arm_flex_joint) },                   // 2 
				{ GetNormalizedJointRosAngle(Joint.arm_roll_joint) },                   // 3 
				{ GetNormalizedJointRosAngle(Joint.wrist_flex_joint) },                 // 4 
				{ GetNormalizedJointRosAngle(Joint.wrist_roll_joint) },                 // 5 
				{ GetNormalizedJointRosAngle(Joint.head_pan_joint) },                   // 6 
				{ GetNormalizedJointRosAngle(Joint.head_tilt_joint) },                  // 7 
				{ this.linkMap[Joint.torso_lift_joint].localPosition.z - this.torsoLiftLinkIniPosZ }, // 8 torso_lift_joint
				{ GetNormalizedJointRosAngle(Joint.hand_motor_joint) },                 // 9 
				{ GetNormalizedJointRosAngle(Joint.hand_l_proximal_joint) },            // 10
				{ GetNormalizedJointRosAngle(Joint.hand_l_spring_proximal_joint) },     // 11
				{ GetNormalizedJointRosAngle(Joint.hand_l_mimic_distal_joint) },        // 12
				{ GetNormalizedJointRosAngle(Joint.hand_l_distal_joint) },              // 13
				{ GetNormalizedJointRosAngle(Joint.hand_r_proximal_joint) },            // 14
				{ GetNormalizedJointRosAngle(Joint.hand_r_spring_proximal_joint) },     // 15
				{ GetNormalizedJointRosAngle(Joint.hand_r_mimic_distal_joint) },        // 16
				{ GetNormalizedJointRosAngle(Joint.hand_r_distal_joint) },              // 17
			};

			this.jointState.header.Update();
			this.jointState.position = positions;

			this.publisher.Publish(this.jointState);
		}

		private double GetNormalizedJointRosAngle(Joint joint)
		{
			Transform link = this.linkMap[joint];

			return HSRCommon.GetNormalizedJointRosAngle(link, joint);
		}
	}
}

