using UnityEngine;

namespace SIGVerse.RosBridge
{
	public abstract class RosBridgeSubscriber
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

		public RosBridgeSubscriber(string topicName)
		{
			this.topic = topicName;
		}

		public RosBridgeSubscriber(string topicName, string typeName)
		{
			this.topic = topicName;
			this.type = typeName;
		}

		public abstract RosMessage ParseMessage(string message);
	}

	public class RosBridgeSubscriber<Tmsg> : RosBridgeSubscriber where Tmsg : RosMessage
	{
		public RosBridgeSubscriber(string topicName) : base(topicName)
		{
		}

		public RosBridgeSubscriber(string topicName, string typeName) : base(topicName, typeName)
		{
		}

		public override RosMessage ParseMessage(string message)
		{
			return JsonUtility.FromJson<Tmsg>(message);
		}
	}
}

