using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SIGVerse.ROSBridge
{
	public abstract class ROSBridgePublisher
	{
		protected string topic;
		protected string type;
		protected ROSBridgeWebSocketConnection webSocketConnection;

		public string Topic
		{
			get { return topic; }
		}

		public string Type
		{
			get { return type; }
		}

		public ROSBridgePublisher(string topicName)
		{
			this.topic = topicName;
		}

		public void SetConnection(ROSBridgeWebSocketConnection webSocketConnection)
		{
			this.webSocketConnection = webSocketConnection;
		}

		public abstract string ToMessage(ROSMessage message);


		protected System.Threading.Thread publishingThread;
		protected Object lockPublishQueue;
		protected bool isPublishing;

		protected string publishMsg;


		public void CreatePublishingThread()
		{
			this.publishingThread = new Thread(RunPublishing);
			this.publishingThread.Start();
		}


		private void RunPublishing()
		{
			while (this.webSocketConnection.IsConnected)
			{
				lock (this.lockPublishQueue)
				{
					if (this.publishMsg != null)
					{
						this.isPublishing = true;
					}
				}

				if (this.isPublishing)
				{
					this.webSocketConnection.Publish(this.publishMsg);

					this.publishMsg = null;
					this.isPublishing = false;
				}
				else
				{
					Thread.Sleep(10);
				}
			}
		}



		/// <summary>
		/// Used to wrap ROSMessages for transmission over the network
		/// </summary>
		/// <typeparam name="T">Message type</typeparam>
		[System.Serializable]
		protected class ROSMessageWrapper<T>
		{
			public string op;
			public string topic;
			public T msg;

			public ROSMessageWrapper(string op, string topic, T data)
			{
				this.op = op;
				this.topic = topic;
				this.msg = data;
			}
		}
	}

	public class ROSBridgePublisher<Tmsg> : ROSBridgePublisher where Tmsg : ROSMessage
	{
		public ROSBridgePublisher(string topicName) : base(topicName)
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

			base.type = messageType;

			base.lockPublishQueue = new object();
			this.isPublishing = false;

			this.publishMsg = null;
		}

		public bool Publish(Tmsg message)
		{

			lock (this.lockPublishQueue)
			{
				if (this.isPublishing)
				{
					return false;
				}

				this.publishMsg = this.ToMessage(message);
			}

			return true;
		}

		public override string ToMessage(ROSMessage message)
		{
			var msg = new ROSMessageWrapper<Tmsg>("publish", topic, (Tmsg)message);

			return UnityEngine.JsonUtility.ToJson(msg);
		}
	}
}
