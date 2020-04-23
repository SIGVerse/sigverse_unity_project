using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;
using SIGVerse.RosBridge;
#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class HsrChat : CommonChat, IRosReceivingStringMsgHandler
	{
		public GameObject rosBridgeScripts;

		//-----------------------------

		/// <summary>
		/// Human Avatar -> Robot
		/// </summary>
		public override void OnReceiveChatMessage(string senderName, string message)
		{
			if (this.photonView.Owner != PhotonNetwork.LocalPlayer) { return; }

			string senderNickName = senderName.Split('#')[0];

			if (senderNickName==PhotonNetwork.NickName) { senderNickName = "You"; }

			// Display the message
			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(senderNickName, message, PanelNoticeStatus.Green);

			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			// Forward the message to ROS
			ExecuteEvents.Execute<SIGVerse.RosBridge.IRosSendingStringMsgHandler>
			(
				target: this.rosBridgeScripts,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnSendRosStringMsg(message)
			);

			SIGVerseLogger.Info("Robot: Received a message. sender=" + senderName + ", message=" + message);
		}

		/// <summary>
		/// Robot -> Human Avatar
		/// </summary>
		public void OnReceiveRosStringMsg(RosBridge.std_msgs.String rosMsg)
		{
			this.SendChatMessage(rosMsg.data);

			SIGVerseLogger.Info("Robot: Sent a message. sender=" + this.transform.root.name + ", message=" + rosMsg.data);
		}
	}
#endif
}
