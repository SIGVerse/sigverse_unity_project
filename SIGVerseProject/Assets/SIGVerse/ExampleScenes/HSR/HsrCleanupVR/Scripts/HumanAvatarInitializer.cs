using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using NewtonVR;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN && SIGVERSE_OCULUS

	public class HumanAvatarInitializer : CommonInitializer
	{
		public GameObject ovrCameraRig;

		public GameObject ethan;

		public GameObject centerEyeAnchor;

		private void Awake()
		{
			InitializeNVRInteractables();
		}

		public static void InitializeNVRInteractables()
		{
			NVRInteractableItem[] nvrInteractableItems = SIGVerseUtils.FindObjectsOfInterface<NVRInteractableItem>();

			foreach(NVRInteractableItem nvrInteractableItem in nvrInteractableItems)
			{
				nvrInteractableItem.enabled = true;
			}
		}


		void Start()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			StartCoroutine(this.SetAvatarName(photonView));

			if (photonView.IsMine)
			{
				this.GetComponent<HumanAvatarChat>().enabled = true;

				this.ovrCameraRig.GetComponent<OVRCameraRig>().enabled = true;

				this.ovrCameraRig.GetComponent<OVRManager>().enabled = true;

				this.ovrCameraRig.GetComponent<SIGVerse.Human.IK.AnchorPostureCalculator>().enabled = true;

				this.centerEyeAnchor.GetComponent<Camera>().enabled = true;

				this.ethan.GetComponent<SimpleHumanVRControllerForPun>().enabled = true;

				CleanupAvatarVRHandControllerForRift[] cleanupAvatarVRHandControllerForRiftList = this.ethan.GetComponents<CleanupAvatarVRHandControllerForRift>();

				foreach(CleanupAvatarVRHandControllerForRift cleanupAvatarVRHandControllerForRift in cleanupAvatarVRHandControllerForRiftList)
				{
					cleanupAvatarVRHandControllerForRift.enabled = true;
				}

				PunLauncher.EnableSubview(this.gameObject);
			}
			else
			{
				this.centerEyeAnchor.GetComponent<Camera>().enabled = false;
			}
		}
	}
#endif
}
