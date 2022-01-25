using UnityEditor;
using UnityEngine;

namespace SIGVerse.Common
{
	public static class SIGVerseMenuCommandUtil
	{
		[MenuItem("CONTEXT/SkinnedMeshRenderer/SIGVerse - Save Mesh")]
		private static void SaveMesh(MenuCommand menuCommand)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = menuCommand.context as SkinnedMeshRenderer;

			if (skinnedMeshRenderer.sharedMesh == null){ return; }
		
			string path = EditorUtility.SaveFilePanelInProject("SIGVerse - Save Mesh", "SavedMesh", "asset", "");

			if (string.IsNullOrEmpty(path)){ return; }
 
			Mesh mesh = GameObject.Instantiate(skinnedMeshRenderer.sharedMesh);

			AssetDatabase.CreateAsset(mesh, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
