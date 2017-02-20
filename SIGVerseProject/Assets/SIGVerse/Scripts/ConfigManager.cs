using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace SIGVerse.Common
{
	public class ConfigManager : Singleton<ConfigManager>
	{
		private static readonly string ConfigFilePath = "SIGVerseConfig.json";

		protected ConfigManager() { } // guarantee this will be always a singleton only - can't use the constructor!

		public ConfigInfo configInfo;

		void Awake()
		{
			this.configInfo = new ConfigInfo();

			if (File.Exists(ConfigFilePath))
			{
				Debug.Log("Config file exists.");

				// File open
				StreamReader srConfigReader = new StreamReader(ConfigFilePath, Encoding.UTF8);

				this.configInfo = JsonUtility.FromJson<ConfigInfo>(srConfigReader.ReadToEnd());

				srConfigReader.Close();
			}
			else
			{
				Debug.Log("Config file not exists.");

				this.configInfo.rosIP = "";
				this.configInfo.rosPort = "";

				this.SaveConfig();
			}
		}

		//// (optional) allow runtime registration of global objects
		//static public T RegisterComponent<T>() where T : Component
		//{
		//	return Instance.GetOrAddComponent<T>();
		//}

		public void SaveConfig()
		{
			StreamWriter swConfigWriter = new StreamWriter(ConfigFilePath, false, Encoding.UTF8);

			Debug.Log("SaveConfig : " + JsonUtility.ToJson(ConfigManager.Instance.configInfo));

			swConfigWriter.WriteLine(JsonUtility.ToJson(ConfigManager.Instance.configInfo, true));

			swConfigWriter.Flush();
			swConfigWriter.Close();
		}
	}

	[System.Serializable]
	public class ConfigInfo
	{
		public string rosIP;
		public string rosPort;
	}
}

