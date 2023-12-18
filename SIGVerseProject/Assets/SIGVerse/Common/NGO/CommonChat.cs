using System;
using System.Collections;
using SIGVerse.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;


namespace SIGVerse.Common
{
	public abstract class CommonChat : NetworkBehaviour, IChatMessageHandler
	{
		protected GameObject chatManager;

		//private bool sentAddChatUser = false;

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
			//Debug.Log("Start User="+this.name);
			//SendAddChatUser();
		}

		//public override void OnNetworkSpawn()
		//{
		//	Debug.Log("OnNetworkSpawn User="+this.name+", IsOwner="+IsOwner+", enabled="+this.enabled);

		//	if(IsOwner && this.enabled)
		//	{
		//		SendAddChatUser();
		//	}
		//}

		public virtual void SendAddChatUser()
		{
			StartCoroutine(SendAddChatUserCoroutine());
		}

		public IEnumerator SendAddChatUserCoroutine()
		{
			while(GetMyName()==string.Empty)
			{
				yield return null;
			}

			ExecuteEvents.Execute<IChatRegistrationHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnAddChatUser(GetMyName())
			);
			
			Debug.Log("SendAddChatUser name="+GetMyName());
		}

		protected virtual void Update()
		{
		}


		protected virtual void OnApplicationQuit()
		{
			ExecuteEvents.Execute<IChatRegistrationHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnRemoveChatUser(GetMyName())
			);
		}

		public virtual void SendChatMessage(string message, string senderName = null)
		{
			if (senderName == null) { senderName = GetMyName(); }

			ExecuteEvents.Execute<IChatMessageHandler>
			(
				target: this.chatManager,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnReceiveChatMessage(senderName, message)
			);
		}

		protected virtual string GetMyName()
		{
			return this.transform.root.name;
		}

		public abstract void OnReceiveChatMessage(string userName, string message);
	}
}

