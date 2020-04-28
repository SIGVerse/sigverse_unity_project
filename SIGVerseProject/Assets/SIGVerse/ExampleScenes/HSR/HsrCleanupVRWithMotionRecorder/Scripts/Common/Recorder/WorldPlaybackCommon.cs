using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SIGVerse.Common.Recorder
{
	public class WorldPlaybackCommon : MonoBehaviour
	{
		public const int PlaybackTypeNone   = 0;
		public const int PlaybackTypeRecord = 1;
		public const int PlaybackTypePlay   = 2;

		// Status
		public const string DataType1Transform   = "11";
//		public const string DataType1VideoPlayer = "12";

		// Events
		public const string DataType1String = "21";

		// Sub type
		public const string DataType2TransformDef = "0";
		public const string DataType2TransformVal = "1";

		//public const string DataType2VideoPlayerDef = "0";
		//public const string DataType2VideoPlayerVal = "1";

		//---------------------------------------

		[TooltipAttribute("Relative to the application folder")]
		public string folderName = "MotionData";


		public string GetFolderPath()
		{
			return Path.GetFullPath(Application.dataPath + "/../" + this.folderName);
		}
	}
}

