using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace SIGVerse.SIGVerseROSBridge
{
	[System.Serializable]
	public class SIGVerseROSBridgeMessage<ROSMessage>
	{
		public string op { get; set; }
		public string topic { get; set; }
		public string type { get; set; }
		public ROSMessage msg { get; set; }

		public SIGVerseROSBridgeMessage(string op, string topic, string type, ROSMessage msg)
		{
			this.op = op;
			this.topic = topic;
			this.type = type;
			this.msg = msg;
		}


		public void sendMsg(NetworkStream networkStream)
		{
			MemoryStream memoryStream = new MemoryStream();
			BsonWriter writer = new BsonWriter(memoryStream);
			JsonSerializer serializer = new JsonSerializer();
			serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			serializer.Serialize(writer, this);

			byte[] msgBinary = memoryStream.ToArray();

			// System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			// sw.Start();

			networkStream.Write(msgBinary, 0, msgBinary.Length);

			// Receive the time gap between Unity and ROS
			if(networkStream.DataAvailable)
			{
				byte[] byteArray = new byte[256];
				networkStream.Read(byteArray, 0, byteArray.Length);
				string message = System.Text.Encoding.UTF8.GetString(byteArray);
				string[] messageArray  = message.Split(',');

				if (messageArray.Length==3)
				{
					UnityEngine.Debug.Log("Time gap sec="+messageArray[1]+", msec="+ messageArray[2]);

					SIGVerse.ROSBridge.std_msgs.Header.SetTimeGap(Int32.Parse(messageArray[1]), Int32.Parse(messageArray[2]));
				}
				else
				{
					UnityEngine.Debug.LogError("Illegal message. Time gap message="+message);
				}
			}
			// sw.Stop();

			// UnityEngine.Debug.Log("time="+sw.Elapsed+", size="+ msgBinary.Length);
		}
	}
}
