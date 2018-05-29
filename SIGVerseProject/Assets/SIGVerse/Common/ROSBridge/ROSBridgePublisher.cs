using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SIGVerse.RosBridge
{
	public abstract class RosBridgePublisher
	{
		private static readonly int SendingInterval = 3; //[ms]

		protected string topic;
		protected string type;
		protected RosBridgeWebSocketConnection webSocketConnection;

		protected System.Threading.Thread publishingThread;
		protected Object lockPublishQueue;

		protected Queue<string> publishMsgQue;
		protected uint queueSize;

		protected bool isRunning;


		public string Topic
		{
			get { return topic; }
		}

		public string Type
		{
			get { return type; }
		}

		public RosBridgePublisher(string topicName, uint queueSize)
		{
			this.topic     = topicName;
			this.queueSize = queueSize;

			this.publishMsgQue = new Queue<string>();
			this.lockPublishQueue = new object();

			this.isRunning = true;
		}

		public void SetConnection(RosBridgeWebSocketConnection webSocketConnection)
		{
			this.webSocketConnection = webSocketConnection;
		}

		public abstract string ToMessage(RosMessage message);


		public void CreatePublishingThread()
		{
			this.publishingThread = new Thread(RunPublishing);
			this.publishingThread.Start();
		}

		public void Unadvertise()
		{
			this.isRunning = false;
		}


		private void RunPublishing()
		{
			string message;

			while (this.isRunning)
			{
				if(!this.webSocketConnection.IsConnected){ continue; }

				while(this.publishMsgQue.Count > 0)
				{
					lock (this.lockPublishQueue)
					{
						message = this.publishMsgQue.Dequeue();
					}

					this.webSocketConnection.Publish(message);
				}
				Thread.Sleep(SendingInterval);
			}
		}


		/// <summary>
		/// Used to wrap ROSMessages for transmission over the network
		/// </summary>
		/// <typeparam name="T">Message type</typeparam>
		[System.Serializable]
		protected class RosMessageWrapper<T>
		{
			public string op;
			public string topic;
			public T msg;

			public RosMessageWrapper(string op, string topic, T data)
			{
				this.op = op;
				this.topic = topic;
				this.msg = data;
			}
		}
	}

	public class RosBridgePublisher<Tmsg> : RosBridgePublisher where Tmsg : RosMessage
	{
		public RosBridgePublisher(string topicName, uint queueSize = 0) : base(topicName, queueSize)
		{
			var getMessageType = typeof(Tmsg).GetMethod("GetMessageType");

			if (getMessageType == null)
			{
				UnityEngine.Debug.LogError("Could not retrieve method GetMessageType() from " + typeof(Tmsg).ToString());
				return;
			}

			string messageType = (string)getMessageType.Invoke(null, null);

			if (messageType == null)
			{
				UnityEngine.Debug.LogError("Could not retrieve valid message type from " + typeof(Tmsg).ToString());
				return;
			}

			this.type = messageType;

//			UnityEngine.Debug.LogError("rosbridge publisher("+topicName+") queue size="+queueSize);
		}

		public bool Publish(Tmsg message)
		{
			lock (this.lockPublishQueue)
			{
				this.publishMsgQue.Enqueue(this.ToMessage(message));

				if (this.publishMsgQue.Count > this.queueSize && this.queueSize > 0)
				{
					this.publishMsgQue.Dequeue();
				}
			}

			return true;
		}

		public override string ToMessage(RosMessage message)
		{
			var msg = new RosMessageWrapper<Tmsg>("publish", topic, (Tmsg)message);

			return UnityEngine.JsonUtility.ToJson(msg);
		}
	}
}
