using UnityEngine;
using System;
using SIGVerse.Common;
using System.Collections.Generic;
using System.Linq;

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
	public class RoomObjectManager : Singleton<RoomObjectManager>
	{
		protected RoomObjectManager() { } // guarantee this will be always a singleton only - can't use the constructor!

		public List<GameObject> roomObjects = null;

		public GameObject GetRoomObject(string name)
		{
			if(this.roomObjects.Where(obj => obj.name == name) == null)
			{
				SIGVerseLogger.Error("There is no object with this name. name=" + name);
				return null;
			}
			return this.roomObjects.Where(obj => obj.name == name).FirstOrDefault();
		}
	}
}

