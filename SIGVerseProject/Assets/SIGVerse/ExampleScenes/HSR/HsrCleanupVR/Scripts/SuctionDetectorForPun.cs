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
	public class SuctionDetectorForPun : SuctionDetector
	{
#if SIGVERSE_PUN

		private PhotonView photonView;

		protected override void Start()
		{
			base.Start();

			this.photonView = this.GetComponent<PhotonView>();
		}

		protected override void Suck(Rigidbody collidedRigidbody)
		{
			base.Suck(collidedRigidbody);

			this.photonView.RPC("SuctionSuckRPC", RpcTarget.Others, collidedRigidbody.name);
		}

		protected override void Release()
		{
			base.Release();

			this.photonView.RPC("SuctionReleaseRPC", RpcTarget.Others);
		}
#endif
	}
}

