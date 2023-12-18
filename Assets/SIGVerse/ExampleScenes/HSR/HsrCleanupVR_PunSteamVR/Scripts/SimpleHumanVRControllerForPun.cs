using System;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Human.VR;

#if SIGVERSE_PUN
using Photon.Pun;
#endif


namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class SimpleHumanVRControllerForPun : SimpleHumanVRController
	{
#if SIGVERSE_PUN
		private PhotonView photonView;

		protected override void Start()
		{
			base.Start();

			this.photonView = this.transform.root.GetComponent<PhotonView>();
		}

		protected override void Update()
		{
			if (photonView.IsMine)
			{
				base.Update();
			}
		}
#endif
	}
}

