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

#if SIGVERSE_STEAMVR
using Valve.VR;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class HumanAvatarChat : CommonChat
	{
#if SIGVERSE_PUN
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
#if SIGVERSE_STEAMVR
				if (SteamVR_Actions.sigverse.PressNearButton.GetStateDown(SteamVR_Input_Sources.Any))
				{
					this.PublishMessage(MsgPickItUp);
				}
				else if (SteamVR_Actions.sigverse_PressFarButton.GetStateDown(SteamVR_Input_Sources.Any))
				{
					this.PublishMessage(MsgCleanUp);
				}
				else if (SteamVR_Actions.sigverse_PressThumbstick.GetStateDown(SteamVR_Input_Sources.RightHand))
				{
					this.PublishMessage(MsgGood);
				}
				else if (SteamVR_Actions.sigverse_PressThumbstick.GetStateDown(SteamVR_Input_Sources.LeftHand))
				{
					this.PublishMessage(MsgBad);
				}
#endif
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

			string speaker = (senderNickName == PhotonNetwork.NickName) ? "You" : senderNickName;

			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(speaker, message, PanelNoticeStatus.Green);

			// For changing the notice of the panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.personalPanel,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			SIGVerseLogger.Info("Human: Received a message. sender=" + senderName + ", message=" + message);
		}
#endif
	}
}

