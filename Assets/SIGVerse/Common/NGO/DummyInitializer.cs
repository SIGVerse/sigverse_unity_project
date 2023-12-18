using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Common
{
	public class DummyInitializer : CommonInitializer
	{
		private bool isInitialized = false;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			this.Initialize();
		}

		void Start()
		{
			this.Initialize();
		}

		private void Initialize()
		{
			// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkbehavior/#spawning

			if (this.isInitialized) { return; }

			this.isInitialized = true;

			if (IsOwner || !IsSpawned)
			{
				this.GetComponentInChildren<Camera>().enabled = true;
				this.GetComponentInChildren<AudioListener>().enabled = true;
			}
		}
	}
}
