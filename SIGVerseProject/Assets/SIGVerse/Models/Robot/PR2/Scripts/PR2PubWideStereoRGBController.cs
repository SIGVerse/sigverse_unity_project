using UnityEngine;

using System;
using System.Collections;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;

namespace SIGVerse.PR2
{
	[RequireComponent(typeof (PR2PubSynchronizer))]

	public class PR2PubWideStereoRGBController : SIGVerseRosBridgePubMessage
	{
		[HeaderAttribute("Left Camera")]
		public PR2PubWideStereoRGB leftPublisher;
		public string topicNameLeftCameraInfo;
		public string topicNameLeftImage;

		[HeaderAttribute("Right Camera")]
		public PR2PubWideStereoRGB rightPublisher;
		public string topicNameRightCameraInfo;
		public string topicNameRightImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 250;

		//--------------------------------------------------

		private PR2PubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private float elapsedTime;


		void Awake()
		{
			this.synchronizer = this.GetComponent<PR2PubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();
		}

		protected override void Start()
		{
			base.Start();

			this.leftPublisher .Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameLeftCameraInfo,  this.topicNameLeftImage,  false, synchronizer.useThread);
			this.rightPublisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameRightCameraInfo, this.topicNameRightImage, true,  synchronizer.useThread);
		}

		void Update()
		{
			if(!this.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.leftPublisher.IsPublishing() || this.rightPublisher.IsPublishing() || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!this.synchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			this.leftPublisher .SendMessageInThisFrame();
			this.rightPublisher.SendMessageInThisFrame();
		}


		public override bool IsConnected()
		{
			return this.leftPublisher.IsConnected() && this.rightPublisher.IsConnected();
		}

		public override void Close()
		{
			this.leftPublisher .Close();
			this.rightPublisher.Close();
		}

		void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
