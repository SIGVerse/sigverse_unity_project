using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.UI;
using Unity.Netcode;

namespace SIGVerse.Common
{
	public class OwnerChanger : NetworkBehaviour
	{
		public static string[] OwnerTags = new string[] { "Player", "Human", "Robot" };

		//private Transform parentObj;

		public override void OnNetworkSpawn()
		{
			//this.parentObj = this.transform.parent;
		}

		void OnCollisionEnter(Collision collision)
		{
//			if (!IsServer) { return; }

			if (NetworkObject.IsOwner) { return; }

			if(!this.ShouldChangeOwner(collision)) { return; }

			NetworkObject playerNetworkObject = collision.transform.root.GetComponent<NetworkObject>();

			if (playerNetworkObject == null)
			{
				SIGVerseLogger.Error("Player does not have NetworkObject. Player=" + collision.transform.root.name);
				return;
			}

//			NetworkObject.ChangeOwnership(playerNetworkObject.OwnerClientId);
			GetOwnershipServerRpc();
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

		//public void ChangeOwnerByServer()
		//{
		//	ChangeOwnerServerRpc();
		//}

		//public void RemoveOwnershipByServer()
		//{
		//	RemoveOwnershipServerRpc();
		//}
		
		//public void ChangeOwnerAndSetParentByServer(NetworkObject parent)
		//{
		//	ChangeOwnerAndSetParentServerRpc(parent);
		//}

		//public void RemoveOwnershipAndParentByServer()
		//{
		//	RemoveOwnershipAndParentServerRpc();
		//}


		[ServerRpc(RequireOwnership = false)]
		public void GetOwnershipServerRpc(ServerRpcParams serverRpcParams = default)
		{
			NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

			Debug.Log("ChangeOwnership name=" + this.name);
		}

		[ServerRpc(RequireOwnership = false)]
		public void RemoveOwnershipServerRpc(ServerRpcParams serverRpcParams = default)
		{
			NetworkObject.RemoveOwnership();

			Debug.Log("RemoveOwnership name=" + this.name);
		}
		
		//[ServerRpc(RequireOwnership = false)]
		//private void ChangeOwnerAndSetParentServerRpc(NetworkObjectReference parent, ServerRpcParams serverRpcParams = default)
		//{
		//	NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);

		//	if (parent.TryGet(out NetworkObject parentObject))
		//	{
		//		NetworkObject.TrySetParent(parentObject);
		//	}
			
		//	Debug.Log("ChangeOwner name=" + this.name+", parent="+parentObject.name);
		//}

		//[ServerRpc(RequireOwnership = false)]
		//private void RemoveOwnershipAndParentServerRpc()
		//{
		//	NetworkObject.RemoveOwnership();

		//	NetworkObject.TrySetParent(parentObj);
			
		//	Debug.Log("RemoveOwnership name=" + this.name);
		//}
	}
}
