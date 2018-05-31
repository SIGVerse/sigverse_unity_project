using UnityEngine;

using System.Collections;
using SIGVerse.SIGVerseRosBridge;


namespace SIGVerse.TurtleBot3
{
	[RequireComponent(typeof (TurtleBot3PubSynchronizer))]

	public class TurtleBot3PubSR300DepthController : SIGVerseRosBridgePubMessage
	{
		public TurtleBot3PubSR300Depth publisher;

		public string topicNameCameraInfo;
		public string topicNameImage;

		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		//--------------------------------------------------

		private TurtleBot3PubSynchronizer synchronizer;

		private int publishSequenceNumber;

		private float elapsedTime;


		void Awake()
		{
			this.synchronizer = this.GetComponent<TurtleBot3PubSynchronizer>();

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
