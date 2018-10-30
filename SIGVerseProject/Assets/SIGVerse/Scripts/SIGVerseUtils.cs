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
		public const string CollisionEffectPath       = "SIGVerse/Prefabs/CollisionEffect";
		public const string CollisionAudioClip1Path   = "SIGVerse/Audio/collision1/collision1";
		public const string CollisionAudioClip2Path   = "SIGVerse/Audio/collision2/collision2";

		private static bool hasInitializeRandom = false;


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

		
		/// <summary>
		/// Generate a random number that follows the normal distribution using the Box-Muller's method. 
		/// </summary>
		public static float GetRandomNumberFollowingNormalDistribution(float sigma=1.0f, float mu=0.0f)
		{
			if(!hasInitializeRandom)
			{
				hasInitializeRandom = true;

				Random.InitState(System.DateTime.Now.Millisecond);
			}

			float x = Random.value;
			float y = Random.value;

			while(x==0.0f){ x = Random.value; }

			return (Mathf.Sqrt(-2.0f * Mathf.Log(x)) * Mathf.Cos(2.0f * Mathf.PI * y)) * sigma + mu;
		}
	}
}


