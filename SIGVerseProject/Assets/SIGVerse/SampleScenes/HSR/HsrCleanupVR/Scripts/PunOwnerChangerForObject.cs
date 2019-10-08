using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.UI;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN

	public class PunOwnerChangerForObject : MonoBehaviour
	{
		public static string[] OwnerTags = new string[] { "Player", "Robot" };

		private PhotonView photonView;

		void Awake()
		{
		}

		void Start()
		{
			this.photonView = this.GetComponent<PhotonView>();

			if(this.photonView == null)
			{
				SIGVerseLogger.Error("There is no PhotonView. GameObject=" + this.name);
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if(!this.ShouldChangeOwner(collision)) { return; }

			PhotonView ownerPhotonView = collision.transform.root.GetComponent<PhotonView>();

			if (ownerPhotonView == null)
			{
				SIGVerseLogger.Error("Player does not have PhotonView. Player=" + collision.transform.root.name);
				return;
			}

			this.photonView.TransferOwnership(ownerPhotonView.Owner);
		}

		private bool ShouldChangeOwner(Collision collision)
		{
			foreach (string ownerTags in OwnerTags)
			{
				if (collision.transform.root.tag == ownerTags)
				{
					return true;
				}
			}

			return false;
		}
	}

#else
	public class PunOwnerChangerForObject : MonoBehaviour
	{
		void Start()
		{
			SIGVerseLogger.Error("SIGVERSE_PUN is NOT defined.");
		}
	}
#endif
}
