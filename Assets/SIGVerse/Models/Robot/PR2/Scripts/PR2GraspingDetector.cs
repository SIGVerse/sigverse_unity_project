using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.PR2
{
	public interface IPr2GraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(HandType handType, GameObject graspedObject);
	}

	public class PR2GraspingDetector : GraspingDetector, IPr2GripperTriggerHandler
	{
		public HandType handType = HandType.Left;

		// Use this for initialization
		protected override void Start()
		{
			base.Start();

			this.leftGripperAngle  = this.leftGripper .transform.localEulerAngles.z;
			this.rightGripperAngle = this.rightGripper.transform.localEulerAngles.z;

			this.preLeftGripperAngle  = this.leftGripperAngle;
			this.preRightGripperAngle = this.rightGripperAngle;
		}

		// Update is called once per frame
		protected override void FixedUpdate()
		{
			this.leftGripperAngle  = this.leftGripper .transform.localEulerAngles.z;
			this.rightGripperAngle = this.rightGripper.transform.localEulerAngles.z;

			// Check hand closing
			if(this.leftGripperAngle > this.preLeftGripperAngle && this.rightGripperAngle > this.preRightGripperAngle)
			{
				this.isGripperClosing = true;
			}
			else
			{
				this.isGripperClosing = false;
			}

			// Calc opening angle
			if(this.leftGripperAngle < this.preLeftGripperAngle && this.rightGripperAngle < this.preRightGripperAngle)
			{
				this.openingAngle += (this.preLeftGripperAngle - this.leftGripperAngle) + (this.preRightGripperAngle - this.rightGripperAngle);
			}
			else
			{
				this.openingAngle = 0.0f;
			}

			if(this.openingAngle > OpeningAngleThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preLeftGripperAngle  = this.leftGripperAngle;
			this.preRightGripperAngle = this.rightGripperAngle;
		}

		public void OnTransferredTriggerEnter(HandType handType, GripperType gripperType, Rigidbody targetRigidbody)
		{
			if(this.handType!=handType){ return; }

			this.OnTransferredTriggerEnter(targetRigidbody, gripperType);
		}

		public void OnTransferredTriggerExit(HandType handType, GripperType gripperType, Rigidbody targetRigidbody)
		{
			if(this.handType!=handType){ return; }

			this.OnTransferredTriggerExit(targetRigidbody, gripperType);
		}

		protected override void SendGraspedObjectInfo(GameObject graspedObject)
		{
			foreach(GameObject graspingNotificationDestination in graspingNotificationDestinations)
			{
				ExecuteEvents.Execute<IPr2GraspedObjectHandler>
				(
					target: graspingNotificationDestination, 
					eventData: null, 
					functor: (reciever, eventData) => reciever.OnChangeGraspedObject(this.handType, graspedObject)
				);
			}
		}
	}
}

