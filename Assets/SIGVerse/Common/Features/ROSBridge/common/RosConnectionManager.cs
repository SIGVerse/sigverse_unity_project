using UnityEngine;
using System;
using SIGVerse.Common;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SIGVerse.RosBridge
{
	public class RosConnectionManager : Singleton<RosConnectionManager>
	{
		protected RosConnectionManager() { } // guarantee this will be always a singleton only - can't use the constructor!

		public RosConnections rosConnections;

		void Awake()
		{
			this.rosConnections = new RosConnections();
		}
	}

	[System.Serializable]
	public class RosConnections
	{
		/// <summary>
		/// Dictionary of the rosbridge connections.
		/// There is no rules on how to use the key.
		/// </summary>
		public Dictionary<string, RosBridgeWebSocketConnection> rosBridgeWebSocketConnectionMap;

		/// <summary>
		/// Dictionary of the SIGVerse rosbridge connections.
		/// There is no rules on how to use the key.
		/// </summary>
		public Dictionary<string, TcpClient>  sigverseRosBridgeTcpClientMap;

		public RosConnections()
		{
			this.rosBridgeWebSocketConnectionMap = new Dictionary<string, RosBridgeWebSocketConnection>();

			this.sigverseRosBridgeTcpClientMap = new Dictionary<string, TcpClient>();
		}
	}
}

