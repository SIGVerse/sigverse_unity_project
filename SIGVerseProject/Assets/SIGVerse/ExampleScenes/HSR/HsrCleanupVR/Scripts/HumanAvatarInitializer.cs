using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

#if SIGVERSE_STEAMVR
using Valve.VR;
using Valve.VR.InteractionSystem;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class HumanAvatarInitializer : CommonInitializer
	{
		public GameObject cameraRig;

		public GameObject ethan;

		public GameObject eyeAnchor;

#if SIGVERSE_PUN && SIGVERSE_STEAMVR

		private void Awake()
		{
//			InitializeNVRInteractables();
		}

		public static void InitializeNVRInteractables()
		{
			//NVRInteractableItem[] nvrInteractableItems = SIGVerseUtils.FindObjectsOfInterface<NVRInteractableItem>();

			//foreach(NVRInteractableItem nvrInteractableItem in nvrInteractableItems)
			//{
			//	nvrInteractableItem.enabled = true;
			//}
		}


		void Start()
		{
			SteamVR_Actions.sigverse.Activate(SteamVR_Input_Sources.Any);

			this.photonView = this.GetComponent<PhotonView>();

			StartCoroutine(this.SetAvatarName());
			StartCoroutine(this.EnableScripts());
		}

		private IEnumerator EnableScripts()
		{
			while(!this.isNameSet)
			{
				yield return null;
			}

			if (this.photonView.IsMine)
			{
				this.GetComponent<HumanAvatarChat>().enabled = true;

				this.GetComponent<Player>().enabled = true;

//				this.cameraRig.GetComponent<SteamVR_PlayArea>().enabled = true;

				this.cameraRig.GetComponent<SIGVerse.Human.IK.AnchorPostureCalculator>().enabled = true;

				SteamVR_Behaviour_Pose[] steamVrBehaviourPoses = this.cameraRig.GetComponentsInChildren<SteamVR_Behaviour_Pose>();

				foreach(SteamVR_Behaviour_Pose steamVrBehaviourPose in steamVrBehaviourPoses)
				{
					steamVrBehaviourPose.enabled = true;
				}

				Hand[] hands = this.cameraRig.GetComponentsInChildren<Hand>(true);

				foreach(Hand hand in hands)
				{
					hand.enabled = true;
				}

				this.eyeAnchor.GetComponent<Camera>().enabled = true;

				this.eyeAnchor.GetComponent<AudioListener>().enabled = true;

				this.eyeAnchor.GetComponent<SteamVR_CameraHelper>().enabled = true;

				this.ethan.GetComponent<SimpleHumanVRControllerForPun>().enabled = true;

				this.EnableCleanupAvatarVRHandControllerForSteamVR();

				PunLauncher.EnableSubview(this.gameObject);
			}
			else
			{
				Rigidbody[] rigidbodies = this.GetComponentsInChildren<Rigidbody>(true);

				foreach(Rigidbody rigidbody in rigidbodies)
				{
					rigidbody.useGravity = false;
//					rigidbody.isKinematic = true;
				}

				this.EnableCleanupAvatarVRHandControllerForSteamVR();
			}
		}

		private void EnableCleanupAvatarVRHandControllerForSteamVR()
		{
			CleanupAvatarVRHandControllerForSteamVR[] cleanupAvatarVRHandControllerForSteamVRs = this.ethan.GetComponents<CleanupAvatarVRHandControllerForSteamVR>();

			foreach (CleanupAvatarVRHandControllerForSteamVR cleanupAvatarVRHandControllerForSteamVR in cleanupAvatarVRHandControllerForSteamVRs)
			{
				cleanupAvatarVRHandControllerForSteamVR.enabled = true;
			}
		}
#endif
	}
}
