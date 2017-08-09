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

		protected ConfigManager() { } // guarantee this will be always a singleton only - can't use the constructor!

		public ConfigInfo configInfo;

		void Awake()
		{
			this.configInfo = InitConfigFile();
		}

		//// (optional) allow runtime registration of global objects
		//static public T RegisterComponent<T>() where T : Component
		//{
		//	return Instance.GetOrAddComponent<T>();
		//}

		public static ConfigInfo InitConfigFile()
		{
			string configFolderPath = Application.dataPath + FolderPath;
			string configFilePath   = configFolderPath + FileName;

			ConfigInfo configInfo = new ConfigInfo();

			if (!Directory.Exists(configFolderPath))
			{
				Directory.CreateDirectory(configFolderPath);
			}

			if (File.Exists(configFilePath))
			{
//				Debug.Log("Read the config file.");

				// File open
				StreamReader srConfigReader = new StreamReader(configFilePath);

				configInfo = JsonUtility.FromJson<ConfigInfo>(srConfigReader.ReadToEnd());

				srConfigReader.Close();
			}
			else
			{
				Debug.Log("Create the config file.");

				SaveConfig(configInfo);
			}

			return configInfo;
		}


		public static void SaveConfig(ConfigInfo configInfo)
		{
			string configFilePath   = Application.dataPath + FolderPath + FileName;

			StreamWriter swConfigWriter = new StreamWriter(configFilePath, false);

			Debug.Log("SaveConfig : " + JsonUtility.ToJson(configInfo));

			swConfigWriter.WriteLine(JsonUtility.ToJson(configInfo, true));

			swConfigWriter.Flush();
			swConfigWriter.Close();
		}

		public static void SaveConfig()
		{
			SaveConfig(ConfigManager.Instance.configInfo);
		}
	}

	[System.Serializable]
	public class ConfigInfo
	{
		private const string DefaultRosbridgeIP         = "192.168.1.101";
		private const int    DefaultRosbridgePort       = 9090;
		private const int    DefaultSigverseBridgePort  = 50001;
		private const bool   DefaultDisplaySigverseMenu = true;

		public string rosbridgeIP;
		public int    rosbridgePort;
		public int    sigverseBridgePort;

		public bool displaySigverseMenu;

		public ConfigInfo()
		{
			this.rosbridgeIP         = DefaultRosbridgeIP;
			this.rosbridgePort       = DefaultRosbridgePort;
			this.sigverseBridgePort  = DefaultSigverseBridgePort;
			this.displaySigverseMenu = DefaultDisplaySigverseMenu;
		}
	}
}

