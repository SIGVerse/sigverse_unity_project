using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class SIGVerseSettings : EditorWindow
	{
		private const char   SymbolSeparator = ';';
		private const string SIGVerseScriptingDefineSymbolsKey = "sigverse_scripting_define_symbols";

		private const string DefineSIGVerseMySQL = "SIGVERSE_MYSQL";

		private const string WindowName   = "SIGVerse";
		private const string MenuItemName = "SIGVerse/SIGVerse Settings"; 

		private static readonly BuildTargetGroup[] BuildTargetGroupList = new BuildTargetGroup[]
		{
			BuildTargetGroup.Standalone,
			BuildTargetGroup.iOS,
			BuildTargetGroup.Android,
			BuildTargetGroup.WebGL,
			BuildTargetGroup.PS4
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

		private bool   isUsingMySQL;


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

			this.isUsingMySQL = Array.IndexOf(defineSymbols, DefineSIGVerseMySQL) >= 0;
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
				GUILayout.Label("* Please add some libraries(MySql.Data.dll, System.Data.dll) if you want to use MySQL.");
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				foreach (BuildTargetGroup buildTargetGroup in BuildTargetGroupList)
				{
					string[] scriptingDefineSymbols  = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(SymbolSeparator);

					List<string> scriptingDefineSymbolList = new List<string>(scriptingDefineSymbols);

					// Add/Remove MySQL define
					this.UpdateScriptingDefineSymbolList(ref scriptingDefineSymbolList, this.isUsingMySQL, DefineSIGVerseMySQL);

					string defineSymbolsStr = String.Join(SymbolSeparator.ToString(), scriptingDefineSymbolList.ToArray());

					// Update ScriptingDefineSymbols of PlayerSettings
					PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbolsStr);

					// Update SIGVerseScriptingDefineSymbols of EditorUserSettings
					EditorUserSettings.SetConfigValue(SIGVerseScriptingDefineSymbolsKey, defineSymbolsStr);
				}
			}

			GUILayout.Space(10);
			GUILayout.Box("", GUILayout.Width(this.position.width), GUILayout.Height(2));


			//// Create Scripts
			//GUILayout.Label("Create Scripts", EditorStyles.boldLabel);

			//EditorGUI.indentLevel++;

			//if (GUILayout.Button ("Create '" +SIGVerseScriptCreator.ScriptName+ "'", GUILayout.Width(300)))
			//{
			//	SIGVerseScriptCreator.CreateScript();
			//}
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

