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
				this.GetComponent<GraspingDetectorForPun>().enabled = true;

				this.GetComponent<AudioListener>().enabled = true;

				this.GetComponent<HsrChat>().enabled = true;

				this.rosBridgeScripts.SetActive(true);

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
			}
		}
#endif
	}
}
