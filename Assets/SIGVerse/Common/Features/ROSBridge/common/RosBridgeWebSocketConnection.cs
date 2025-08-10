using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SIGVerse.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

namespace SIGVerse.RosBridge
{
	public delegate void RosMessageCallback<in Tmsg>(Tmsg msg);
	public delegate bool RosServiceCallback<in Targs, Tresp>(Targs args, out Tresp resp);
	
	delegate void MessageCallback(RosMessage msg);                                 // internal; used to wrap RosMessageCallbacks
	delegate bool ServiceCallback(ServiceArgs args, out ServiceResponse response); // internal; used to wrap RosServiceCallbacks

	[System.Serializable]
	public class Topper
	{
		public string op;
		public string topic;

		public Topper(string jsonString)
		{
			Topper message = JsonUtility.FromJson<Topper>(jsonString);
			this.op    = message.op;
			this.topic = message.topic;
		}
	}
	
	[System.Serializable]
	public class RosbridgeJson<Tmsg>
	{
		public string op;
		public string id;
		public string topic;
		public Tmsg msg;
		public string type;
		public string service;

		public RosbridgeJson(string jsonString)
		{
			RosbridgeJson<Tmsg> message = JsonUtility.FromJson<RosbridgeJson<Tmsg>>(jsonString);
			this.op      = message.op;
			this.id      = message.id;
			this.topic   = message.topic;
			this.msg     = message.msg;
			this.type    = message.type;
			this.service = message.service;
		}
	}


	public class RosBridgeWebSocketConnection
	{
		/// <summary>
		/// A queue with a limited maximum size. If an object added to the queue causes the queue
		/// to go over the specified maximum size, it will automatically dequeue the oldest entry.
		/// A maximum size of 0 implies an unrestricted queue (making this equivalent to Queue<T>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class RenderQueue<T> : Queue<T>
		{
			// Maximum size of queue. Unrestricted if zero.
			private uint maxSize = 0;

			public RenderQueue(uint maxSize)
			{
				this.maxSize = maxSize;
			}

			public new void Enqueue(T obj)
			{
				base.Enqueue(obj);

				if (base.Count > this.maxSize && this.maxSize > 0)
				{
					base.Dequeue();
				}
			}
		}

		private class MessageTask
		{
			private RosBridgeSubscriber subscriber;
			private RosMessage msg;

			public MessageTask(RosBridgeSubscriber subscriber, RosMessage msg)
			{
				this.subscriber = subscriber;
				this.msg = msg;
			}

			public RosBridgeSubscriber getSubscriber()
			{
				return this.subscriber;
			}

			public RosMessage getMsg()
			{
				return this.msg;
			}
		}

		private class ServiceTask
		{
			private RosBridgeServiceProvider service;
			private ServiceArgs serviceArgs;
			private ServiceResponse serviceResponse;
			private string id;

			public ServiceTask(RosBridgeServiceProvider service, ServiceArgs serviceArgs, string id)
			{
				this.service = service;
				this.serviceArgs = serviceArgs;
				this.id = id;
			}

			public RosBridgeServiceProvider Service
			{
				get { return this.service; }
			}

			public ServiceArgs ServiceArgs
			{
				get { return this.serviceArgs; }
			}

			public ServiceResponse Response
			{
				get { return this.serviceResponse; }
				set { this.serviceResponse = value; }
			}

			public string Id
			{
				get { return this.id; }
			}
		}

		// true: BSON, false: JSON
		public static readonly bool UseBson = false;

		private const int ConnectionTimeOut = 5000;

		private static HashSet<string> cannotConnectUrlList = new HashSet<string>();

		private static object cannotConnectLockObj = new object();

		private static void AddCannotConnectUrlList(string url)
		{
			lock(cannotConnectLockObj)
			{
				cannotConnectUrlList.Add(url);
			}
		}

		private static bool CanConnect(string url)
		{
			lock(cannotConnectLockObj)
			{
				return !cannotConnectUrlList.Contains(url);
			}
		}


		private string host;
		private int port;
		private WebSocket webSocket;
		private bool isConnected = false;

		private Dictionary<RosBridgeSubscriber, MessageCallback> subscribers;
		private List<RosBridgePublisher> publishers; 
		private Dictionary<RosBridgeServiceProvider, ServiceCallback> serviceProviders;

		private Dictionary<string, RenderQueue<MessageTask>> msgQueue = new Dictionary<string, RenderQueue<MessageTask>>();
		private Dictionary<string, RenderQueue<ServiceTask>> svcQueue = new Dictionary<string, RenderQueue<ServiceTask>>();

		private object lockMsgQueue;

		public bool IsConnected
		{
			get { return this.isConnected; }
		}

		public RosBridgeWebSocketConnection(string host, int port)
		{
			this.host = host;
			this.port = port;

			this.publishers = new List<RosBridgePublisher>();
			this.subscribers = new Dictionary<RosBridgeSubscriber, MessageCallback>();
			this.serviceProviders = new Dictionary<RosBridgeServiceProvider, ServiceCallback>();

			this.lockMsgQueue = new object();
		}

		/// <summary>
		/// Add a publisher to this connection. There can be many publishers.
		/// </summary>
		/// <typeparam name="Tpub">Publisher type to advertise</typeparam>
		/// <param name="topic">Topic to advertise on</param>
		/// <returns>A publisher which can be used to broadcast data on the given topic</returns>
		public RosBridgePublisher<Tmsg> Advertise<Tmsg>(string topic, uint queueSize = 0) where Tmsg : RosMessage
		{
			RosBridgePublisher<Tmsg> publisher = (RosBridgePublisher<Tmsg>)Activator.CreateInstance(typeof(RosBridgePublisher<Tmsg>), new object[] { topic, queueSize });
			publisher.SetConnection(this);
			publisher.CreatePublishingThread();

			this.publishers.Add(publisher);

			if (this.IsConnected)
			{
				RosBridgeMsg.SendAdvertiseTopic(this.webSocket, publisher.Topic, publisher.Type);
			}

			return publisher;
		}

		/// <summary>
		/// Remove a publisher from this connection
		/// </summary>
		/// <param name="publisher"></param>
		public void Unadvertise(RosBridgePublisher publisher)
		{
			if (this.IsConnected)
			{
				RosBridgeMsg.SendUnadvertiseTopic(this.webSocket, publisher.Topic);
			}

			publisher.Unadvertise();
			this.publishers.Remove(publisher);
		}

		/// <summary>
		/// Add a subscriber callback to this connection. There can be many subscribers.
		/// </summary>
		/// <typeparam name="Tmsg">Message type used in the callback</typeparam>
		/// <param name="sub">Subscriber</param>
		/// <param name="callback">Method to call when a message matching the given subscriber is received</param>
		public RosBridgeSubscriber<Tmsg> Subscribe<Tmsg>(string topic, RosMessageCallback<Tmsg> callback, uint queueSize = 0) where Tmsg : RosMessage, new()
		{
			MessageCallback MessageCallback = (RosMessage msg) =>
			{
				Tmsg message = msg as Tmsg;
				callback(message);
			};

			var getMessageType = typeof(Tmsg).GetMethod("GetMessageType");

			if (getMessageType == null)
			{
				Debug.LogError("Could not retrieve method GetMessageType() from " + typeof(Tmsg).ToString());
				return null;
			}
			string messageType = (string)getMessageType.Invoke(null, null);

			if (messageType == null)
			{
				Debug.LogError("Could not retrieve valid message type from " + typeof(Tmsg).ToString());
				return null;
			}

			RosBridgeSubscriber<Tmsg> subscriber = new RosBridgeSubscriber<Tmsg>(topic, messageType);

			this.subscribers.Add(subscriber, MessageCallback);
			this.msgQueue.Add(subscriber.Topic, new RenderQueue<MessageTask>(queueSize));

			if (this.IsConnected)
			{
				RosBridgeMsg.SendSubscribe(this.webSocket, subscriber.Topic, subscriber.Type);
			}

			return subscriber;
		}
		

		/// <summary>
		/// Remove a subscriber callback from this connection.
		/// </summary>
		/// <param name="subscriber"></param>
		public void Unsubscribe(RosBridgeSubscriber subscriber)
		{
			if (subscriber == null)
			{
				return;
			}

			this.subscribers.Remove(subscriber);
			this.msgQueue.Remove(subscriber.Topic);

			if (this.IsConnected)
			{
				RosBridgeMsg.SendUnsubscribe(this.webSocket, subscriber.Topic);
			}
		}

		/// <summary>
		/// Add a Service server to this connection. There can be many servers, but each service should only have one.
		/// </summary>
		/// <typeparam name="Tsrv">ServiceProvider type</typeparam>
		/// <typeparam name="Targ">Message type containing parameters for this service</typeparam>
		/// <typeparam name="Tres">Message type containing response data returned by this service</typeparam>
		/// <param name="srv">The service to advertise</param>
		/// <param name="callback">Method to invoke when the service is called</param>
		public RosBridgeServiceProvider<Targ> Advertise<Tsrv, Targ, Tres>(string service, RosServiceCallback<Targ, Tres> callback) where Tsrv : RosBridgeServiceProvider<Targ> where Targ : ServiceArgs where Tres : ServiceResponse, new()
		{
			ServiceCallback ServiceCallback = (ServiceArgs args, out ServiceResponse response) =>
			{
				Targ request = (Targ)args;
				Tres res = new Tres();
				bool success = callback(request, out res);
				response = res;
				return success;
			};

			Tsrv srv = (Tsrv)Activator.CreateInstance(typeof(Tsrv), new object[] { service } );
			this.serviceProviders.Add(srv, ServiceCallback);
			this.svcQueue.Add(srv.Name, new RenderQueue<ServiceTask>(0));

			if (this.IsConnected)
			{
				RosBridgeMsg.SendAdvertiseService(this.webSocket, srv.Name, srv.Type);
			}

			return srv;
		}

		/// <summary>
		/// Remove a Service server from this connection
		/// </summary>
		/// <param name="serviceProvider"></param>
		public void Unadvertise(RosBridgeServiceProvider serviceProvider)
		{
			if (this.IsConnected)
			{
				RosBridgeMsg.SendUnadvertiseService(this.webSocket, serviceProvider.Name);
			}

			this.serviceProviders.Remove(serviceProvider);
		}

		/// <summary>
		/// Connect to the remote ros environment.
		/// </summary>
		public void Connect<Tmsg>() where Tmsg : RosMessage
		{
			if (this.IsConnected)
			{
				return;
			}

			string url = "ws://" + this.host + ":" + this.port;

			if(!CanConnect(url))
			{
				throw new Exception("Cannot connect. url=" + url);
			}

			this.webSocket = new WebSocket(url);

			this.webSocket.OnOpen    += (sender, eventArgs) => { Debug.Log("WebSocket Open  url=" + url); };
			this.webSocket.OnMessage += (sender, eventArgs) =>
			{
				if (UseBson) { this.OnMessage<Tmsg>(eventArgs.RawData); }
				else         { this.OnMessage<Tmsg>(eventArgs.Data); }
			};
			this.webSocket.OnError   += (sender, eventArgs) => 
			{
				if (this.isConnected) { Debug.LogError("WebSocket Error Message: " + eventArgs.Message); }
				else { Debug.Log("WebSocket Error occurred when Not connected.: " + eventArgs.Message);}
			};
			this.webSocket.OnClose   += (sender, eventArgs) => this.OnClose();

//			this.webSocket.Connect();
			this.webSocket.ConnectAsync();

			DateTime startTime = DateTime.Now;

			while (this.webSocket.ReadyState != WebSocketState.Open)
			{
				if((DateTime.Now - startTime).TotalMilliseconds > ConnectionTimeOut)
				{
					AddCannotConnectUrlList(url);

					SIGVerseLogger.Error("Failed to connect. IP="+this.host + ", Port="+this.port + "  (Time out)");
					throw new Exception("Failed to connect. IP="+this.host + ", Port="+this.port + "  (Time out)");
				}

				Thread.Sleep(100);
			}

			if (!this.webSocket.IsAlive)
			{
				Debug.Log("Error: Connection was faild.");
			}
			else
			{
				Debug.Log("Connected to ROSbridge server");

				foreach (var sub in this.subscribers)
				{
					RosBridgeMsg.SendSubscribe(this.webSocket, sub.Key.Topic, sub.Key.Type);
				}
				foreach (RosBridgePublisher pub in this.publishers)
				{
					RosBridgeMsg.SendAdvertiseTopic(this.webSocket, pub.Topic, pub.Type);
				}
				foreach (var srv in this.serviceProviders)
				{
					RosBridgeMsg.SendAdvertiseService(this.webSocket, srv.Key.Name, srv.Key.Type);
				}

				this.isConnected = true;

				//foreach (RosBridgePublisher pub in this.publishers)
				//{
				//	pub.CreatePublishingThread();
				//}
			}
		}

		/// <summary>
		/// Disconnect from the remote ros environment.
		/// </summary>
		public void Disconnect()
		{
			if (!this.IsConnected) { return; }

			//this.sendMsgThread.Abort();

			this.isConnected = false;
			Thread.Sleep(15);

			foreach (var sub in this.subscribers)
			{
				RosBridgeMsg.SendUnsubscribe(this.webSocket, sub.Key.Topic);
			}
			foreach (var pub in this.publishers)
			{
				RosBridgeMsg.SendUnadvertiseTopic(this.webSocket, pub.Topic);
				pub.Unadvertise();
			}
			foreach (var srv in this.serviceProviders)
			{
				RosBridgeMsg.SendUnadvertiseService(this.webSocket, srv.Key.Name);
			}

			this.webSocket.Close();
			this.msgQueue.Clear();
		}

		private void OnMessage<Tmsg>(byte[] messageByte) where Tmsg : RosMessage
		{
			string message = null;

			using (var bsonStream = new MemoryStream(messageByte))
			using (var reader = new BsonDataReader(bsonStream))
			{
				var jsonSerializer = new JsonSerializer();
				var jsonString = JsonConvert.SerializeObject(jsonSerializer.Deserialize(reader));

				message = jsonString;
			}

			OnMessage<Tmsg>(message);
		}

		private void OnMessage<Tmsg>(string message) where Tmsg : RosMessage
		{
//			Debug.LogWarning("OnMessage="+message);

			if ((message != null) && !message.Equals(string.Empty))
			{
				Topper topper = new Topper(message);

				if ("publish".Equals(topper.op))  // Topic
				{
					RosbridgeJson<Tmsg> rosbridgeJson = null;

					foreach (var sub in this.subscribers)
					{
						// only consider subscribers with a matching topic
						if (topper.topic != sub.Key.Topic) { continue; }

						if(rosbridgeJson == null)
						{
							rosbridgeJson = new RosbridgeJson<Tmsg>(message);
						}

						MessageTask newTask = new MessageTask(sub.Key, rosbridgeJson.msg);

						lock (this.lockMsgQueue)
						{
							this.msgQueue[rosbridgeJson.topic].Enqueue(newTask);
						}
					}
				}
				else if ("call_service".Equals(topper.op)) // Service
				{
					RosbridgeJson<Tmsg> rosbridgeJson = new RosbridgeJson<Tmsg>(message);

					foreach (var srv in this.serviceProviders)
					{
						if (srv.Key.Name == rosbridgeJson.service)
						{
							ServiceArgs args = null;
//							ServiceResponse response = null;

							// if we have args, parse them (args are optional on services, though)
							Match match = Regex.Match(message, @"""args""\s*:\s*({.*}),");
							if (match.Success)
							{
								args = srv.Key.ParseRequest(match.Groups[1].Value);
							}

							// add service request to queue, to be processed later in Render()
							lock(this.lockMsgQueue)
							{
								this.svcQueue[srv.Key.Name].Enqueue(new ServiceTask(srv.Key, args, rosbridgeJson.id));
							}

							break; // there should only be one server for each service
						}
					}
				}
				else
				{
					Debug.LogWarning("Unhandled message:\n" + message);
				}
			}
			else
			{
				Debug.Log("Got an empty message from the web socket");
			}
		}

		public void OnClose()
		{
			Debug.Log("WebSocket Close");
			this.Disconnect();
		}

		/// <summary>
		/// Should be called at least once each frame. Calls any available callbacks for received messages.
		/// Note: MUST be called from Unity's main thread!
		/// </summary>
		public void Render()
		{
			float startTime   = Time.realtimeSinceStartup;  // time at start of this frame
			float maxDuration = 0.5f * Time.fixedDeltaTime; // max time we want to spend working
			float elapsedTime = 0.0f;                       // time spent so far processing messages

			while (elapsedTime < maxDuration)
			{
				// get queued work to do
				List<MessageTask> msgTasks = this.MessagePump();
				List<ServiceTask> svcTasks = this.ServicePump();

				// bail if we have no work to do
				if (msgTasks.Count == 0 && svcTasks.Count == 0)
				{
					break;
				}

				// call all msg subsriber callbacks
				foreach (var msgTask in msgTasks)
				{
					this.subscribers[msgTask.getSubscriber()](msgTask.getMsg());
				}

				// call all svc handlers
				foreach (var svcTask in svcTasks)
				{
					ServiceResponse response = null;

					// invoke service handler
					bool success =this.serviceProviders[svcTask.Service](svcTask.ServiceArgs, out response);

					// send response
					RosBridgeMsg.SendServiceResponse(this.webSocket, success, svcTask.Service.Name, svcTask.Id, JsonUtility.ToJson(response));
				}

				elapsedTime = Time.realtimeSinceStartup - startTime;
			}
		}

		/// <summary>
		/// Pulls one message from each queue for processing
		/// </summary>
		/// <returns>A list of queued messages</returns>
		private List<MessageTask> MessagePump()
		{
			List<MessageTask> tasks = new List<MessageTask>();

			lock (this.lockMsgQueue)
			{
				foreach (var msg in this.msgQueue)
				{
					// peel one entry from each queue to process on this frame
					if (msg.Value.Count > 0)
					{
						tasks.Add(msg.Value.Dequeue());
					}
				}
			}
			return tasks;
		}

		/// <summary>
		/// Pulls one message from each service queue for processing
		/// </summary>
		/// <returns>A list of queued service requests</returns>
		private List<ServiceTask> ServicePump()
		{
			List<ServiceTask> tasks = new List<ServiceTask>();

			lock (this.lockMsgQueue)
			{
				foreach (var svc in this.svcQueue)
				{
					// peel one entry from each queue to process on this frame
					if (svc.Value.Count > 0)
					{
						tasks.Add(svc.Value.Dequeue());
					}
				}
			}
			return tasks;
		}


		/// <summary>
		/// Publish a message to be sent to the ROS environment. Note: You must Advertise() before you can Publish().
		/// </summary>
		/// <param name="publisher">Publisher associated with the topic to publish to</param>
		/// <param name="msg">Message to publish</param>
		public void Publish(string msgStr)
		{
			if (this.webSocket != null && this.IsConnected)
			{
				RosBridgeMsg.Publish(this.webSocket, msgStr);
			}
			else
			{
				Debug.LogWarning("Could not publish message! No current connection to ROSBridge...");
			}
		}
	}
}

