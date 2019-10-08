using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if (SIGVERSE_PUN)
	public class LocalTransformView : MonoBehaviour, IPunObservable
	{
		//public bool syncLocalPositionX;
		//public bool syncLocalPositionY;
		//public bool syncLocalPositionZ;

		void Start()
		{
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				stream.SendNext(this.transform.localPosition);
			}
			else
			{
				this.transform.localPosition = (Vector3)stream.ReceiveNext();
			}
		}
	}
#else
	public class LocalTransformView : MonoBehaviour
	{
	}
#endif
}

