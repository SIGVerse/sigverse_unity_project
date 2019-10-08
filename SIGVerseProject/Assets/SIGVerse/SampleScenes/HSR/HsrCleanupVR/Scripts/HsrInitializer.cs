using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN

	public class HsrInitializer : MonoBehaviour
	{
		public GameObject rosBridgeScripts;

		void Awake()
		{
		}

		void Start()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			this.gameObject.name = photonView.Owner.NickName;

			if (photonView.IsMine)
			{
				this.GetComponent<GraspingDetectorForPun>().enabled = true;

				this.GetComponent<HsrChat>().enabled = true;

				this.rosBridgeScripts.SetActive(true);

				PunLauncher.EnableSubview(this.gameObject);
			}

//			this.AddPhotonComponentsToRobot(photonView);
		}

//		private void AddPhotonComponentsToRobot(PhotonView photonView)
//		{
////			photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
////			photonView.ObservedComponents.Clear();

//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.BaseFootPrintPosNoiseName), true, false);
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.BaseFootPrintRigidbodyName), true, true);
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.BaseFootPrintRotNoiseName), false, true);

//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.torso_lift_link.ToString()), true, false);
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.arm_lift_link.ToString()), true, false);

//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.arm_flex_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.arm_roll_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.wrist_flex_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.wrist_roll_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.head_pan_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.head_tilt_link.ToString()));

//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.hand_motor_dummy_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.hand_l_proximal_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.hand_r_proximal_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.hand_l_distal_link.ToString()));
//			PunLauncher.AddPhotonTransformView(photonView, SIGVerseUtils.FindGameObjectFromChild(this.transform.root, HSRCommon.Link.hand_r_distal_link.ToString()));
//		}
	}

#else
	public class HsrInitializer : MonoBehaviour
	{
		void Start()
		{
			throw new Exception("SIGVERSE_PUN is NOT defined.");
		}
	}
#endif
}
