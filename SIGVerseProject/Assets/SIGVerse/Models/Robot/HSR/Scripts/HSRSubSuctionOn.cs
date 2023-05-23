using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;


namespace SIGVerse.ToyotaHSR
{
	public class HSRSubSuctionOn : RosSubMessage<SIGVerse.RosBridge.std_msgs.Bool>
	{
		public SuctionDetector suctionDetector;

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.std_msgs.Bool suctionOn)
		{
			Debug.Log("Receive SuctionOn=" + suctionOn.data);

			if(suctionOn.data)
			{
				this.suctionDetector.StartSuction();
			}
			else
			{
				this.suctionDetector.StopSuction();
			}
		}
	}
}

