using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SIGVerse.Common
{
	public class PlayerSpawner : NetworkBehaviour
	{
		[TooltipAttribute("[s]")]
		public float lifeSpan = 5.0f;

		private static string initPrefabName;
		private static Vector3 initPosition;
		private static Quaternion initRotation;
		private static string initPlayerName;

		public override void OnNetworkSpawn()
		{
			if (IsOwner && NetworkManager.Singleton.IsClient)
			{
				SpawnPlayerServerRpc(initPrefabName, initPosition, initRotation, initPlayerName);
			}
			if (NetworkManager.Singleton.IsServer)
			{
				StartCoroutine(DespawnPlayerSpawner());
			}
		}

		private IEnumerator DespawnPlayerSpawner()
		{
			yield return new WaitForSeconds(this.lifeSpan);

			this.GetComponent<NetworkObject>().Despawn();
		}

		public void SetPlayerInfo(string prefabName, Vector3 position, Quaternion rotation, string playerName)
		{
			PlayerSpawner.initPrefabName = prefabName;
			PlayerSpawner.initPosition   = position;
			PlayerSpawner.initRotation   = rotation;
			PlayerSpawner.initPlayerName = playerName;
		}

		[ServerRpc]
		private void SpawnPlayerServerRpc(string prefabName, Vector3 position, Quaternion rotation, string playerName, ServerRpcParams serverRpcParams = default)
		{
			StartCoroutine(SpawnPlayerCoroutine(prefabName, position, rotation, playerName, serverRpcParams));
		}

		private IEnumerator SpawnPlayerCoroutine(string prefabName, Vector3 position, Quaternion rotation, string playerName, ServerRpcParams serverRpcParams = default)
		{
			// Workarounds to avoid a bug.
			// https://issuetracker.unity3d.com/issues/netcode-for-gameobject-spawnasplayerobject-does-not-change-networkmanager-dot-singleton-dot-localclient-dot-playerobject-when-the-gameobject-is-instantiated-right-after-the-host-is-started
			yield return null; 

			NetworkPrefab networkPrefab = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Where(x => x.Prefab.name==prefabName).First();
			NetworkObject playerPrefab = networkPrefab.Prefab.GetComponent<NetworkObject>();

			if (playerPrefab == null) { throw new Exception("PlayerPrefab is null. name=" + prefabName); }

			NetworkObject newPlayer = Instantiate(playerPrefab, position, rotation);

			if (newPlayer == null) { throw new Exception("Player instance is null. name=" + prefabName); }

			newPlayer.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);

			if (newPlayer.TryGetComponent<CommonInitializer>(out var initializer)) 
			{
				initializer.SetPlayerObjectName(playerName + serverRpcParams.Receive.SenderClientId.ToString("00") + "#" + prefabName);
			}
		}
	}
}
