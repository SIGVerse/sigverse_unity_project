using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.RosBridge
{
	public interface IRosSendingStringMsgHandler : IEventSystemHandler
	{
		void OnSendRosStringMsg(string message);
	}

	public class RosPubString : RosPubMessage<SIGVerse.RosBridge.std_msgs.msg.String>, IRosSendingStringMsgHandler
	{
		public void OnSendRosStringMsg(string message)
		{
			SIGVerseLogger.Info("Sending string message :" + message);

			SIGVerse.RosBridge.std_msgs.msg.String rosMsg = new SIGVerse.RosBridge.std_msgs.msg.String();
			rosMsg.data = message;

			this.publisher.Publish(rosMsg);
		}
	}
}

