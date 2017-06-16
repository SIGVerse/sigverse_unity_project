using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace SIGVerse.Common
{
	public class ConfigManager : Singleton<ConfigManager>
	{
		private const string FolderPath = "/../SIGVerseConfig/";
		private const string FileName = "SIGVerseConfig.json";

		private string configFolderPath;
		private string configFilePath;

		protected ConfigManager() { } // guarantee this will be always a singleton only - can't use the constructor!

		public ConfigInfo configInfo;

		void Awake()
		{
			this.configFolderPath = Application.dataPath + FolderPath;
			this.configFilePath = this.configFolderPath + FileName;

			this.configInfo = new ConfigInfo();

			if (!Directory.Exists(this.configFolderPath))
			{
				Directory.CreateDirectory(this.configFolderPath);
			}

			if (File.Exists(configFilePath))
			{
				Debug.Log("Config file exists.");

				// File open
				StreamReader srConfigReader = new StreamReader(configFilePath, Encoding.UTF8);

				this.configInfo = JsonUtility.FromJson<ConfigInfo>(srConfigReader.ReadToEnd());

				srConfigReader.Close();
			}
			else
			{
				Debug.Log("Config file not exists.");

				this.configInfo.rosIP = "192.168.1.101";
				this.configInfo.rosPort = "9090";
				this.configInfo.sigverseBridgePort = "50001";

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
			StreamWriter swConfigWriter = new StreamWriter(configFilePath, false, Encoding.UTF8);

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
		public string sigverseBridgePort;
	}
}

