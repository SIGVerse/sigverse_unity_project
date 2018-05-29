using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.SIGVerseRosBridge
{
	[System.Serializable]
	public class SIGVerseRosBridgeTimeSynchronizer : MonoBehaviour
	{
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public string rosBridgeIP;
		public int    sigverseBridgePort;

		public string topicName = "/sigverse/TimeSync";

		public bool shouldSynchronize = true;

		//--------------------------------------------------

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		private SIGVerseRosBridgeMessage<SIGVerse.RosBridge.std_msgs.Time> timeSyncMsg = null;

		private void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP        = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.sigverseBridgePort == 0)
			{
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}

			this.tcpClient = new System.Net.Sockets.TcpClient(rosBridgeIP, sigverseBridgePort);

			this.networkStream = this.tcpClient.GetStream();

			this.networkStream.ReadTimeout  = 100000;
			this.networkStream.WriteTimeout = 100000;

			this.timeSyncMsg = new SIGVerseRosBridgeMessage<SIGVerse.RosBridge.std_msgs.Time>("publish", this.topicName, "sigverse/TimeSync", null);
		}


		void FixedUpdate()
		{
			try
			{
				// Receive the time gap between Unity and ROS
				if(this.networkStream.DataAvailable)
				{
					byte[] byteArray = new byte[256];

					if(networkStream.CanRead)
					{
						networkStream.Read(byteArray, 0, byteArray.Length);
					}

					string message = System.Text.Encoding.UTF8.GetString(byteArray);
					string[] messageArray  = message.Split(',');

					if (messageArray.Length==3)
					{
						SIGVerseLogger.Info("Time gap sec="+messageArray[1]+", msec="+ messageArray[2]);

						SIGVerse.RosBridge.std_msgs.Header.SetTimeGap(Int32.Parse(messageArray[1]), Int32.Parse(messageArray[2]));
					}
					else
					{
						SIGVerseLogger.Error("Illegal message. Time gap message="+message);
					}
				}
			}
			catch (ObjectDisposedException exception)
			{
				SIGVerseLogger.Warn(exception.Message);
			}
		}

		void Update()
		{
			if(this.shouldSynchronize)
			{
				this.shouldSynchronize = false;

				/*
				 * Send requesting time gap message
				 */
				TimeSpan epochTime = (DateTime.Now.ToUniversalTime() - UnixEpoch);

				SIGVerse.RosBridge.std_msgs.Time time = new RosBridge.std_msgs.Time();
				time.data.secs  = (int)epochTime.TotalSeconds;
				time.data.nsecs = epochTime.Milliseconds * 1000 * 1000;

				this.timeSyncMsg.msg = time;
			
				this.timeSyncMsg.SendMsg(this.networkStream);
			}
		}
	}
}
