using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;
using System;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubPressureSensorOn : RosPubMessage<SIGVerse.RosBridge.std_msgs.msg.Bool>
	{
		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 200;

		public SuctionDetector suctionDetector;

		//--------------------------------------------------
		private float elapsedTime = 0.0f;

		protected override void Update()
		{
			base.Update();

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			SIGVerse.RosBridge.std_msgs.msg.Bool pressureSensorOn =  new SIGVerse.RosBridge.std_msgs.msg.Bool();

			pressureSensorOn.data = suctionDetector.IsPressureSensorOn();

			this.publisher.Publish(pressureSensorOn);
		}
	}
}

