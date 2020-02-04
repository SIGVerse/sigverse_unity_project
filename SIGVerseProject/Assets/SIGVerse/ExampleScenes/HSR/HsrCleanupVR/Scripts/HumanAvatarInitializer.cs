using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if SIGVERSE_PUN
using Photon.Pun;
using static Photon.Pun.PhotonAnimatorView;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN && SIGVERSE_OCULUS

	public class HumanAvatarInitializer : MonoBehaviour
	{
		public GameObject ovrCameraRig;

		public GameObject ethan;

		public GameObject centerEyeAnchor;

		void Awake()
		{
		}

		void Start()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			this.gameObject.name = photonView.Owner.NickName;

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

//			this.AddPhotonComponentsToHumanAvatar(photonView);
		}

//		private void AddPhotonComponentsToHumanAvatar(PhotonView photonView)
//		{
//			//photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
//			//photonView.ObservedComponents.Clear();

//			// Add PhotonAnimatorView
//			Animator[] animators = this.GetComponentsInChildren<Animator>();

//			foreach (Animator animator in animators)
//			{
//				PhotonAnimatorView photonAnimatorView = animator.gameObject.AddComponent<PhotonAnimatorView>();

////				photonAnimatorView.GetLayerSynchronizeType(); // Cannot get at this point.

//				for (int i=0; i<animator.layerCount; i++)
//				{
//					photonAnimatorView.SetLayerSynchronized(i, SynchronizeType.Discrete);
//				}

//				foreach (AnimatorControllerParameter animatorControllerParameter in animator.parameters)
//				{
//					photonAnimatorView.SetParameterSynchronized(animatorControllerParameter.name, (ParameterType)animatorControllerParameter.type, SynchronizeType.Discrete);
//				}

//				photonView.ObservedComponents.Add(photonAnimatorView);
//			}

//			// Add PhotonTransformView
//			Transform[] ovrCameraRigTransforms = ovrCameraRig.GetComponentsInChildren<Transform>();

//			foreach(Transform ovrCameraRigTransform in ovrCameraRigTransforms)
//			{
//				PunLauncher.AddPhotonTransformView(photonView, ovrCameraRigTransform.gameObject, true, true);
//			}

//			PunLauncher.AddPhotonTransformView(photonView, this.ethan, true, true);

//			// Add scripts
//			CleanupAvatarVRHandControllerForRift[] cleanupAvatarVRHandControllerForRiftList = this.ethan.GetComponents<CleanupAvatarVRHandControllerForRift>();

//			foreach (CleanupAvatarVRHandControllerForRift cleanupAvatarVRHandControllerForRift in cleanupAvatarVRHandControllerForRiftList)
//			{
//				photonView.ObservedComponents.Add(cleanupAvatarVRHandControllerForRift);
//			}
//		}
	}
#endif
}
