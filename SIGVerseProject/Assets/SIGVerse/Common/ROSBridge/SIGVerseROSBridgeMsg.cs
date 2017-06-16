using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

using SIGVerse.ROSBridge.sensor_msgs;

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

			// sw.Stop();

			// UnityEngine.Debug.Log("time="+sw.Elapsed+", size="+ msgBinary.Length);
		}
	}
}
