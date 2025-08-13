using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;
using SIGVerse.RosBridge;
using SIGVerse.ExampleScenes.Hsr;

namespace SIGVerse.Common
{
	public class RobotChat : CommonChat, IRosReceivingStringMsgHandler
	{
		public GameObject rosBridgeScripts;

		//-----------------------------

		private GameObject mainMenu;

		protected override void Awake()
		{
			base.Awake();

			this.mainMenu = GameObject.Find(ChatManager.MainMenuName);

			if (this.mainMenu == null)
			{
				SIGVerseLogger.Error("Could not find MainMenu.");
			}
		}

		/// <summary>
		/// Human Avatar -> Robot
		/// </summary>
		public override void OnReceiveChatMessage(string senderName, string message)
		{
			if (!IsOwner) { return; }

			// Display the message
			string speaker = (senderName == GetMyName()) ? "You" : senderName;

			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(speaker, message, PanelNoticeStatus.Green);

			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			// Forward the message to ROS
			if (senderName != GetMyName())
			{
				ExecuteEvents.Execute<SIGVerse.RosBridge.IRosSendingStringMsgHandler>
				(
					target: this.rosBridgeScripts,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnSendRosStringMsg(message)
				);
			}

			SIGVerseLogger.Info("Robot: Received a message. sender=" + senderName + ", message=" + message);
		}

		/// <summary>
		/// Robot -> Human Avatar
		/// </summary>
		public void OnReceiveRosStringMsg(RosBridge.std_msgs.msg.String rosMsg)
		{
			this.SendChatMessage(rosMsg.data);

			SIGVerseLogger.Info("Robot: Sent a message. sender=" + this.transform.root.name + ", message=" + rosMsg.data);
		}
	}
}
