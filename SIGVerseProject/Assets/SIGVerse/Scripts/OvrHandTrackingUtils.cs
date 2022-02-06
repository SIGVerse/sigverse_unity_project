using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SIGVerse.Common
{
	public static class OvrHandTrackingUtils
	{
		public struct ConversionAngle
		{
			public float x;
			public float y;
			public float z;
			public float w;

			public ConversionAngle(Quaternion qua)
			{
				this.x = qua.x;
				this.y = qua.y;
				this.z = qua.z;
				this.w = qua.w;
			}

			public Quaternion GetQuaternion()
			{
				return new Quaternion(x, y, z, w);
			}
		}

		public static Transform FindChild( Transform parent, string name )
		{
			if ( parent.name == name )
				return parent;

			foreach ( Transform child in parent )
			{
				var found = FindChild( child, name );
				if ( found != null )
					return found;
			}

			return null;
		}

		public static void CreateConversionAngleFile(Animator avatarAnimator, Transform ovrLeftHand, Transform ovrRightHand, string fileName)
		{
			Dictionary<HumanBodyBones, Quaternion> avavarRotMap = GetRotationMap(GetAvatarFingerTransformMap(avatarAnimator));
			Dictionary<HumanBodyBones, Quaternion> ovrRotMap    = GetRotationMap(GetOvrTransformMap(ovrLeftHand, ovrRightHand));

			Dictionary<HumanBodyBones, ConversionAngle> conversionAngleMap = new Dictionary<HumanBodyBones, ConversionAngle>();

			foreach(HumanBodyBones bone in ovrRotMap.Keys)
			{
				conversionAngleMap.Add(bone, new ConversionAngle(Quaternion.Inverse(ovrRotMap[bone]) * avavarRotMap[bone]));
			}
			
			string jsonData = JsonConvert.SerializeObject(conversionAngleMap, Formatting.Indented);
			string path = SaveJsonFile(jsonData, fileName);

//			Debug.Log("Json file : " + jsonData);
			Debug.Log("Output Json file. path=" + path);
		}

		private static Dictionary<HumanBodyBones, Quaternion> GetRotationMap(Dictionary<HumanBodyBones, Transform> transformMap)
		{
			Dictionary<HumanBodyBones, Quaternion> rotMap = new Dictionary<HumanBodyBones, Quaternion>();

			foreach(HumanBodyBones bone in transformMap.Keys)
			{
				rotMap[bone] = transformMap[bone].rotation; 
			}

			return rotMap;
		}

		public static Dictionary<HumanBodyBones, Transform> GetAvatarFingerTransformMap(Animator animator)
		{
			Dictionary<HumanBodyBones, Transform> transformMap = new Dictionary<HumanBodyBones, Transform>();

			foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
			{
				// Add hands and fingers
				if(bone==HumanBodyBones.LeftHand || bone==HumanBodyBones.RightHand || (HumanBodyBones.LeftThumbProximal<=bone && bone<=HumanBodyBones.RightLittleDistal))
				{
					transformMap.Add(bone, animator.GetBoneTransform(bone));
				}
			}

			return transformMap;
		}

		public static Dictionary<HumanBodyBones, Transform> GetOvrTransformMap(Transform leftHand, Transform rightHand)
		{
			Dictionary<HumanBodyBones, Transform> transformMap = new Dictionary<HumanBodyBones, Transform>();

			transformMap.Add(HumanBodyBones.LeftHand, FindChild(leftHand, "Hand_WristRoot"));

			transformMap.Add(HumanBodyBones.LeftThumbProximal,     FindChild(leftHand, "Hand_Thumb1"));
			transformMap.Add(HumanBodyBones.LeftThumbIntermediate, FindChild(leftHand, "Hand_Thumb2"));
			transformMap.Add(HumanBodyBones.LeftThumbDistal,       FindChild(leftHand, "Hand_Thumb3"));

			transformMap.Add(HumanBodyBones.LeftIndexProximal,     FindChild(leftHand, "Hand_Index1"));
			transformMap.Add(HumanBodyBones.LeftIndexIntermediate, FindChild(leftHand, "Hand_Index2"));
			transformMap.Add(HumanBodyBones.LeftIndexDistal,       FindChild(leftHand, "Hand_Index3"));

			transformMap.Add(HumanBodyBones.LeftMiddleProximal,     FindChild(leftHand, "Hand_Middle1"));
			transformMap.Add(HumanBodyBones.LeftMiddleIntermediate, FindChild(leftHand, "Hand_Middle2"));
			transformMap.Add(HumanBodyBones.LeftMiddleDistal,       FindChild(leftHand, "Hand_Middle3"));

			transformMap.Add(HumanBodyBones.LeftRingProximal,     FindChild(leftHand, "Hand_Ring1"));
			transformMap.Add(HumanBodyBones.LeftRingIntermediate, FindChild(leftHand, "Hand_Ring2"));
			transformMap.Add(HumanBodyBones.LeftRingDistal,       FindChild(leftHand, "Hand_Ring3"));

			transformMap.Add(HumanBodyBones.LeftLittleProximal,     FindChild(leftHand, "Hand_Pinky1"));
			transformMap.Add(HumanBodyBones.LeftLittleIntermediate, FindChild(leftHand, "Hand_Pinky2"));
			transformMap.Add(HumanBodyBones.LeftLittleDistal,       FindChild(leftHand, "Hand_Pinky3"));

			transformMap.Add(HumanBodyBones.RightHand, FindChild(rightHand, "Hand_WristRoot"));

			transformMap.Add(HumanBodyBones.RightThumbProximal,     FindChild(rightHand, "Hand_Thumb1"));
			transformMap.Add(HumanBodyBones.RightThumbIntermediate, FindChild(rightHand, "Hand_Thumb2"));
			transformMap.Add(HumanBodyBones.RightThumbDistal,       FindChild(rightHand, "Hand_Thumb3"));

			transformMap.Add(HumanBodyBones.RightIndexProximal,     FindChild(rightHand, "Hand_Index1"));
			transformMap.Add(HumanBodyBones.RightIndexIntermediate, FindChild(rightHand, "Hand_Index2"));
			transformMap.Add(HumanBodyBones.RightIndexDistal,       FindChild(rightHand, "Hand_Index3"));

			transformMap.Add(HumanBodyBones.RightMiddleProximal,     FindChild(rightHand, "Hand_Middle1"));
			transformMap.Add(HumanBodyBones.RightMiddleIntermediate, FindChild(rightHand, "Hand_Middle2"));
			transformMap.Add(HumanBodyBones.RightMiddleDistal,       FindChild(rightHand, "Hand_Middle3"));

			transformMap.Add(HumanBodyBones.RightRingProximal,     FindChild(rightHand, "Hand_Ring1"));
			transformMap.Add(HumanBodyBones.RightRingIntermediate, FindChild(rightHand, "Hand_Ring2"));
			transformMap.Add(HumanBodyBones.RightRingDistal,       FindChild(rightHand, "Hand_Ring3"));

			transformMap.Add(HumanBodyBones.RightLittleProximal,     FindChild(rightHand, "Hand_Pinky1"));
			transformMap.Add(HumanBodyBones.RightLittleIntermediate, FindChild(rightHand, "Hand_Pinky2"));
			transformMap.Add(HumanBodyBones.RightLittleDistal,       FindChild(rightHand, "Hand_Pinky3"));

			return transformMap;
		}

		public static string SaveJsonFile(string jsonData, string fileName)
		{
			string filePath = Application.dataPath + "/Resources/" + fileName + ".json";

			StreamWriter writer = new StreamWriter(filePath, false);

			writer.WriteLine(jsonData);
			writer.Flush();
			writer.Close();

			return filePath;
		}

		private static Dictionary<HumanBodyBones, ConversionAngle> ReadConversionAngleFile(string fileName)
		{
			string configFolderPath = Application.dataPath + ConfigManager.FolderPath + "data/"; 
			string configFilePath   = configFolderPath + fileName + ".json";

			if (!Directory.Exists(configFolderPath))
			{
				Directory.CreateDirectory(configFolderPath);
			}

			if (File.Exists(configFilePath))
			{
				// File open
				StreamReader srConfigReader = new StreamReader(configFilePath);

				string jsonData = srConfigReader.ReadToEnd();
				srConfigReader.Close();

				return JsonConvert.DeserializeObject<Dictionary<HumanBodyBones, ConversionAngle>>(jsonData);
			}
			else
			{
				throw new System.Exception("No ConversionAngle file.");
			}
		}

		public static Dictionary<HumanBodyBones, Quaternion> GetJsonData(string fileName)
		{
			Dictionary<HumanBodyBones, ConversionAngle> conversionAngleMap = ReadConversionAngleFile(fileName);

			Dictionary<HumanBodyBones, Quaternion> map = new Dictionary<HumanBodyBones, Quaternion>();

			foreach(KeyValuePair<HumanBodyBones, ConversionAngle> pair in conversionAngleMap)
			{
				map.Add(pair.Key, pair.Value.GetQuaternion());
			}

			return map;
		}
	}
}
