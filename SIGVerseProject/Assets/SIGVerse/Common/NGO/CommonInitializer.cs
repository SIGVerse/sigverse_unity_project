using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Unity.Collections;

namespace SIGVerse.Common
{
	public class CommonInitializer : NetworkBehaviour
	{
		private NetworkVariable<FixedString64Bytes> playerObjectName = new NetworkVariable<FixedString64Bytes>();

		public void Awake()
		{
			this.playerObjectName.OnValueChanged += OnPlayerObjectNameChanged;
		}

		public override void OnNetworkSpawn()
		{
			OnPlayerObjectNameChanged(this.playerObjectName.Value, this.playerObjectName.Value);
		}

		private void OnPlayerObjectNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
		{
			this.name = current.Value.ToString();
		}

		public void SetPlayerObjectName(string name)
		{
			this.playerObjectName.Value = name;
		}

		protected void DisableCollision()
		{
			this.GetComponentsInChildren<Collider>().ToList().ForEach(x => x.enabled = false);
		}

		protected void MakeRigidbodyKinematic()
		{
			Rigidbody[] rigidbodies = this.GetComponentsInChildren<Rigidbody>(true);

			foreach (Rigidbody rigidbody in rigidbodies)
			{
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
		}
	}
}

