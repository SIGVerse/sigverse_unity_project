using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class HsrInitializer : CommonInitializer
	{
#if SIGVERSE_PUN

		public GameObject rosBridgeScripts;

		void Start()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			StartCoroutine(this.SetAvatarName(photonView));

			if (photonView.IsMine)
			{
				this.GetComponent<GraspingDetectorForPun>().enabled = true;

				this.GetComponent<HsrChat>().enabled = true;

				this.rosBridgeScripts.SetActive(true);

				PunLauncher.EnableSubview(this.gameObject);
			}
		}
#endif
	}
}
