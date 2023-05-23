using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class SuctionDetectorRPC : MonoBehaviour
	{
		public GameObject vacuum;

		private GameObject suckedObject;
		private Transform  suckedParentObject;

#if SIGVERSE_PUN

		[PunRPC]
		protected void SuctionSuckRPC(string objectName)
		{
			this.suckedObject = RoomObjectManager.Instance.GetRoomObject(objectName);

			// Save the parent object
			this.suckedParentObject = this.suckedObject.transform.parent;

			// Change parent object to the robot hand
			this.suckedObject.transform.parent = this.vacuum.transform;

			Rigidbody graspedRigidbody = this.suckedObject.GetComponent<Rigidbody>();

			graspedRigidbody.useGravity = false;
//			graspedRigidbody.isKinematic = true;
			graspedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			this.suckedObject.GetComponent<ThrowableWithoutSuction>().ChangeOwner();

			SIGVerseLogger.Info("Suck by SuctionDetectorRPC :" + objectName);
		}

		[PunRPC]
		protected void SuctionReleaseRPC()
		{
			if(this.suckedObject == null)
			{
				SIGVerseLogger.Warn("SuckedObject is null");
				return;
			}

			// Restore the parent object
			this.suckedObject.transform.parent = this.suckedParentObject;

			Rigidbody graspedRigidbody = this.suckedObject.GetComponent<Rigidbody>();

			graspedRigidbody.useGravity = true;
//			graspedRigidbody.isKinematic = false;
			graspedRigidbody.constraints = RigidbodyConstraints.None;

			this.suckedObject = null;
			this.suckedParentObject = null;

			SIGVerseLogger.Info("Release by SuctionDetectorRPC");
		}
#endif
	}
}

