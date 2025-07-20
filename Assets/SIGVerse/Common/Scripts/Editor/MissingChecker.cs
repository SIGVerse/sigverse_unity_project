using System.Collections;
using UnityEditor;
using UnityEngine;
using SIGVerse.Common;

public class MissingChecker : MonoBehaviour
{
	[MenuItem("SIGVerse/Tools/Find Missing (in Scene)")]
	private static void FindMissingInScene()
	{
		Debug.Log("Find Missing (in Scene) - Start -");

		GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

		int cntMissing = 0;

		foreach (GameObject gameObject in gameObjects)
		{
			if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
			{
				Debug.Log(SIGVerseUtils.GetHierarchyPath(gameObject.transform));
				cntMissing++;
			}
		}

		Debug.Log("Find Missing (in Scene) - End -  (All GameObjects = "+gameObjects.Length+". Missing Count="+cntMissing+")");
	}

	[MenuItem("SIGVerse/Tools/Find Missing Prefabs (in Assets)")]
	private static void FindMissingPrefabsInAssets()
	{
		Debug.Log("Find Missing Prefabs (in Assets) - Start -");

		string[] guids = AssetDatabase.FindAssets("t:Prefab");

		int cntMissing = 0;

		foreach ( string guid in guids )
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			
			if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
			{
				Debug.Log(path);
				cntMissing++;
			}
		}

		Debug.Log("Find Missing Prefabs (in Assets) - End -  (All Prefabs = "+guids.Length+". Missing Count="+cntMissing+")");
	}}
