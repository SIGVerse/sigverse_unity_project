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
	public class CommonInitializer : MonoBehaviour
	{
#if SIGVERSE_PUN

		protected IEnumerator SetAvatarName(PhotonView photonView)
		{
			object avatarNameObj;

			while (!photonView.Owner.CustomProperties.TryGetValue(PunLauncher.AvatarNameKey, out avatarNameObj))
			{
				yield return null;
			}

			this.gameObject.name = (string)avatarNameObj;
		}
#endif
	}
}

