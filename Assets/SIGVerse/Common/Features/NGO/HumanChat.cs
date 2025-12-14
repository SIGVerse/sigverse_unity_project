using SIGVerse.Common;
using SIGVerse.ExampleScenes.Hsr;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace SIGVerse.Common
{
	public class HumanChat : CommonChat
	{
		public bool useSpeechSynthesizer = true;

		public GameObject personalPanel;

		public string msgRightPrimaryButton   = "Pick it up!";
		public string msgRightSecondaryButton = "Clean up!";
		public string msgLeftPrimaryButton    = "Pick it up!";
		public string msgLeftSecondaryButton  = "Clean up!";

		public string msgRightPrimary2DAxisClick = "Good!";
		public string msgLeftPrimary2DAxisClick  = "Bad!";

		[SerializeField] private InputActionReference leftPrimaryButton;
		[SerializeField] private InputActionReference leftSecondaryButton;
		[SerializeField] private InputActionReference leftThumbstickClick;
		[SerializeField] private InputActionReference rightPrimaryButton;
		[SerializeField] private InputActionReference rightSecondaryButton;
		[SerializeField] private InputActionReference rightThumbstickClick;
		//-----------------------------

		private const float MessageSendingInterval = 0.5f;

		private float lastMessageSentTime = 0.0f;

		private SAPISpeechSynthesis sapiSpeechSynthesis;

		protected override void Awake()
		{
			base.Awake();

			if(this.useSpeechSynthesizer)
			{
				GameObject ttsObj = GameObject.Find(SAPISpeechSynthesis.SAPISpeechSynthesisName);

				if (ttsObj == null)
				{
					SIGVerseLogger.Warn("Could not find SAPISpeechSynthesis.");
				}
				else
				{
					this.sapiSpeechSynthesis = ttsObj.GetComponent<SAPISpeechSynthesis>();
				}
			}
		}

		protected override void Update()
		{
			if (!IsOwner) { return; }

			base.Update();

			if (Time.time - this.lastMessageSentTime > MessageSendingInterval)
			{
				if(this.leftPrimaryButton.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgLeftPrimaryButton);
				}
				if(this.leftSecondaryButton.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgLeftSecondaryButton);
				}
				if(this.rightPrimaryButton.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgRightPrimaryButton);
				}
				if(this.rightSecondaryButton.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgRightSecondaryButton);
				}

				if(this.leftThumbstickClick.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgLeftPrimary2DAxisClick);
				}
				if(this.rightThumbstickClick.action.WasPressedThisFrame())
				{
					this.PublishMessage(this.msgRightPrimary2DAxisClick);
				}
			}
		}

		private void PublishMessage(string message)
		{
			this.SendChatMessage(message);

			this.lastMessageSentTime = Time.time;

			SIGVerseLogger.Info("Human: Sent a message. sender=" + this.transform.root.name + ", message=" + message);
		}

		/// <summary>
		/// Anyone -> Human Avatar
		/// </summary>
		public override void OnReceiveChatMessage(string senderName, string message)
		{
			if (!IsOwner) { return; }

			string speaker = (senderName == GetMyName()) ? "You" : senderName;

			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(speaker, message, PanelNoticeStatus.Green);

			// For changing the notice of the panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.personalPanel,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			if(this.useSpeechSynthesizer)
			{
				this.sapiSpeechSynthesis.Speak(message);
			}

			SIGVerseLogger.Info("Human: Received a message. sender=" + senderName + ", message=" + message);
		}
	}
}

