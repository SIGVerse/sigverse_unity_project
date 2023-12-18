using UnityEngine;

using System;
using System.Collections;
using SIGVerse.SIGVerseRosBridge;

namespace SIGVerse.Common
{
	[RequireComponent(typeof (RobotPubSynchronizer))]
	public class RobotPubCameraImageController : SIGVerseRosBridgePubMessage
	{
		public RobotPubCameraImage publisher;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		protected RobotPubSynchronizer synchronizer;

		protected int publishSequenceNumber;

		protected float elapsedTime;


		protected virtual void Awake()
		{
			this.synchronizer = this.GetComponent<RobotPubSynchronizer>();

			this.publishSequenceNumber = this.synchronizer.GetAssignedSequenceNumber();
		}

		protected override void Start()
		{
			base.Start();

			this.publisher.Initialize(this.rosBridgeIP, this.sigverseBridgePort, this.topicNameCameraInfo, this.topicNameImage, synchronizer.useThread);
		}

		protected virtual void Update()
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

		protected virtual void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
