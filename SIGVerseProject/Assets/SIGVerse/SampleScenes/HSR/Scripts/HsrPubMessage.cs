using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;

namespace SIGVerse.SampleScenes.Hsr
{
	public interface IRosMsgSendHandler : IEventSystemHandler
	{
		void OnSendRosMessage(string message);
	}

	public class HsrPubMessage : RosPubMessage<SIGVerse.RosBridge.std_msgs.String>, IRosMsgSendHandler
	{
		public void OnSendRosMessage(string message)
		{
			SIGVerseLogger.Info("Sending message :" + message);

			SIGVerse.RosBridge.std_msgs.String rosMsg = new SIGVerse.RosBridge.std_msgs.String();
			rosMsg.data = message;

			this.publisher.Publish(rosMsg);
		}
	}
}

