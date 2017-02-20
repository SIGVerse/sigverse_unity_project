using UnityEngine;
using System.Collections.Generic;
using System;

namespace SIGVerse.Common
{
	[Serializable]
	public class ApplicationConfigs : ScriptableObject
	{
		public SIGVerseConfig sigverseConfig;
		public UserConfig userConfig;
	}

	[Serializable]
	public class SIGVerseConfig
	{
		public string version = "";
		public string rosIP = "";
		public string rosPort = "";
	}

	[Serializable]
	public class UserConfig
	{
		public string key1 = "";
		public string key2 = "";
		public string key3 = "";
	}
}
