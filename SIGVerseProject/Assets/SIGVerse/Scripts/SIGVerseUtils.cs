using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SIGVerse.Common
{
	public static class SIGVerseUtils
	{
		public const string SIGVerseMenuResourcePath  = "SIGVerse/Prefabs/SIGVerseMenu";
		public const string EventSystemResourcePath   = "SIGVerse/Prefabs/EventSystem";
		public const string WarningWindowResourcePath = "SIGVerse/Prefabs/WarningWindow";
		public const string ConceptImageResourcePath  = "SIGVerse/Images/ConceptImageHeader";
		public const string UnlitShaderResourcePath   = "SIGVerse/Shaders/UnlitShader";


		public static Vector3 CalcContactAveragePoint(Collision collision)
		{
			ContactPoint[] contactPoints = collision.contacts;

			Vector3 contactPointAve = Vector3.zero;

			foreach(ContactPoint contactPoint in contactPoints)
			{
				contactPointAve += contactPoint.point;
			}

			contactPointAve /= contactPoints.Length;

			return contactPointAve;
		}


		public static string GetHierarchyPath(Transform transform)
		{
			string path = transform.name;

			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			return path;
		}


		public static Transform FindTransformFromChild(Transform root, string name)
		{
			Transform[] transforms = root.GetComponentsInChildren<Transform>();

			foreach (Transform transform in transforms)
			{
				if (transform.name == name)
				{
					return transform;
				}
			}

			return null;
		}


		public static GameObject FindGameObjectFromChild(Transform root, string name)
		{
			Transform transform = FindTransformFromChild(root, name);

			if(transform!=null)
			{
				return transform.gameObject;
			}
			else
			{
				return null;
			}
		}

		public static T[] FindObjectsOfInterface<T>() where T : class
		{
			List<T> list = new List<T>();

			Component[] allComponents = GameObject.FindObjectsOfType<Component>(); // High Load

			foreach (Component component in allComponents)
			{
				var componentAsT = component as T;

				if (componentAsT != null)
				{
					list.Add (componentAsT);
				}
			}

			return list.ToArray();
		}
	}
}


