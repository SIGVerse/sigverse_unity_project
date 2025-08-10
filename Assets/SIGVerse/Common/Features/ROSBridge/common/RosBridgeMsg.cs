using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using static SIGVerse.RosBridge.RosBridgeWebSocketConnection;

namespace SIGVerse.RosBridge
{
	public class RosBridgeMsg
	{
		private static byte[] ConvertToBytes(string msg)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BsonDataWriter writer = new BsonDataWriter(memoryStream))
				{
					JsonSerializer serializer = new JsonSerializer();
					serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
					serializer.Serialize(writer, JObject.Parse(msg));

					return memoryStream.ToArray();
				}
			}
		}

		private static T GetRosbridgeMessage<T>(string jsonMsg)
		{
			if (typeof(T) == typeof(string))
			{
				return (T)((object)jsonMsg);
			}
			else if (typeof(T) == typeof(byte[]))
			{
				return (T)((object)ConvertToBytes(jsonMsg));
			}
			else
			{
				throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");
			}
		}

		private static T AdvertiseTopic<T>(string topic, string type)
		{
			string jsonMsg = @"{""op"" : ""advertise"", ""topic"": """ + topic + @""", ""type"": """ + type + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendAdvertiseTopic(WebSocket webSocket, string topic, string type)
		{
			Debug.Log("Sending " + AdvertiseTopic<string>(topic, type));

			if (UseBson){ webSocket.Send(AdvertiseTopic<byte[]>(topic, type)); }
			else        { webSocket.Send(AdvertiseTopic<string>(topic, type)); }
		}

		private static T UnadvertiseTopic<T>(string topic)
		{
			string jsonMsg = @"{""op"" : ""unadvertise"", ""topic"": """ + topic + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendUnadvertiseTopic(WebSocket webSocket, string topic)
		{
			Debug.Log("Sending " + UnadvertiseTopic<string>(topic));

			if (UseBson){ webSocket.Send(UnadvertiseTopic<byte[]>(topic)); }
			else        { webSocket.Send(UnadvertiseTopic<string>(topic)); }
		}
		
		private static T AdvertiseService<T>(string service, string type)
		{
			string jsonMsg = @"{""op"" : ""advertise_service"", ""type"" : """ + type + @""", ""service"" : """ + service + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendAdvertiseService(WebSocket webSocket, string service, string type)
		{
			Debug.Log("Sending " + AdvertiseService<string>(service, type));

			if (UseBson){ webSocket.Send(AdvertiseService<byte[]>(service, type)); }
			else        { webSocket.Send(AdvertiseService<string>(service, type)); }
		}

		private static T UnadvertiseService<T>(string service)
		{
			string jsonMsg = @"{""op"" : ""unadvertise_service"", ""service"" : """ + service + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendUnadvertiseService(WebSocket webSocket, string service)
		{
			Debug.Log("Sending " + UnadvertiseService<string>(service));

			if (UseBson){ webSocket.Send(UnadvertiseService<byte[]>(service)); }
			else        { webSocket.Send(UnadvertiseService<string>(service)); }
		}

		private static T ServiceResponse<T>(bool success, string service, string id, string resultValues)
		{
			string jsonMsg = @"{""op"": ""service_response"", ""id"": """ + id + @""", ""service"": """ + service + @""", ""values"": " + resultValues + @", ""result"": " + success.ToString().ToLower() + @"}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendServiceResponse(WebSocket webSocket, bool success, string service, string id, string resultValues)
		{
			Debug.Log("Sending " + ServiceResponse<string>(success, service, id, resultValues));

			if (UseBson){ webSocket.Send(ServiceResponse<byte[]>(success, service, id, resultValues)); }
			else        { webSocket.Send(ServiceResponse<string>(success, service, id, resultValues)); }
		}

		private static T Subscribe<T>(string topic)
		{
			string jsonMsg = @"{""op"" : ""subscribe"", ""topic"": """ + topic + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendSubscribe(WebSocket webSocket, string topic)
		{
			Debug.Log("Sending " + Subscribe<string>(topic));

			if (UseBson){ webSocket.Send(Subscribe<byte[]>(topic)); }
			else        { webSocket.Send(Subscribe<string>(topic)); }
		}

		private static T Subscribe<T>(string topic, string type)
		{
			string jsonMsg = @"{""op"" : ""subscribe"", ""topic"": """ + topic + @""", ""type"": """ + type + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendSubscribe(WebSocket webSocket, string topic, string type)
		{
			Debug.Log("Sending " + Subscribe<string>(topic, type));

			if (UseBson){ webSocket.Send(Subscribe<byte[]>(topic, type)); }
			else        { webSocket.Send(Subscribe<string>(topic, type)); }
		}

		private static T Unsubscribe<T>(string topic)
		{
			string jsonMsg = @"{""op"" : ""unsubscribe"", ""topic"": """ + topic + @"""}";
			return GetRosbridgeMessage<T>(jsonMsg);
		}

		public static void SendUnsubscribe(WebSocket webSocket, string topic)
		{
			Debug.Log("Sending " + Unsubscribe<string>(topic));

			if (UseBson){ webSocket.Send(Unsubscribe<byte[]>(topic)); }
			else        { webSocket.Send(Unsubscribe<string>(topic)); }
		}

		public static void Publish(WebSocket webSocket, string message)
		{
//			Debug.Log("Publish " + message);

			if (UseBson){ webSocket.Send(ConvertToBytes(message)); }
			else        { webSocket.Send(message); }
		}
	}
}
