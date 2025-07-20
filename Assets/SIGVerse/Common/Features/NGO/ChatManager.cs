using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using Unity.Netcode;

namespace SIGVerse.Common
{
	public interface IChatRegistrationHandler : IEventSystemHandler
	{
		void OnAddChatUser(string userName);
		void OnRemoveChatUser(string userName);
	}

	public interface IChatMessageHandler : IEventSystemHandler
	{
		void OnReceiveChatMessage(string senderName, string message);
	}


	public class ChatManager : NetworkBehaviour, IChatRegistrationHandler, IChatMessageHandler
	{
		public const string ChatManagerName = "ChatManager";
		public const string MainMenuName    = "MainMenu";

		//-----------------------------

		public GameObject[] extraMessageDestinations;

		private GameObject mainMenu;

		private Dictionary<string, GameObject> userMap;

		void Awake()
		{
			this.mainMenu = GameObject.Find(ChatManager.MainMenuName);

			if (this.mainMenu == null)
			{
				SIGVerseLogger.Warn("Could not find MainMenu.");
			}
		}

		void Start()
		{
			this.ClearChatUserList();
		}

		public void OnReceiveChatMessage(string senderName, string message)
		{
			SIGVerseLogger.Info("Receive ChatMessage on ChatManager. sender=" + senderName + ", message=" + message);

			ForwardMessageServerRpc(senderName, message);
		}

		[ServerRpc(RequireOwnership = false)]
		private void ForwardMessageServerRpc(string senderName, string message)
		{
			ForwardMessageClientRpc(senderName, message);
		}

		[ClientRpc]
		private void ForwardMessageClientRpc(string senderName, string message)
		{
			SIGVerseLogger.Info("ForwardMessage sender=" + senderName + ", message=" + message + ", chat user num="+this.userMap.Keys.Count);

			// Forward the message 
			foreach (KeyValuePair<string, GameObject> user in this.userMap)
			{
				// Publish a message to logged-in user objects.
				ExecuteEvents.Execute<IChatMessageHandler>
				(
					target: user.Value,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveChatMessage(senderName, message)
				);
			}

			if(this.extraMessageDestinations!=null)
			{
				foreach(GameObject extraMessageDestination in this.extraMessageDestinations)
				{
					ExecuteEvents.Execute<IChatMessageHandler>
					(
						target: extraMessageDestination,
						eventData: null,
						functor: (reciever, eventData) => reciever.OnReceiveChatMessage(senderName, message)
					);
				}
			}
		}

		public void OnAddChatUser(string userName)
		{
			Debug.Log("OnAddChatUser  Call Name="+userName);
			AddChatUserServerRpc(userName);
		}

		[ServerRpc(RequireOwnership = false)]
		private void AddChatUserServerRpc(string userName)
		{
//			Debug.Log("AddChatUserServerRpc  Passing Name="+userName);
			AddChatUserClientRpc(userName);
		}

		[ClientRpc]
		private void AddChatUserClientRpc(string userName)
		{
//			Debug.Log("AddChatUserClientRpc  Passing Name="+userName);
			// Wait for GameObject creation
			//StartCoroutine(AddChatUserAfter1sec(userName));

			// --
			this.userMap.Add(userName, GameObject.Find(userName));

			SIGVerseLogger.Info("AddChatUser name="+userName+", UserCount="+this.userMap.Count);
		}

		//private IEnumerator AddChatUserAfter1sec(string userName)
		//{
		//	yield return new WaitForSeconds(1.0f);

		//	this.userMap.Add(userName, GameObject.Find(userName));

		//	SIGVerseLogger.Info("AddChatUser name="+userName+", UserCount="+this.userMap.Count);
		//}

		public void OnRemoveChatUser(string userName)
		{
			RemoveChatUserServerRpc(userName);
		}

		[ServerRpc(RequireOwnership = false)]
		private void RemoveChatUserServerRpc(string userName)
		{
			RemoveChatUserClientRpc(userName);
		}

		[ClientRpc]
		private void RemoveChatUserClientRpc(string userName)
		{
			this.userMap.Remove(userName);
		}

		public void ClearChatUserList()
		{
			this.userMap = new Dictionary<string, GameObject>();
		}
	}
}

