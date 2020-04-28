using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using UnityEngine.UI;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN
	public class HumanAvatarChat : CommonChat
	{
		public GameObject personalPanel;

		//-----------------------------

		private const string MsgPickItUp = "Pick it up!";
		private const string MsgCleanUp  = "Clean up!";
		private const string MsgGood     = "Good!";
		private const string MsgBad      = "Bad!";

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
				else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick))
				{
					this.PublishMessage(MsgGood);
				}
				else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick))
				{
					this.PublishMessage(MsgBad);
				}
			}
		}

		private void PublishMessage(string message)
		{
			this.SendChatMessage(message);

			this.latestSendingMessageTime = Time.time;

			SIGVerseLogger.Info("Human: Sent a message. sender=" + this.transform.root.name + ", message=" + message);
		}

		/// <summary>
		/// Anyone -> Human Avatar
		/// </summary>
		public override void OnReceiveChatMessage(string senderName, string message)
		{
			if (this.photonView.Owner != PhotonNetwork.LocalPlayer) { return; }

			string senderNickName = senderName.Split('#')[0];

			if (senderNickName==PhotonNetwork.NickName) { senderNickName = "You"; }

			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(senderNickName, message, PanelNoticeStatus.Green);

			// For changing the notice of the panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.personalPanel,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			SIGVerseLogger.Info("Human: Received a message. sender=" + senderName + ", message=" + message);
		}
	}
#endif
}

