using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIGVerse.Common
{
	public static class MissingChecker
	{
		[MenuItem("SIGVerse/Missing/Check Current Stage (Scene or Prefab)")]
		public static void CheckCurrentStage()
		{
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
				var issues = new List<string>(1024);
				CheckGameObjectRecursive(prefabStage.prefabContentsRoot, issues);

				if (issues.Count == 0)
					Debug.Log($"[MissingChecker] No missing found in prefab stage: {prefabStage.assetPath}");
				else
					Debug.LogWarning($"[MissingChecker] Found {issues.Count} issue(s) in prefab stage: {prefabStage.assetPath}\n" + string.Join("\n", issues));

				return;
			}

			// Fallback to active scene
			var scene = SceneManager.GetActiveScene();
			if (!scene.isLoaded)
			{
				Debug.LogError("No active scene loaded.");
				return;
			}

			var roots = scene.GetRootGameObjects();
			var sceneIssues = new List<string>(1024);
			foreach (var root in roots)
				CheckGameObjectRecursive(root, sceneIssues);

			if (sceneIssues.Count == 0)
				Debug.Log($"[MissingChecker] No missing found in scene: {scene.path}");
			else
				Debug.LogWarning($"[MissingChecker] Found {sceneIssues.Count} issue(s) in scene: {scene.path}\n" + string.Join("\n", sceneIssues));
		}

		[MenuItem("SIGVerse/Missing/Check All Scenes & Prefabs in Assets")]
		public static void CheckAllScenesAndPrefabsInAssets()
		{
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null && prefabStage.scene.isDirty)
			{
				Debug.LogError("Please save the prefab you are currently editing before running this tool.");
				return;
			}

			for (int i = 0;i < SceneManager.sceneCount;i++)
			{
				var s = SceneManager.GetSceneAt(i);
				if (s.isLoaded && s.isDirty)
				{
					Debug.LogError("Please save the scene(s) you are currently editing before running this tool.");
					return;
				}
			}

			var setup = EditorSceneManager.GetSceneManagerSetup();

			try
			{
				// Scenes
				var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
				foreach (var guid in sceneGuids)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (string.IsNullOrEmpty(path)) continue;

					var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

					var issues = new List<string>(1024);
					foreach (var root in scene.GetRootGameObjects())
						CheckGameObjectRecursive(root, issues);

					if (issues.Count == 0)
						Debug.Log($"[MissingChecker] No missing found in scene: {path}");
					else
						Debug.LogWarning($"[MissingChecker] Found {issues.Count} issue(s) in scene: {path}\n" + string.Join("\n", issues));
				}

				// Prefabs
				var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
				foreach (var guid in prefabGuids)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (string.IsNullOrEmpty(path)) continue;

					GameObject root = null;
					try
					{
						root = PrefabUtility.LoadPrefabContents(path);

						var issues = new List<string>(1024);
						CheckGameObjectRecursive(root, issues);

						if (issues.Count == 0)
							Debug.Log($"[MissingChecker] No missing found in prefab: {path}");
						else
							Debug.LogWarning($"[MissingChecker] Found {issues.Count} issue(s) in prefab: {path}\n" + string.Join("\n", issues));
					}
					finally
					{
						if (root != null)
							PrefabUtility.UnloadPrefabContents(root);
					}
				}
			}
			finally
			{
				EditorSceneManager.RestoreSceneManagerSetup(setup);
			}
		}

		private static void CheckGameObjectRecursive(GameObject go, List<string> issues)
		{
			// 1) Missing Script
			var comps = go.GetComponents<Component>();
			for (int i = 0;i < comps.Length;i++)
			{
				if (comps[i] == null)
				{
					issues.Add($"[MissingScript] {GetHierarchyPath(go)} (component index {i})");
				}
			}

			// 2) Missing references in serialized fields
			foreach (var comp in comps)
			{
				if (comp == null) continue;

				var so = new SerializedObject(comp);
				var it = so.GetIterator();
				bool enterChildren = true;
				while (it.NextVisible(enterChildren))
				{
					enterChildren = false;

					if (it.propertyType != SerializedPropertyType.ObjectReference)
						continue;

					// objectReferenceValue is null, but objectReferenceInstanceIDValue is not 0, so the reference is missing.
					if (it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
					{
						issues.Add($"[MissingRef] {GetHierarchyPath(go)} / {comp.GetType().Name}.{it.propertyPath}");
					}
				}
			}

			// children
			var t = go.transform;
			for (int i = 0;i < t.childCount;i++)
			{
				CheckGameObjectRecursive(t.GetChild(i).gameObject, issues);
			}
		}

		private static string GetHierarchyPath(GameObject go)
		{
			var path = go.name;
			var t = go.transform;
			while (t.parent != null)
			{
				t = t.parent;
				path = t.name + "/" + path;
			}
			return path;
		}
	}
}