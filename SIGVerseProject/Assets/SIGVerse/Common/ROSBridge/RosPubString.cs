using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;

namespace SIGVerse.RosBridge
{
	public interface IRosSendingStringMsgHandler : IEventSystemHandler
	{
		void OnSendRosStringMsg(string message);
	}

	public class RosPubString : RosPubMessage<SIGVerse.RosBridge.std_msgs.String>, IRosSendingStringMsgHandler
	{
		public void OnSendRosStringMsg(string message)
		{
			SIGVerseLogger.Info("Sending string message :" + message);

			SIGVerse.RosBridge.std_msgs.String rosMsg = new SIGVerse.RosBridge.std_msgs.String();
			rosMsg.data = message;

			this.publisher.Publish(rosMsg);
		}
	}
}

