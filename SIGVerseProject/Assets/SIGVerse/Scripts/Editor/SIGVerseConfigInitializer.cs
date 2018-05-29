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
	public class SIGVerseConfigInitializer
	{
		static SIGVerseConfigInitializer()
		{
			DirectoryInfo sampleDirectoryInfo = new DirectoryInfo(Application.dataPath + ConfigManager.FolderPath + "sample/");

			if(!sampleDirectoryInfo.Exists) { return; }

			foreach (FileInfo fileInfo in sampleDirectoryInfo.GetFiles().Where(fileinfo => fileinfo.Name != ".gitignore"))
			{
				string destFilePath = Application.dataPath + ConfigManager.FolderPath + fileInfo.Name;

				if (!File.Exists(destFilePath))
				{
					fileInfo.CopyTo(destFilePath);
				}
			}
		}
	}
}

