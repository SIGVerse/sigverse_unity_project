using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;
using UnityEditor.Build;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class SIGVerseSettings : EditorWindow
	{
		private const char   SymbolSeparator = ';';
		private const string SIGVerseScriptingDefineSymbolsKey = "sigverse_scripting_define_symbols";

		private const string DefineSIGVerseMySQL   = "SIGVERSE_MYSQL";
		private const string DefineSIGVerseSteamVR = "SIGVERSE_STEAMVR";
		private const string DefineSIGVersePun     = "SIGVERSE_PUN";

		private const string WindowName   = "SIGVerse";
		private const string MenuItemName = "SIGVerse/SIGVerse Settings"; 

		private static readonly NamedBuildTarget[] NamedBuildTargets = new NamedBuildTarget[]
		{
			NamedBuildTarget.Standalone,
			NamedBuildTarget.iOS,
			NamedBuildTarget.Android,
			NamedBuildTarget.WebGL
		};

		private Texture2D conceptTexture;
		private Rect headerRect;

		private string rosbridgeIP;
		private int    rosbridgePort;
		private int    sigverseBridgePort;
		private string logFileName;
		private bool   useSigverseMenu;
		private bool   isAutoStartWithMenu;
		private bool   setUpRosTimestamp;

		private bool isUsingMySQL;
		private bool isUsingSteamVR;
		private bool isUsingPun;


		void OnEnable ()
		{
			this.conceptTexture = (Texture2D)Resources.Load(SIGVerseUtils.ConceptImageResourcePath);

			this.headerRect = new Rect(0, 0, 720, 100);

			ConfigInfo configInfo = ConfigManager.InitConfigFile();

			this.rosbridgeIP         = configInfo.rosbridgeIP;
			this.rosbridgePort       = configInfo.rosbridgePort;
			this.sigverseBridgePort  = configInfo.sigverseBridgePort;
			this.logFileName         = configInfo.logFileName;
			this.useSigverseMenu     = configInfo.useSigverseMenu;
			this.isAutoStartWithMenu = configInfo.isAutoStartWithMenu;
			this.setUpRosTimestamp   = configInfo.setUpRosTimestamp;

			// Get the define symbol settings from EditorUserSettings
			this.GetDefineSymbolSettings();
		}

		private void GetDefineSymbolSettings()
		{
			// Get from EditorUserSettings
			string defineSymbolsStr = EditorUserSettings.GetConfigValue(SIGVerseScriptingDefineSymbolsKey);

			if(defineSymbolsStr==null)
			{
				defineSymbolsStr = string.Empty;

				EditorUserSettings.SetConfigValue(SIGVerseScriptingDefineSymbolsKey, defineSymbolsStr);
			}

			string[] defineSymbols = defineSymbolsStr.Split(SymbolSeparator);

			this.isUsingMySQL   = Array.IndexOf(defineSymbols, DefineSIGVerseMySQL)   >= 0;
			this.isUsingSteamVR = Array.IndexOf(defineSymbols, DefineSIGVerseSteamVR) >= 0;
			this.isUsingPun     = Array.IndexOf(defineSymbols, DefineSIGVersePun)     >= 0;
		}


		[MenuItem(MenuItemName)]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(SIGVerseSettings), false, WindowName);
		}
	

		void OnGUI()
		{
			EditorGUI.DrawPreviewTexture(this.headerRect, this.conceptTexture);

			GUILayout.Space(this.headerRect.height + 10);

			// Config file Settings
			GUILayout.Label("Config file Settings", EditorStyles.boldLabel);

			EditorGUI.indentLevel++;

			EditorGUIUtility.labelWidth = 240;

			EditorGUI.BeginChangeCheck();

			this.rosbridgeIP         = EditorGUILayout.TextField("Rosbridge IP",                       this.rosbridgeIP,        GUILayout.Width(EditorGUIUtility.labelWidth + 120));
			this.rosbridgePort       = EditorGUILayout.IntField ("Rosbridge Port",                     this.rosbridgePort,      GUILayout.Width(EditorGUIUtility.labelWidth + 80));
			this.sigverseBridgePort  = EditorGUILayout.IntField ("SIGVerse Bridge Port",               this.sigverseBridgePort, GUILayout.Width(EditorGUIUtility.labelWidth + 80));
			this.logFileName         = EditorGUILayout.TextField("Log File Name",                      this.logFileName,        GUILayout.Width(EditorGUIUtility.labelWidth + 300));
			this.useSigverseMenu     = EditorGUILayout.Toggle   ("Use SIGVerse menu",                  this.useSigverseMenu);
			this.isAutoStartWithMenu = EditorGUILayout.Toggle   ("     (option)  Auto Start",          this.isAutoStartWithMenu);
			this.setUpRosTimestamp   = EditorGUILayout.Toggle   ("Set up Time stamps of ROS message",  this.setUpRosTimestamp);

			if (EditorGUI.EndChangeCheck())
			{
				ConfigInfo configInfo = new ConfigInfo();

				configInfo.rosbridgeIP         = this.rosbridgeIP;
				configInfo.rosbridgePort       = this.rosbridgePort;
				configInfo.sigverseBridgePort  = this.sigverseBridgePort;
				configInfo.logFileName         = this.logFileName;
				configInfo.useSigverseMenu     = this.useSigverseMenu;
				configInfo.isAutoStartWithMenu = this.isAutoStartWithMenu;
				configInfo.setUpRosTimestamp   = this.setUpRosTimestamp;

				ConfigManager.InitConfigFile(); // Create config file
				ConfigManager.SaveConfig(configInfo);
			}

			GUILayout.Space(10);
			GUILayout.Box("", GUILayout.Width(this.position.width), GUILayout.Height(2));


			// Scripting Define Symbols Settings
			GUILayout.Label("Define symbols Settings", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();
			{
				this.isUsingMySQL = EditorGUILayout.Toggle("Use MySQL", this.isUsingMySQL);
				GUILayout.Space(20);
				GUILayout.Label("* Please import MySQL library (MySql.Data.dll)");
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				this.isUsingSteamVR = EditorGUILayout.Toggle("Use SteamVR", this.isUsingSteamVR);
				GUILayout.Space(20);
				GUILayout.Label("* Please import SteamVR Plugin");
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				this.isUsingPun = EditorGUILayout.Toggle("Use PUN", this.isUsingPun);
				GUILayout.Space(20);
				GUILayout.Label("* Please import Photon Unity Networking libraries");
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				foreach (NamedBuildTarget namedBuildTarget in NamedBuildTargets)
				{
					string[] scriptingDefineSymbols  = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(SymbolSeparator);

					List<string> scriptingDefineSymbolList = new List<string>(scriptingDefineSymbols);

					// Add/Remove MySQL define
					this.UpdateScriptingDefineSymbolList(ref scriptingDefineSymbolList, this.isUsingMySQL, DefineSIGVerseMySQL);

					// Add/Remove SteamVR define
					this.UpdateScriptingDefineSymbolList(ref scriptingDefineSymbolList, this.isUsingSteamVR, DefineSIGVerseSteamVR);

					// Add/Remove PUN define
					this.UpdateScriptingDefineSymbolList(ref scriptingDefineSymbolList, this.isUsingPun, DefineSIGVersePun);

					string defineSymbolsStr = String.Join(SymbolSeparator.ToString(), scriptingDefineSymbolList.ToArray());

					// Update ScriptingDefineSymbols of PlayerSettings
					PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineSymbolsStr);

					// Update SIGVerseScriptingDefineSymbols of EditorUserSettings
					EditorUserSettings.SetConfigValue(SIGVerseScriptingDefineSymbolsKey, defineSymbolsStr);
				}
			}

			GUILayout.Space(10);
			GUILayout.Box("", GUILayout.Width(this.position.width), GUILayout.Height(2));
		}

		void UpdateScriptingDefineSymbolList(ref List<string> scriptingDefineSymbolList, bool isUsingDefine, string defineStr)
		{
			if(isUsingDefine && !scriptingDefineSymbolList.Contains(defineStr))
			{
				scriptingDefineSymbolList.Add(defineStr);
			}
			if(!isUsingDefine && scriptingDefineSymbolList.Contains(defineStr))
			{
				scriptingDefineSymbolList.RemoveAll(symbol => symbol == defineStr);
			}
		}
	}
}

