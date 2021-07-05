using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public abstract class CommonChat : MonoBehaviour, IChatMessageHandler
	{
		protected GameObject chatManager;
		protected GameObject mainMenu;

		protected PhotonView photonView;

		protected virtual void Awake()
		{
			this.chatManager = GameObject.Find(ChatManager.ChatManagerName);

			if (this.chatManager == null)
			{
				SIGVerseLogger.Error("Could not find ChatManager!");
			}

			this.mainMenu = GameObject.Find(ChatManager.MainMenuName);

			if (this.mainMenu == null)
			{
				SIGVerseLogger.Warn("Could not find MainMenu.");
			}
		}

		protected virtual void Start()
		{
			this.photonView = this.GetComponent<PhotonView>();

			ExecuteEvents.Execute<IChatRegistrationHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnAddChatUser(this.transform.root.name)
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
				functor: (reciever, eventData) => reciever.OnRemoveChatUser(this.transform.root.name)
			);
		}

		protected virtual void SendChatMessage(string message)
		{
			ExecuteEvents.Execute<IChatMessageHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnReceiveChatMessage(this.transform.root.name, message)
			);
		}

		public abstract void OnReceiveChatMessage(string userName, string message);
#else
	public abstract class CommonChat : MonoBehaviour 
	{
#endif
	}
}

