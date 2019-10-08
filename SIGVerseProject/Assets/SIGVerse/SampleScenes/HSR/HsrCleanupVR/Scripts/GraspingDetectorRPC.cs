using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class GraspingDetectorRPC : MonoBehaviour
	{
		public GameObject handPalm;

		private GameObject graspedObject;
		private Transform  graspedParentObject;

		[PunRPC]
		protected void GraspRPC(string objectName)
		{
			this.graspedObject = RoomObjectManager.Instance.GetRoomObject(objectName);

			// Save the parent object
			this.graspedParentObject = this.graspedObject.transform.parent;

			// Change parent object to the robot hand
			this.graspedObject.transform.parent = this.handPalm.transform;

			Rigidbody graspedRigidbody = this.graspedObject.GetComponent<Rigidbody>();

			graspedRigidbody.useGravity = false;
//			graspedRigidbody.isKinematic = true;
			graspedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			SIGVerseLogger.Info("Grasp by GraspingDetectorRPC :" + objectName);
		}

		[PunRPC]
		protected void ReleaseRPC()
		{
			if(this.graspedObject == null)
			{
				SIGVerseLogger.Warn("GraspedObject is null");
				return;
			}

			// Restore the parent object
			this.graspedObject.transform.parent = this.graspedParentObject;

			Rigidbody graspedRigidbody = this.graspedObject.GetComponent<Rigidbody>();

			graspedRigidbody.useGravity = true;
//			graspedRigidbody.isKinematic = false;
			graspedRigidbody.constraints = RigidbodyConstraints.None;

			this.graspedObject = null;
			this.graspedParentObject = null;

			SIGVerseLogger.Info("Release by GraspingDetectorRPC");
		}
	}
#endif
}

