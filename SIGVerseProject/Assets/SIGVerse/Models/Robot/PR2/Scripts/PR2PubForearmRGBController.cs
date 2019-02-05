using UnityEngine;

using System;
using System.Collections;
using SIGVerse.Common;
using SIGVerse.SIGVerseRosBridge;

namespace SIGVerse.PR2
{
	[RequireComponent(typeof (PR2PubSynchronizer))]

	public class PR2PubForearmRGBController : SIGVerseRosBridgePubMessage
	{
		public PR2PubForearmRGB publisher;
		public string topicNameCameraInfo;
		public string topicNameImage;

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

			this.publisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameCameraInfo,  this.topicNameImage, synchronizer.useThread);
		}

		void Update()
		{
			if(!this.IsConnected()) { return; }

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.publisher.IsPublishing() || this.elapsedTime < this.sendingInterval * 0.001f)
			{
				return;
			}

			if(!this.synchronizer.CanExecute(this.publishSequenceNumber)) { return; }

			this.elapsedTime = 0.0f;

			this.publisher.SendMessageInThisFrame();
		}


		public override bool IsConnected()
		{
			return this.publisher.IsConnected();
		}

		public override void Close()
		{
			this.publisher.Close();
		}

		void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
