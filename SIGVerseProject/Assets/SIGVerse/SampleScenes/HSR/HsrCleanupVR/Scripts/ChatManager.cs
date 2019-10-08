using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using SIGVerse.Common;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
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

#if SIGVERSE_PUN

	public class ChatManager : MonoBehaviour, IChatRegistrationHandler, IChatMessageHandler
	{

		public const string ChatManagerName = "ChatManager";

		//-----------------------------

		private PhotonView photonView;

		private Dictionary<string, GameObject> userMap;

		void Start()
		{
			this.photonView = this.GetComponent<PhotonView>();

			this.userMap = new Dictionary<string, GameObject>();
		}

		public void OnReceiveChatMessage(string senderName, string message)
		{
			SIGVerseLogger.Info("Receive ChatMessage on ChatManager. user=" + senderName + ", message=" + message);

			this.photonView.RPC("PublishMessage", RpcTarget.All, senderName, message);
		}

		[PunRPC]
		private void PublishMessage(string senderName, string message)
		{
			SIGVerseLogger.Info("PublishMessage userName=" + senderName + ", message=" + message);

//			foreach (KeyValuePair<string, GameObject> user in this.userMap)
			{
//				if (user.Key != PhotonNetwork.NickName) { continue; } // Publish a message only to logged-in user objects.

				// Publish a message only to logged-in user objects.
				ExecuteEvents.Execute<IChatMessageHandler>
				(
					target: this.userMap[PhotonNetwork.NickName],
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveChatMessage(senderName, message)
				);

				//if(user.Value==null)
				//{
				//	SIGVerseLogger.Warn("user.Value==null");
				//}
				//SIGVerseLogger.Info("sent to "+ user.Key);
			}
		}

		public void OnAddChatUser(string userName)
		{
			this.photonView.RPC("AddChatUser", RpcTarget.AllBuffered, userName);
		}

		[PunRPC]
		private void AddChatUser(string userName)
		{
			SIGVerseLogger.Info("AddChatUser name="+userName);

			// Wait for GameObject creation
			StartCoroutine(AddChatUserAfter3sec(userName));
		}

		private IEnumerator AddChatUserAfter3sec(string userName)
		{
			yield return new WaitForSeconds(3.0f);

			this.userMap.Add(userName, GameObject.Find(userName));
		}

		public void OnRemoveChatUser(string userName)
		{
			this.photonView.RPC("RemoveChatUser", RpcTarget.AllBuffered, userName);
		}

		[PunRPC]
		private void RemoveChatUser(string userName)
		{
			this.userMap.Remove(userName);
		}
	}
#endif
}

