using System;
using UnityEngine;
using SIGVerse.Common;
using SIGVerse.RosBridge;

namespace SIGVerse.SIGVerseRosBridge
{
	[System.Serializable]
	public class SIGVerseRosBridgeTimeSynchronizer : SIGVerseRosBridgePubMessage
	{
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public string topicName = "/sigverse/TimeSync";

		public bool shouldSynchronize = true;

		//--------------------------------------------------

		private System.Net.Sockets.TcpClient tcpClient = null;
		private System.Net.Sockets.NetworkStream networkStream = null;

		private SIGVerseRosBridgeMessage<SIGVerse.RosBridge.std_msgs.Time> timeSyncMsg = null;

		protected override void Start()
		{
			base.Start();

			if (!RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.ContainsKey(topicName))
			{
				this.tcpClient = SIGVerseRosBridgeConnection.GetConnection(this.rosBridgeIP, this.sigverseBridgePort);

				RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap.Add(topicName, this.tcpClient);
			}
			else
			{
				this.tcpClient = RosConnectionManager.Instance.rosConnections.sigverseRosBridgeTcpClientMap[topicName];
			}

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

					int numberOfBytesRead = 0;

					if(networkStream.CanRead)
					{
						numberOfBytesRead = networkStream.Read(byteArray, 0, byteArray.Length);
					}

					string message = System.Text.Encoding.UTF8.GetString(byteArray, 0, numberOfBytesRead);
					
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
				time.data.sec     = (int) epochTime.TotalSeconds;
				time.data.nanosec = (uint)epochTime.Milliseconds * 1000 * 1000;

				this.timeSyncMsg.msg = time;
			
				this.timeSyncMsg.SendMsg(this.networkStream);
			}
		}

		public override bool IsConnected()
		{
			return this.networkStream != null && this.tcpClient.Connected;
		}

		public override void Close()
		{
			if (this.networkStream != null) { this.networkStream.Close(); }
			if (this.tcpClient     != null) { this.tcpClient.Close(); }
		}

		void OnApplicationQuit()
		{
			this.Close();
		}
	}
}
