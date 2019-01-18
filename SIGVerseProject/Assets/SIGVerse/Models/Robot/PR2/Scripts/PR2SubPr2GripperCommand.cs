using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using System;
using static SIGVerse.PR2.PR2Common;
using SIGVerse.RosBridge.pr2_controllers_msgs;

namespace SIGVerse.PR2
{
	public class PR2SubPr2GripperCommand : RosSubMessage<SIGVerse.RosBridge.pr2_controllers_msgs.Pr2GripperCommand>, IPr2GraspedObjectHandler
	{
		public HandType handType = HandType.Left;

		//--------------------------------------------------

		private Transform gripperLFingerLink;
		private Transform gripperRFingerLink;
		private Transform gripperLFingerTipLink;
		private Transform gripperRFingerTipLink;

		private Pr2GripperCommand gripperCommand = null;

		private float gripperCurrentPos = 0.0f;

		private GameObject graspedObject = null;


		void Awake()
		{
			if(this.handType==HandType.Left)
			{
				this.gripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_link    .ToString());
				this.gripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_link    .ToString());
				this.gripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_l_finger_tip_link.ToString());
				this.gripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.l_gripper_r_finger_tip_link.ToString());
			}
			else
			{
				this.gripperLFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_link    .ToString());
				this.gripperRFingerLink    = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_link    .ToString());
				this.gripperLFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_l_finger_tip_link.ToString());
				this.gripperRFingerTipLink = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.r_gripper_r_finger_tip_link.ToString());
			}
		}

		//protected override void Start()
		//{
		//	base.Start();
		//}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.pr2_controllers_msgs.Pr2GripperCommand gripperCommand)
		{
			this.gripperCommand = gripperCommand;
		}


		protected void FixedUpdate()
		{
			if (this.gripperCommand == null){ return; }

			float newPos = this.GetGripperNewPosition();

			if(Mathf.Abs(newPos - this.gripperCurrentPos) < 0.0001f) // Movement is small
			{
				this.gripperCommand = null;
				return;
			}

			// Grasping and hand closing
			if (this.graspedObject!=null && newPos < this.gripperCurrentPos)
			{
				// Have to stop
				this.gripperCommand = null;
				return;
			}
			// Otherwise
			else
			{
				float angleDiff = (newPos - this.gripperCurrentPos) * 0.52f/0.086f * Mathf.Rad2Deg; // 0.52[rad](30[deg]) is max angle of a gripper joint and 0.086[m] is gripper distance.

				this.gripperLFingerLink   .localRotation *= Quaternion.Euler(0, 0, -angleDiff);
				this.gripperRFingerLink   .localRotation *= Quaternion.Euler(0, 0, -angleDiff);
				this.gripperLFingerTipLink.localRotation *= Quaternion.Euler(0, 0, +angleDiff);
				this.gripperRFingerTipLink.localRotation *= Quaternion.Euler(0, 0, +angleDiff);
			}

			this.gripperCurrentPos = newPos;
		}

		private float GetGripperNewPosition()
		{
			PR2Common.Joint joint = (this.handType==HandType.Left)? PR2Common.Joint.l_gripper_joint : PR2Common.Joint.r_gripper_joint;

			float maxDistance = 0.05f * Time.fixedDeltaTime;  // speed=0.05[m/s]

			float newPos = Mathf.Clamp((float)this.gripperCommand.position, this.gripperCurrentPos-maxDistance, this.gripperCurrentPos+maxDistance);

			return PR2Common.GetClampedPosition(newPos, joint);
		}

		public void OnChangeGraspedObject(HandType handType, GameObject graspedObject)
		{
			if(this.handType==handType)
			{
				this.graspedObject = graspedObject;
			}
		}
	}
}

