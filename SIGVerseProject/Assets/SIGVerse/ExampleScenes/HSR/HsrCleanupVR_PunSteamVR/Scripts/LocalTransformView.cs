using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class LocalTransformView : MonoBehaviour, IPunObservable
	{
		public bool syncLocalPosition = true;
		public bool syncLocalRotation = true;

		void Start()
		{
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				if(this.syncLocalPosition)
				{
					stream.SendNext(this.transform.localPosition);
				}
				if (this.syncLocalRotation)
				{
					stream.SendNext(this.transform.localRotation);
				}
			}
			else
			{
				if (this.syncLocalPosition)
				{
					this.transform.localPosition = (Vector3)stream.ReceiveNext();
				}
				if (this.syncLocalRotation)
				{
					this.transform.localRotation = (Quaternion)stream.ReceiveNext();
				}
			}
		}
	}
#endif
}

