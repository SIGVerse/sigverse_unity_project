using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using SIGVerse.Common;

/// <summary>
/// SIGVerse Script Creator
/// </summary>
public static class SIGVerseScriptCreator 
{
	private const string MenuItemName = "SIGVerse/Create Scripts"; 
	public  const string ScriptName   = "EditorConstantsManager.cs";
	private const string ScriptPath   = "Assets/SIGVerse/Scripts/" + ScriptName;

	/// <summary>
	/// Create script
	/// </summary>
//	[MenuItem(MenuItemName)]
	public static void CreateScript()
	{
		if (!CanCreate()) { return; }

		SIGVerseScriptCreator.CreateScripts();

		Debug.Log("Created '" +ScriptName+ "'");
		EditorUtility.DisplayDialog("Create Scripts", "Created '" +ScriptName+ "'", "OK");
	}

	/// <summary>
	/// Detail of creating script
	/// </summary>
	public static void CreateScripts()
	{
		StringBuilder stringBuilder = new StringBuilder();

		stringBuilder.AppendLine("using System;");
		stringBuilder.AppendLine("using System.Collections.ObjectModel;");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("/// <summary>");
		stringBuilder.AppendLine("/// Constants management class");
		stringBuilder.AppendLine("/// </summary>");
		stringBuilder.AppendFormat("public static class {0}", Path.GetFileNameWithoutExtension(ScriptPath)).AppendLine();
		stringBuilder.AppendLine("{");
		stringBuilder.Append("\t").AppendLine("public static readonly ReadOnlyCollection<string> sceneNameArray");
		stringBuilder.Append("\t\t").Append("= Array.AsReadOnly(new string[] { ");

		bool isFirstScene = true;

		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (isFirstScene)
			{
				isFirstScene = false;
			}
			else
			{
				stringBuilder.Append(", ");
			}

			stringBuilder.Append("\""+Path.GetFileNameWithoutExtension(scene.path) + "\"");
		}
		
		stringBuilder.AppendLine(" });");
		stringBuilder.AppendLine("}");

		string directoryName = Path.GetDirectoryName(ScriptPath);

		if(!Directory.Exists(directoryName))
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch(Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				EditorUtility.DisplayDialog("Exception occurred", "Couldn't create directory !", "OK");
				return;
			}
		}

		File.WriteAllText(ScriptPath, stringBuilder.ToString());

		AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
	}

	/// <summary>
	/// Return that creating the script file is possible or not
	/// </summary>
//	[MenuItem(MenuItemName, true)]
	public static bool CanCreate()
	{
		return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
	}
}

