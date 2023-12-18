using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.Common
{
	public class GraspingDetectorNgo : GraspingDetector
	{
		private OwnerChanger ownerChanger;

		protected override void Grasp(Rigidbody collidedRigidbody)
		{
			this.ownerChanger = collidedRigidbody.GetComponent<OwnerChanger>();

			if (this.ownerChanger == null) { SIGVerseLogger.Error("this.ownerChanger == null in Grasp(). name="+collidedRigidbody.gameObject.name); return; }

			this.ownerChanger.GetOwnershipServerRpc();

			collidedRigidbody.useGravity  = false;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			GraspedObjectFixerWithHolder graspedObjectFixer = collidedRigidbody.gameObject.AddComponent<GraspedObjectFixerWithHolder>();
			graspedObjectFixer.SetHolder(this.handPalm.transform);
			graspedObjectFixer.canChangeGraspPoint = true;

			this.graspedRigidbody = collidedRigidbody;

			this.SendGraspedObjectInfo(this.graspedRigidbody.gameObject);

			SIGVerseLogger.Info("Grasped: "+this.graspedRigidbody.gameObject.name);

			this.latestReleaseTime = 0.0f;
		}

		protected override void Release()
		{
			if (this.ownerChanger == null) { SIGVerseLogger.Error("this.ownerChanger == null in Release()"); return; }

			// If immediately return ownership, the behavior of the object after user throw the object becomes server-driven.
			// this.ownerChanger.RemoveOwnershipServerRpc();

			this.graspedRigidbody.useGravity  = true;
			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

			GraspedObjectFixerWithHolder graspedObjectFixer = this.graspedRigidbody.gameObject.GetComponent<GraspedObjectFixerWithHolder>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.graspedRigidbody = null;

			this.SendGraspedObjectInfo(null);

			SIGVerseLogger.Info("Released the object");

			this.latestReleaseTime = Time.time;
		}
	}
}

