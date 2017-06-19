using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class SIGVerseWindow : EditorWindow
	{
		private const string WindowName   = "SIGVerse";
		private const string MenuItemName = "SIGVerse/SIGVerse Window"; 

		private Texture2D conceptTexture;
		private Rect headerRect;

		private string rosbridgeIP;
		private int    rosbridgePort;
		private int    sigverseBridgePort;
		private bool   displaySigverseMenu;

		void OnEnable ()
		{
			this.conceptTexture = (Texture2D)Resources.Load("SIGVerse/Images/ConceptImageHeader");

			this.headerRect = new Rect(0, 0, 720, 100);

			ConfigInfo configInfo = ConfigManager.InitConfigFile();

			this.rosbridgeIP         = configInfo.rosbridgeIP;
			this.rosbridgePort       = configInfo.rosbridgePort;
			this.sigverseBridgePort  = configInfo.sigverseBridgePort;
			this.displaySigverseMenu = configInfo.displaySigverseMenu;
		}

		[MenuItem(MenuItemName)]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(SIGVerseWindow), false, WindowName);
		}
	
		void OnGUI()
		{
			EditorGUI.DrawPreviewTexture(this.headerRect, this.conceptTexture);

			GUILayout.Space(this.headerRect.height + 10);

			// Config file Settings
			GUILayout.Label("Config file Settings", EditorStyles.boldLabel);

			EditorGUI.indentLevel++;

			EditorGUIUtility.labelWidth = 180;

			EditorGUI.BeginChangeCheck ();

			this.rosbridgeIP         = EditorGUILayout.TextField("Rosbridge IP",          this.rosbridgeIP,        GUILayout.Width(EditorGUIUtility.labelWidth + 120));
			this.rosbridgePort       = EditorGUILayout.IntField ("Rosbridge Port",        this.rosbridgePort,      GUILayout.Width(EditorGUIUtility.labelWidth + 80));
			this.sigverseBridgePort  = EditorGUILayout.IntField ("SIGVerse Bridge Port",  this.sigverseBridgePort, GUILayout.Width(EditorGUIUtility.labelWidth + 80));
			this.displaySigverseMenu = EditorGUILayout.Toggle   ("Display SIGVerse menu", this.displaySigverseMenu);

			if (EditorGUI.EndChangeCheck ())
			{
				ConfigInfo configInfo = new ConfigInfo();

				configInfo.rosbridgeIP         = this.rosbridgeIP;
				configInfo.rosbridgePort       = this.rosbridgePort;
				configInfo.sigverseBridgePort  = this.sigverseBridgePort;
				configInfo.displaySigverseMenu = this.displaySigverseMenu;

				ConfigManager.InitConfigFile(); // Create config file
				ConfigManager.SaveConfig(configInfo);
			}

			GUILayout.Space(10);

			// Create Scripts
			GUILayout.Label("Create Scripts", EditorStyles.boldLabel);

			EditorGUI.indentLevel++;

			if (GUILayout.Button ("Create '" +SIGVerseScriptCreator.ScriptName+ "'", GUILayout.Width(300)))
			{
				SIGVerseScriptCreator.CreateScript();
			}
		}
	}
}

