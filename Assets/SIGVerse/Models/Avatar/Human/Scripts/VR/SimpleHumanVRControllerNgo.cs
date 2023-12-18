using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace SIGVerse.Human.VR
{
	public class SimpleHumanVRControllerNgo : SimpleHumanVRController
	{
		private NetworkObject networkObject;

		protected override void Awake()
		{
			base.Awake();

			this.networkObject = this.transform.root.GetComponent<NetworkObject>();
		}

		protected override void Update()
		{
			if (this.networkObject.IsOwner || !this.networkObject.IsSpawned)
			{
				base.Update();
			}
		}
	}
}

