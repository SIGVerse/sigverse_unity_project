using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN

	public abstract class CommonChat : MonoBehaviour, IChatMessageHandler
	{
		//-----------------------------

		protected GameObject chatManager;

		protected string nickName;

		protected virtual void Awake()
		{
			this.chatManager = GameObject.Find(ChatManager.ChatManagerName);

			if (this.chatManager == null)
			{
				SIGVerseLogger.Error("Could not find ChatManager!");
			}
		}

		protected virtual void Start()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			this.nickName = photonView.Owner.NickName;

			ExecuteEvents.Execute<IChatRegistrationHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnAddChatUser(this.nickName)
			);
		}

		protected virtual void Update()
		{
		}

		protected virtual void OnDisable()
		{
			ExecuteEvents.Execute<IChatRegistrationHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnRemoveChatUser(this.nickName)
			);
		}

		protected virtual void SendChatMessage(string message)
		{
			ExecuteEvents.Execute<IChatMessageHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnReceiveChatMessage(this.nickName, message)
			);
		}

		public abstract void OnReceiveChatMessage(string userName, string message);
	}
#else
	public class CommonChat : MonoBehaviour
	{
		void Start()
		{
			SIGVerseLogger.Error("SIGVERSE_PUN is NOT defined.");
		}
	}
#endif
}

