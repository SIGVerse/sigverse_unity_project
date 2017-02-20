using UnityEngine;

namespace SIGVerse.ROSBridge
{
	public abstract class ROSBridgeSubscriber
	{
		protected string topic;
		protected string type;

		public string Topic
		{
			get { return topic; }
		}

		public string Type
		{
			get { return type; }
		}

		public ROSBridgeSubscriber(string topicName)
		{
			this.topic = topicName;
		}

		public ROSBridgeSubscriber(string topicName, string typeName)
		{
			this.topic = topicName;
			this.type = typeName;
		}

		public abstract ROSMessage ParseMessage(string message);
	}

	public class ROSBridgeSubscriber<Tmsg> : ROSBridgeSubscriber where Tmsg : ROSMessage
	{
		public ROSBridgeSubscriber(string topicName) : base(topicName)
		{
		}

		public ROSBridgeSubscriber(string topicName, string typeName) : base(topicName, typeName)
		{
		}

		public override ROSMessage ParseMessage(string message)
		{
			return JsonUtility.FromJson<Tmsg>(message);
		}
	}
}

