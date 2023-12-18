using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;
using System.Collections.Generic;

namespace SIGVerse.RosBridge
{
	public interface IRosReceivingStringMsgHandler : IEventSystemHandler
	{
		void OnReceiveRosStringMsg(SIGVerse.RosBridge.std_msgs.String rosMsg);
	}

	public class RosSubString : RosSubMessage<SIGVerse.RosBridge.std_msgs.String>
	{
		public List<GameObject> forwardingDestinations;

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.std_msgs.String rosMsg)
		{
			SIGVerseLogger.Info("Received string message :"+rosMsg.data);

			foreach(GameObject forwardingDestination in this.forwardingDestinations)
			{
				ExecuteEvents.Execute<IRosReceivingStringMsgHandler>
				(
					target: forwardingDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveRosStringMsg(rosMsg)
				);
			}
		}
	}
}
