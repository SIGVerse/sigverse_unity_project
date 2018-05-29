using UnityEngine;
using SIGVerse.Common;
using SIGVerse.RosBridge;

namespace SIGVerse.SIGVerseRosBridge
{
	abstract public class SIGVerseRosBridgePubMessage : MonoBehaviour, IRosConnection
	{
		public string rosBridgeIP;
		public int    sigverseBridgePort;

		//--------------------------------------------------

		protected virtual void Start()
		{
			if (this.rosBridgeIP.Equals(string.Empty))
			{
				this.rosBridgeIP        = ConfigManager.Instance.configInfo.rosbridgeIP;
			}
			if (this.sigverseBridgePort == 0)
			{
				this.sigverseBridgePort = ConfigManager.Instance.configInfo.sigverseBridgePort;
			}
		}

		//protected virtual void OnDestroy()
		//{
		//}

		//protected virtual void Update()
		//{
		//}

		public abstract bool IsConnected();

		public virtual void Clear()
		{
		}

		public abstract void Close();
	}
}

