using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using UnityEngine.UI;
using SIGVerse.RosBridge;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class HumanAvatarChat : CommonChat
	{
		public GameObject personalPanel;

		//-----------------------------

		private const string MsgPickItUp = "Pick it up!";
		private const string MsgCleanUp  = "Clean up!";

		private const float SendingMessageInterval = 0.5f;

		private float latestSendingMessageTime = 0.0f;

		protected override void Update()
		{
			base.Update();

			if(Time.time - this.latestSendingMessageTime > SendingMessageInterval)
			{
				if (OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.X))
				{
					this.PublishMessage(MsgPickItUp);
				}
				else if (OVRInput.GetDown(OVRInput.RawButton.B) || OVRInput.GetDown(OVRInput.RawButton.Y))
				{
					this.PublishMessage(MsgCleanUp);
				}
			}
		}

		private void PublishMessage(string message)
		{
			this.SendChatMessage(message);

			this.latestSendingMessageTime = Time.time;

			SIGVerseLogger.Info("Human: Sent the message. user=" + this.nickName + ", message=" + message);
		}

		/// <summary>
		/// Anyone -> Human Avatar
		/// </summary>
		public override void OnReceiveChatMessage(string userName, string message)
		{
			this.SendPanelNotice(userName, message);

			SIGVerseLogger.Info("Human: Received the message. user=" + userName + ", message=" + message);
		}

		private void SendPanelNotice(string userName, string message)
		{
			string dispMessage = (userName == PhotonNetwork.NickName) ? "You:                    \n" + message : userName + ":                    \n" + message;

			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(dispMessage, 100, PanelNoticeStatus.Green, 5.0f);

			// For changing the notice of the panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.personalPanel,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);
		}
	}
#else
	public class HumanAvatarChat : MonoBehaviour
	{
		void Start()
		{
			SIGVerseLogger.Error("SIGVERSE_PUN is NOT defined.");
		}
	}
#endif
}

