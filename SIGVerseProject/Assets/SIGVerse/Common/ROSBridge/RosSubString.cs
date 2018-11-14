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
		public List<GameObject> destinations;

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.std_msgs.String rosMsg)
		{
			SIGVerseLogger.Info("Received string message :"+rosMsg.data);

			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<IRosReceivingStringMsgHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveRosStringMsg(rosMsg)
				);
			}
		}
	}
}
