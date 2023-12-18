using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using UnityEngine.UI;
using SIGVerse.ExampleScenes.Hsr;
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
		//-----------------------------

		private const float MessageSendingInterval = 0.5f;

		private float lastMessageSentTime = 0.0f;

		private SAPISpeechSynthesis sapiSpeechSynthesis;

		private InputDevice leftHandDevice;
		private InputDevice rightHandDevice;

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

		protected override void Start()
		{
			base.Start();

			StartCoroutine(GetLeftHandDevice(XRNode.LeftHand));
			StartCoroutine(GetRightHandDevice(XRNode.RightHand));

			//StartCoroutine(GetXrDevice(XRNode.LeftHand,  this.leftHandDevice));
			//StartCoroutine(GetXrDevice(XRNode.RightHand, this.rightHandDevice));
		}

		private IEnumerator GetLeftHandDevice(XRNode xrNode)
		{
			yield return StartCoroutine(SIGVerseUtils.GetXrDevice(xrNode, x => this.leftHandDevice = x));
		}

		private IEnumerator GetRightHandDevice(XRNode xrNode)
		{
			yield return StartCoroutine(SIGVerseUtils.GetXrDevice(xrNode, x => this.rightHandDevice = x));
		}


		private IEnumerator GetXrDevice(XRNode xrNode, InputDevice inputDevice)
		{
			yield return StartCoroutine(SIGVerseUtils.GetXrDevice(xrNode, x => inputDevice = x));
		}

		protected override void Update()
		{
			if (!IsOwner) { return; }

			base.Update();

			if (Time.time - this.lastMessageSentTime > MessageSendingInterval)
			{
				if(this.rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool rightPrimaryButton) && rightPrimaryButton)
				{
					this.PublishMessage(this.msgRightPrimaryButton);
				}
				if(this.rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightSecondaryButton) && rightSecondaryButton)
				{
					this.PublishMessage(this.msgRightSecondaryButton);
				}
				if(this.leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftPrimaryButton) && leftPrimaryButton)
				{
					this.PublishMessage(this.msgLeftPrimaryButton);
				}
				if(this.leftHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool leftSecondaryButton) && leftSecondaryButton)
				{
					this.PublishMessage(this.msgLeftSecondaryButton);
				}

				if(this.rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool rightPrimary2DAxisClick) && rightPrimary2DAxisClick)
				{
					this.PublishMessage(this.msgRightPrimary2DAxisClick);
				}
				if(this.leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool leftPrimary2DAxisClick) && leftPrimary2DAxisClick)
				{
					this.PublishMessage(this.msgLeftPrimary2DAxisClick);
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

