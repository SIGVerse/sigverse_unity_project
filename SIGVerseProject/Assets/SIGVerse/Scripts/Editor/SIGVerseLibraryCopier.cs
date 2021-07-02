using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class SIGVerseLibraryCopier
	{
		static SIGVerseLibraryCopier()
		{
			FileInfo stopLibraryCopyFile = new FileInfo(Application.dataPath + "/Plugins/StopLibraryCopy.txt");

			if (stopLibraryCopyFile.Exists) { return; }

			string[] contentsPaths = new string[] { "/MonoBleedingEdge/lib/mono/unityjit/System.Windows.Forms.dll" };

			foreach (string contentsPath in contentsPaths)
			{
				FileInfo contentFile = new FileInfo(EditorApplication.applicationContentsPath + contentsPath);

				if (!contentFile.Exists)
				{
					Debug.LogError("Content file NOT exists. path=" + contentsPath + "\nIf you want to stop this error, please create " + stopLibraryCopyFile.FullName);
					continue;
				}

				FileInfo contentFileInPlugins = new FileInfo(Application.dataPath + "/Plugins/" + contentFile.Name);

//				if (!contentFileInPlugins.Exists || contentFile.CreationTime > contentFileInPlugins.CreationTime)
				if (!contentFileInPlugins.Exists || contentFile.LastWriteTime > contentFileInPlugins.LastWriteTime)
				{
					contentFile.CopyTo(contentFileInPlugins.FullName, true);
					Debug.Log("Content file copied. path=" + contentFileInPlugins.FullName);
				}
			}
		}
	}
}

