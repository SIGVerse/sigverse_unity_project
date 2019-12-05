using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;
using SIGVerse.RosBridge;

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class HsrChat : CommonChat, IRosReceivingStringMsgHandler
	{
		public GameObject rosBridgeScripts;

		//-----------------------------

		/// <summary>
		/// Human Avatar -> Robot
		/// </summary>
		public override void OnReceiveChatMessage(string userName, string message)
		{
			if (!userName.StartsWith(this.name)) { return; }

			ExecuteEvents.Execute<SIGVerse.RosBridge.IRosSendingStringMsgHandler>
			(
				target: this.rosBridgeScripts,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnSendRosStringMsg(message)
			);

			SIGVerseLogger.Info("Robot: Received the message. user=" + userName + ", message=" + message);
		}

		/// <summary>
		/// Robot -> Human Avatar
		/// </summary>
		public void OnReceiveRosStringMsg(RosBridge.std_msgs.String rosMsg)
		{
			this.SendChatMessage(rosMsg.data);

			SIGVerseLogger.Info("Robot: Sent the message. user=" + this.nickName + ", message=" + rosMsg.data);
		}
	}
#endif
}
