using System;
using System.IO;
using SIGVerse.Common;

namespace SIGVerse.SIGVerseRosBridge
{
	public class SIGVerseRosBridgeConnection
	{
		private const int SigverseRosbridgeConnectionTimeOut = 5000;

		private static bool canConnect = true;

		public static System.Net.Sockets.TcpClient GetConnection(string rosBridgeIP, int sigverseBridgePort)
		{
			if(!canConnect)
			{
				throw new Exception("Cannot connect HSR. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
			}

			System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();

			IAsyncResult connectResult = tcpClient.BeginConnect(rosBridgeIP, sigverseBridgePort, null, null);

			bool isConnected = connectResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(SigverseRosbridgeConnectionTimeOut));

			if (!isConnected)
			{
				canConnect = false;

				SIGVerseLogger.Error("Failed to connect. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
				throw new Exception("Failed to connect. IP="+rosBridgeIP + ", Port="+sigverseBridgePort);
			}

			return tcpClient;
		}
	}
}
