using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using System;

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
			this.armLiftLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmLiftLinkName );
			this.armFlexLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmFlexLinkName );
			this.armRollLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.ArmRollLinkName );
			this.wristFlexLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.WristFlexLinkName );
			this.wristRollLink     = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.WristRollLinkName );
			this.headPanLink       = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HeadPanLinkName );
			this.headTiltLink      = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HeadTiltLinkName );
			this.handLProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HandLProximalLinkName );
			this.handRProximalLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.HandRProximalLinkName );

			this.armLiftLinkIniPosZ = this.armLiftLink.localPosition.z;
		}

		protected override void Start()
		{
			base.Start();

			this.jointState = new JointState();
			this.jointState.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "hsrb_joint_states");

			this.jointState.name = new List<string>();
			this.jointState.name.Add(HSRCommon.ArmLiftJointName);            // 1
			this.jointState.name.Add(HSRCommon.ArmFlexJointName);            // 2
			this.jointState.name.Add(HSRCommon.ArmRollJointName);            // 3
			this.jointState.name.Add(HSRCommon.WristFlexJointName);          // 4
			this.jointState.name.Add(HSRCommon.WristRollJointName);          // 5
			this.jointState.name.Add(HSRCommon.HeadPanJointName);            // 6
			this.jointState.name.Add(HSRCommon.HeadTiltJointName);           // 7
			this.jointState.name.Add(HSRCommon.HandLSpringProximalJointName); // 8 Convert name from HandLProximalJointName to HandLSpringProximalJointName
			this.jointState.name.Add(HSRCommon.HandRSpringProximalJointName); // 9 Convert name from HandRProximalJointName to HandRSpringProximalJointName

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

			List<double> positions = new List<double>();

			//1 ArmLiftJoint
			positions.Add(this.armLiftLink.localPosition.z - this.armLiftLinkIniPosZ);
			//2 ArmFlexJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.armFlexLink.localEulerAngles.y, HSRCommon.ArmFlexJointName) * Mathf.Deg2Rad);
			//3 ArmRollJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.armRollLink.localEulerAngles.z, HSRCommon.ArmRollJointName) * Mathf.Deg2Rad);
			//4 WristFlexJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.wristFlexLink.localEulerAngles.y, HSRCommon.WristFlexJointName) * Mathf.Deg2Rad);
			//5 WristRollJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.wristRollLink.localEulerAngles.z, HSRCommon.WristRollJointName) * Mathf.Deg2Rad);
			//6 HeadPanJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(-this.headPanLink.localEulerAngles.z, HSRCommon.HeadPanJointName) * Mathf.Deg2Rad);
			//7 HeadTiltJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.headTiltLink.localEulerAngles.y, HSRCommon.HeadTiltJointName) * Mathf.Deg2Rad);
			//8 HandLSpringProximalJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.handLProximalLink.localEulerAngles.x, HSRCommon.HandLProximalJointName) * Mathf.Deg2Rad);
			//9 HandRSpringProximalJoint
			positions.Add(HSRCommon.GetCorrectedJointsEulerAngle(this.handRProximalLink.localEulerAngles.x, HSRCommon.HandRProximalJointName) * Mathf.Deg2Rad);

			this.jointState.header.Update();
			this.jointState.position = positions;

//			float position = HSRCommon.GetClampedPosition(value, name);

			this.publisher.Publish(this.jointState);
		}
	}
}

