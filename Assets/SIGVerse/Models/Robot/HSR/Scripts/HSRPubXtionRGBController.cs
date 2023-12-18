using UnityEngine;

using System;
using System.Collections;
using SIGVerse.SIGVerseRosBridge;

namespace SIGVerse.ToyotaHSR
{
	[RequireComponent(typeof (HSRPubSynchronizer))]

	public class HSRPubXtionRGBController : SIGVerseRosBridgePubMessage
	{
		public HSRPubXtionRGB publisher;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		private HSRPubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private float elapsedTime;


		void Awake()
		{
			this.synchronizer = this.GetComponent<HSRPubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();
		}

		protected override void Start()
		{
			base.Start();

			this.publisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameCameraInfo, this.topicNameImage, synchronizer.useThread);
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
