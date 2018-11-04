using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;
using System.Collections.Generic;

namespace SIGVerse.SampleScenes.Hsr
{
	public interface IRosMsgReceiveHandler : IEventSystemHandler
	{
		void OnReceiveRosMessage(SIGVerse.RosBridge.std_msgs.String rosMsg);
	}

	public class HsrSubMessage : RosSubMessage<SIGVerse.RosBridge.std_msgs.String>
	{
		public List<GameObject> destinations;

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.std_msgs.String rosMsg)
		{
			SIGVerseLogger.Info("Received message :"+rosMsg.data);

			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<IRosMsgReceiveHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveRosMessage(rosMsg)
				);
			}
		}
	}
}
