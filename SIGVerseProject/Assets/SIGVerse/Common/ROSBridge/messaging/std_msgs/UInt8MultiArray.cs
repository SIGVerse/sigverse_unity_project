// Generated by gencs from std_msgs/UInt8MultiArray.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;

using SIGVerse.RosBridge.std_msgs;

namespace SIGVerse.RosBridge 
{
	namespace std_msgs 
	{
		[System.Serializable]
		public class UInt8MultiArray : RosMessage
		{
			public std_msgs.MultiArrayLayout layout;
			public System.Collections.Generic.List<byte>  data;


			public UInt8MultiArray()
			{
				this.layout = new std_msgs.MultiArrayLayout();
				this.data = new System.Collections.Generic.List<byte>();
			}

			public UInt8MultiArray(std_msgs.MultiArrayLayout layout, System.Collections.Generic.List<byte>  data)
			{
				this.layout = layout;
				this.data = data;
			}

			new public static string GetMessageType()
			{
				return "std_msgs/UInt8MultiArray";
			}

			new public static string GetMD5Hash()
			{
				return "82373f1612381bb6ee473b5cd6f5d89c";
			}
		} // class UInt8MultiArray
	} // namespace std_msgs
} // namespace SIGVerse.ROSBridge

