using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SIGVerse.Common
{
	public static class SIGVerseUtils
	{
		public const string SIGVerse = "SIGVerse";

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

				UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
			}

			float x = UnityEngine.Random.value;
			float y = UnityEngine.Random.value;

			while(x==0.0f){ x = UnityEngine.Random.value; }

			return (Mathf.Sqrt(-2.0f * Mathf.Log(x)) * Mathf.Cos(2.0f * Mathf.PI * y)) * sigma + mu;
		}


		/// <summary>
		/// Get an ordinal number.
		/// </summary>
		public static string GetOrdinal(int number)
		{
			string suffix = String.Empty;

			int ones = number % 10;
			int tens = (int)Math.Floor(number / 10M) % 10;

			if (tens == 1)
			{
				suffix = "th";
			}
			else
			{
				switch (ones)
				{
					case 1:  { suffix = "st"; break; }
					case 2:  { suffix = "nd"; break; }
					case 3:  { suffix = "rd"; break; }
					default: { suffix = "th"; break; }
				}
			}
			return String.Format("{0}{1}", number, suffix);
		}
	
		/// <summary>
		/// based on https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
		/// </summary>
		public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, List<string> excludedFolders = null)
		{
			// Get information about the source directory
			var dir = new DirectoryInfo(sourceDir);

			// Check if the source directory exists
			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

			// Cache directories before we start copying
			DirectoryInfo[] dirs = dir.GetDirectories();

			// Create the destination directory
			Directory.CreateDirectory(destinationDir);

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);

				if(!File.Exists(targetFilePath))
				{
					file.CopyTo(targetFilePath);
				}
			}

			// If recursive and copying subdirectories, recursively call this method
			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					if(excludedFolders!=null && excludedFolders.Contains(subDir.Name)) { continue; }

					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true, excludedFolders);
				}
			}
		}
	}
}


