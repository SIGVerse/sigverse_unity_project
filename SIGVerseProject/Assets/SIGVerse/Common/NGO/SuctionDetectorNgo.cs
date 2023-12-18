using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.Common
{
	public class SuctionDetectorNgo : SuctionDetector
	{
		private OwnerChanger ownerChanger;

		protected override void Suck(Rigidbody collidedRigidbody)
		{
			this.ownerChanger = collidedRigidbody.GetComponent<OwnerChanger>();

			if (this.ownerChanger == null) { SIGVerseLogger.Error("this.ownerChanger == null in Suck(). name="+collidedRigidbody.gameObject.name); return; }

			this.ownerChanger.GetOwnershipServerRpc();

			//this.savedParentObj = collidedRigidbody.gameObject.transform.parent;

			//collidedRigidbody.gameObject.transform.parent = this.vacuum;

			collidedRigidbody.useGravity  = false;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			GraspedObjectFixerWithHolder graspedObjectFixer = collidedRigidbody.gameObject.AddComponent<GraspedObjectFixerWithHolder>();
			graspedObjectFixer.SetHolder(this.vacuum);
			graspedObjectFixer.canChangeGraspPoint = true;

			this.suckedRigidbody = collidedRigidbody;

			this.initSuckedRelPos = this.vacuum.InverseTransformPoint(this.suckedRigidbody.position);
			this.initSuckedRelRot = Quaternion.Inverse(this.vacuum.rotation) * this.suckedRigidbody.rotation;
			//this.preSuckedRelPos = this.vacuum.position - this.suckedRigidbody.transform.position;
			//this.preSuckedRelRot = this.vacuum.rotation * Quaternion.Inverse(this.suckedRigidbody.transform.rotation);

			SIGVerseLogger.Info("Suction: Sucked: " + this.suckedRigidbody.name);
		}

		protected override void Release()
		{
			if (this.ownerChanger == null) { SIGVerseLogger.Error("this.ownerChanger == null in Release()"); return; }

			// If immediately return ownership, the behavior of the object after user throw the object becomes server-driven.
			// this.ownerChanger.RemoveOwnershipServerRpc();

			//this.suckedRigidbody.transform.parent = this.savedParentObj;

			this.suckedRigidbody.useGravity  = true;
			this.suckedRigidbody.constraints = RigidbodyConstraints.None;

			GraspedObjectFixer graspedObjectFixer = this.suckedRigidbody.gameObject.GetComponent<GraspedObjectFixer>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.suckedRigidbody = null;
			//this.savedParentObj = null;

			SIGVerseLogger.Info("Suction: Released the object");
		}
	}
}

