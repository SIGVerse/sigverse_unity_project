using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SIGVerse.Common
{
	[InitializeOnLoad]
	public class SIGVerseConfigInitializer : IPostprocessBuildWithReport
	{
		static SIGVerseConfigInitializer()
		{
			DirectoryInfo sampleDirectoryInfo = new DirectoryInfo(Application.dataPath + "/../" + ConfigManager.FolderName + "/sample/");

			if (!sampleDirectoryInfo.Exists) { return; }

			foreach (FileInfo fileInfo in sampleDirectoryInfo.GetFiles().Where(fileinfo => fileinfo.Name != ".gitignore"))
			{
				string destFilePath = Application.dataPath + "/../" + ConfigManager.FolderName + "/" + fileInfo.Name;

				if (!File.Exists(destFilePath))
				{
					fileInfo.CopyTo(destFilePath);
				}
			}
		}

		public int callbackOrder { get { return 0; } }

		public void OnPostprocessBuild(BuildReport report)
		{
//			if(report.summary.result == BuildResult.Succeeded && report.summary.platformGroup == BuildTargetGroup.Standalone)
			if(report.summary.platformGroup == BuildTargetGroup.Standalone)
			{
				this.CopyConfigFileForBuild(Path.GetDirectoryName(report.summary.outputPath)+"/" + ConfigManager.FolderName);
			}
		}

		private void CopyConfigFileForBuild(string buildDirPath)
		{
			string configFolderPath = Application.dataPath + "/../" + ConfigManager.FolderName + "/";

			SIGVerseUtils.CopyDirectory(configFolderPath, buildDirPath, true, new List<string>(){"sample"}); 
		}
	}
}

