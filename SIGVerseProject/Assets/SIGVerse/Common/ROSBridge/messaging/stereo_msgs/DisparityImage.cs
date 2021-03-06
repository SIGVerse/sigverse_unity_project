// Generated by gencs from stereo_msgs/DisparityImage.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;

using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;

namespace SIGVerse.RosBridge 
{
	namespace stereo_msgs 
	{
		[System.Serializable]
		public class DisparityImage : RosMessage
		{
			public std_msgs.Header header;
			public sensor_msgs.Image image;
			public float f;
			public float T;
			public sensor_msgs.RegionOfInterest valid_window;
			public float min_disparity;
			public float max_disparity;
			public float delta_d;


			public DisparityImage()
			{
				this.header = new std_msgs.Header();
				this.image = new sensor_msgs.Image();
				this.f = 0.0f;
				this.T = 0.0f;
				this.valid_window = new sensor_msgs.RegionOfInterest();
				this.min_disparity = 0.0f;
				this.max_disparity = 0.0f;
				this.delta_d = 0.0f;
			}

			public DisparityImage(std_msgs.Header header, sensor_msgs.Image image, float f, float T, sensor_msgs.RegionOfInterest valid_window, float min_disparity, float max_disparity, float delta_d)
			{
				this.header = header;
				this.image = image;
				this.f = f;
				this.T = T;
				this.valid_window = valid_window;
				this.min_disparity = min_disparity;
				this.max_disparity = max_disparity;
				this.delta_d = delta_d;
			}

			new public static string GetMessageType()
			{
				return "stereo_msgs/DisparityImage";
			}

			new public static string GetMD5Hash()
			{
				return "04a177815f75271039fa21f16acad8c9";
			}
		} // class DisparityImage
	} // namespace stereo_msgs
} // namespace SIGVerse.ROSBridge

