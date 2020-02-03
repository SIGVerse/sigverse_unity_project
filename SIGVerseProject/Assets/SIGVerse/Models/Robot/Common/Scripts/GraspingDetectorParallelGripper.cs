using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.Common
{
	public class GraspingDetectorParallelGripper : GraspingDetector
	{
		protected const float OpeningDistanceThreshold = 0.0f;

		protected float leftGripperPos;
		protected float rightGripperPos;

		protected float preLeftGripperPos;
		protected float preRightGripperPos;

		protected float openingDistance;
		
		// Use this for initialization
		protected override void Start()
		{
			this.UpdateGripperPosition();

			this.preLeftGripperPos  = this.leftGripperPos;
			this.preRightGripperPos = this.rightGripperPos;

			this.graspedRigidbody = null;
			this.isGripperClosing = false;

			this.openingDistance = 0.0f;
		}

		// Update is called once per frame
		protected override void FixedUpdate()
		{
			this.UpdateGripperPosition();

			// Check hand closing
			if(this.leftGripperPos < this.preLeftGripperPos && this.rightGripperPos < this.preRightGripperPos)
			{
				this.isGripperClosing = true;
			}
			else
			{
				this.isGripperClosing = false;
			}

			if(this.leftGripperPos > this.preLeftGripperPos && this.rightGripperPos > this.preRightGripperPos)
			{
				this.openingDistance += (this.leftGripperPos - this.preLeftGripperPos) + (this.rightGripperPos - this.preRightGripperPos);
			}
			else
			{
				this.openingDistance = 0.0f;
			}

			if(this.openingDistance > OpeningDistanceThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preLeftGripperPos  = this.leftGripperPos;
			this.preRightGripperPos = this.rightGripperPos;
		}

		protected virtual void UpdateGripperPosition()
		{
			this.leftGripperPos  = + this.leftGripper .transform.localPosition.x;
			this.rightGripperPos = - this.rightGripper.transform.localPosition.x;
		}
	}
}

