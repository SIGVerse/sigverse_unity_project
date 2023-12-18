using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LinkType  = SIGVerse.TurtleBot3.TurtleBot3LinkInfo.LinkType;
using JointType = SIGVerse.TurtleBot3.TurtleBot3JointInfo.JointType;
using MovementType = SIGVerse.TurtleBot3.TurtleBot3JointInfo.MovementType;
using MovementAxis = SIGVerse.TurtleBot3.TurtleBot3JointInfo.MovementAxis;

namespace SIGVerse.TurtleBot3
{
	public static class TurtleBot3Common
	{
		public const float MaxSpeedBase    = 0.26f; // [m/s]
		public const float MaxSpeedBaseRad = 1.82f; // [rad/s] 
		public const float MinSpeed    = 0.001f; // [m/s]
		public const float MinSpeedRad = 0.01f;  // [rad/s]

		public const float MaxSpeedArm  = 1.5f;  // [rad/s] Provisional value. 
		public const float MaxSpeedHand = 0.15f; // [m/s]   Provisional value. 

		// Joint names
		//public const string WheelLeftJoint  = "wheel_left_joint";
		//public const string WheelRightJoint = "wheel_right_joint";

		//public const string Joint1 = "joint1"; // Rotate Link2
		//public const string Joint2 = "joint2"; // Rotate Link3
		//public const string Joint3 = "joint3"; // Rotate Link4
		//public const string Joint4 = "joint4"; // Rotate Link5
		//public const string GripJoint    = "grip_joint";
		//public const string GripJointSub = "grip_joint_sub";

		
		public static Dictionary<LinkType, string>              linkNameMap;
//		public static Dictionary<LinkType, TurtleBot3LinkInfo>  linkInfoMap;
		public static Dictionary<JointType, string>             jointNameMap;
		public static Dictionary<string,   TurtleBot3JointInfo> jointInfoMap;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void CreateLinkInfo()
		{
			linkNameMap = new Dictionary<LinkType, string>();

			// Links of TurtleBot3
			linkNameMap.Add(LinkType.Odom,          "odom");
			linkNameMap.Add(LinkType.BaseFootprint, "base_footprint");
			linkNameMap.Add(LinkType.BaseLink,      "base_link");

			linkNameMap.Add(LinkType.CasterBackLeftLink,      "caster_back_left_link");
			linkNameMap.Add(LinkType.CasterBackRightLink,     "caster_back_right_link");
			linkNameMap.Add(LinkType.WheelLeftLink,           "wheel_left_link");
			linkNameMap.Add(LinkType.WheelRightLink,          "wheel_right_link");

			//linkNameMap.Add(LinkType.CameraLink,              "camera_link");
			//linkNameMap.Add(LinkType.CameraRgbFrame,          "camera_rgb_frame");
			//linkNameMap.Add(LinkType.CameraDepthFrame,        "camera_depth_frame");
			//linkNameMap.Add(LinkType.CameraRgbOpticalFram,    "camera_rgb_optical_frame");
			//linkNameMap.Add(LinkType.CameraDepthOpticalFrame, "camera_depth_optical_frame");
			linkNameMap.Add(LinkType.zedm_base_link,                  "zedm_base_link");
			linkNameMap.Add(LinkType.zedm_camera_center,              "zedm_camera_center");
			linkNameMap.Add(LinkType.zedm_left_camera_frame,          "zedm_left_camera_frame");
			linkNameMap.Add(LinkType.zedm_right_camera_frame,         "zedm_right_camera_frame");
			linkNameMap.Add(LinkType.zedm_left_camera_optical_frame,  "zedm_left_camera_optical_frame");
			linkNameMap.Add(LinkType.zedm_right_camera_optical_frame, "zedm_right_camera_optical_frame");

			// Laser Distance Sensor
			linkNameMap.Add(LinkType.BaseScan, "base_scan");

			// Links of Open-Manipulator
			linkNameMap.Add(LinkType.Link1,       "link1");
			linkNameMap.Add(LinkType.Link2,       "link2");
			linkNameMap.Add(LinkType.Link3,       "link3");
			linkNameMap.Add(LinkType.Link4,       "link4");
			linkNameMap.Add(LinkType.Link5,       "link5");
			linkNameMap.Add(LinkType.GripLink,    "grip_link");
			linkNameMap.Add(LinkType.GripLinkSub, "grip_link_sub");
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void CreateJointInfo()
		{
			jointNameMap = new Dictionary<JointType, string>();

			// Joints of TurtleBot3
			jointNameMap.Add(JointType.WheelLeftJoint, "wheel_left_joint");
			jointNameMap.Add(JointType.WheelRightJoint,"wheel_right_joint");

			// Joints of Open-Manipulator
			jointNameMap.Add(JointType.Joint1,      "joint1"); 
			jointNameMap.Add(JointType.Joint2,      "joint2"); 
			jointNameMap.Add(JointType.Joint3,      "joint3"); 
			jointNameMap.Add(JointType.Joint4,      "joint4"); 
			jointNameMap.Add(JointType.GripJoint,   "grip_joint");
			jointNameMap.Add(JointType.GripJointSub,"grip_joint_sub");


			jointInfoMap = new Dictionary<string, TurtleBot3JointInfo>();

			// Joints of TurtleBot3
			jointInfoMap.Add(jointNameMap[JointType.WheelLeftJoint],  new TurtleBot3JointInfo(JointType.WheelLeftJoint,  LinkType.WheelLeftLink,  MovementType.Angular, MovementAxis.MinusY, -180.0f, +180.0f)); // rad=3.14
			jointInfoMap.Add(jointNameMap[JointType.WheelRightJoint], new TurtleBot3JointInfo(JointType.WheelRightJoint, LinkType.WheelRightLink, MovementType.Angular, MovementAxis.MinusY, -180.0f, +180.0f)); // rad=3.14

			// Joints of Open-Manipulator
			jointInfoMap.Add(jointNameMap[JointType.Joint1],       new TurtleBot3JointInfo(JointType.Joint1,       LinkType.Link2,       MovementType.Angular, MovementAxis.MinusZ, -162.15f, +162.15f)); // rad=2.83
			jointInfoMap.Add(jointNameMap[JointType.Joint2],       new TurtleBot3JointInfo(JointType.Joint2,       LinkType.Link3,       MovementType.Angular, MovementAxis.MinusY, -110.00f, +110.00f)); // rad=1.92
			jointInfoMap.Add(jointNameMap[JointType.Joint3],       new TurtleBot3JointInfo(JointType.Joint3,       LinkType.Link4,       MovementType.Angular, MovementAxis.MinusY, -162.15f, +162.15f)); // rad=2.83
			jointInfoMap.Add(jointNameMap[JointType.Joint4],       new TurtleBot3JointInfo(JointType.Joint4,       LinkType.Link5,       MovementType.Angular, MovementAxis.MinusY, -162.15f, +162.15f)); // rad=2.83
			jointInfoMap.Add(jointNameMap[JointType.GripJoint],    new TurtleBot3JointInfo(JointType.GripJoint,    LinkType.GripLink,    MovementType.Linear,  MovementAxis.MinusY, -  0.01f, +  0.035f));
			jointInfoMap.Add(jointNameMap[JointType.GripJointSub], new TurtleBot3JointInfo(JointType.GripJointSub, LinkType.GripLinkSub, MovementType.Linear,  MovementAxis.PlusY,  -  0.01f, +  0.035f));
		}

		public static TurtleBot3JointInfo GetJointInfo(string name)
		{
			foreach(KeyValuePair<string, TurtleBot3JointInfo> jointInfoPair in jointInfoMap)
			{
				if(jointInfoPair.Key == name) { return jointInfoPair.Value; }
			}
			
			return null;
		}


		public static Transform FindGameObjectFromChild(Transform root, JointType jointType)
		{
			return FindGameObjectFromChild(root, jointInfoMap[jointNameMap[jointType]].linkType);
		}

		public static Transform FindGameObjectFromChild(Transform root, LinkType linkType)
		{
			Transform[] transforms = root.GetComponentsInChildren<Transform>();

			foreach (Transform transform in transforms)
			{
				if (transform.name == linkNameMap[linkType])
				{
					return transform;
				}
			}

			return null;
		}

		public static List<Transform> GetLinksInChildren(Transform root)
		{
			List<Transform> linkList = new List<Transform>();

			AddLink(linkList, FindGameObjectFromChild(root, LinkType.BaseFootprint));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.BaseLink));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.CasterBackLeftLink));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.CasterBackRightLink));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.WheelLeftLink));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.WheelRightLink));
			//AddLink(linkList, FindGameObjectFromChild(root, LinkType.CameraLink));
			//AddLink(linkList, FindGameObjectFromChild(root, LinkType.CameraRgbFrame));
			//AddLink(linkList, FindGameObjectFromChild(root, LinkType.CameraDepthFrame));
			//AddLink(linkList, FindGameObjectFromChild(root, LinkType.CameraRgbOpticalFram));
			//AddLink(linkList, FindGameObjectFromChild(root, LinkType.CameraDepthOpticalFrame));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_base_link));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_camera_center));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_left_camera_frame));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_right_camera_frame));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_left_camera_optical_frame));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.zedm_right_camera_optical_frame));

			AddLink(linkList, FindGameObjectFromChild(root, LinkType.BaseScan));

			AddLink(linkList, FindGameObjectFromChild(root, LinkType.Link1));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.Link2));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.Link3));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.Link4));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.Link5));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.GripLink));
			AddLink(linkList, FindGameObjectFromChild(root, LinkType.GripLinkSub));

			return linkList;
		}

		private static void AddLink(List<Transform> linkList, Transform link)
		{
			if(link!=null)
			{
				linkList.Add(link);
			}
		}

		public static Dictionary<string, Transform> GetJointNameToLinkMap(Transform root)
		{
			Dictionary<string, Transform> linkMap = new Dictionary<string, Transform>();

			foreach(KeyValuePair<string, TurtleBot3JointInfo> jointInfoPair in jointInfoMap)
			{
				linkMap.Add(jointInfoPair.Key, FindGameObjectFromChild(root, jointInfoPair.Value.linkType));
			}
			return linkMap;
		}


		public static float GetClampedPosition(string name, float value)
		{
			if (jointInfoMap[name].movementType == MovementType.Angular)
			{
				return Mathf.Clamp(value, jointInfoMap[name].minVal * Mathf.Deg2Rad, jointInfoMap[name].maxVal * Mathf.Deg2Rad);
			}
			else
			{
				return Mathf.Clamp(value, jointInfoMap[name].minVal, jointInfoMap[name].maxVal);
			}
		}

		public static void UpdateJoint(string name, Transform transform, float val)
		{
			MovementType movementType = jointInfoMap[name].movementType;
			
			if(movementType == MovementType.Angular)
			{
				transform.localEulerAngles = GetMovedVector3(transform.localEulerAngles, jointInfoMap[name], val * Mathf.Rad2Deg);
			}
			else
			{
				transform.localPosition = GetMovedVector3(transform.localPosition, jointInfoMap[name], val);
			}
		}

		private static Vector3 GetMovedVector3(Vector3 vec, TurtleBot3JointInfo jointInfo, float val)
		{
			Vector3 newVec = vec;

			MovementAxis movementAxis = jointInfo.movementAxis;
			float min = jointInfo.minVal;
			float max = jointInfo.maxVal;

			switch(movementAxis)
			{
				case MovementAxis.PlusX:
				{
					newVec.x = Mathf.Clamp(val, min, max);
					break;
				}
				case MovementAxis.PlusY:
				{
					newVec.y = Mathf.Clamp(val, min, max);
					break;
				}
				case MovementAxis.PlusZ:
				{
					newVec.z = Mathf.Clamp(val, min, max);
					break;
				}
				case MovementAxis.MinusX:
				{
					newVec.x = Mathf.Clamp(-val, -max, -min);
					break;
				}
				case MovementAxis.MinusY:
				{
					newVec.y = Mathf.Clamp(-val, -max, -min);
					break;
				}
				case MovementAxis.MinusZ:
				{
					newVec.z = Mathf.Clamp(-val, -max, -min);
					break;
				}
			}

			return newVec;
		}


		public static float GetCorrectedJointsEulerAngle(string name, float value)
		{
			if (name == jointNameMap[JointType.WheelLeftJoint] || 
				name == jointNameMap[JointType.WheelRightJoint]
			){
				value = GetCorrectedEulerAngle(value, jointInfoMap[name].minVal, jointInfoMap[name].maxVal);
			}

			if (name == jointNameMap[JointType.Joint1] || 
				name == jointNameMap[JointType.Joint2] || 
				name == jointNameMap[JointType.Joint3] || 
				name == jointNameMap[JointType.Joint4]
			){
				value = GetCorrectedEulerAngle(value, jointInfoMap[name].minVal, jointInfoMap[name].maxVal, 5.0f);
			}

			return value;
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue, float play)
		{
			value = (value > maxValue + play) ? value - 360f : value;
			value = (value < minValue - play) ? value + 360f : value;

			return value;
		}

		private static float GetCorrectedEulerAngle(float value, float minValue, float maxValue)
		{
			return GetCorrectedEulerAngle(value, minValue, maxValue, 0.0f);
		}

		//public static Vector3 ConvertPositionRosToUnity(Vector3 position)
		//{
		//	return new Vector3(-position.y, position.z, position.x);
		//}

		//public static Vector3 ConvertPositionUnityToRos(Vector3 position)
		//{
		//	return new Vector3(position.z, -position.x, position.y);
		//}
	}
}

