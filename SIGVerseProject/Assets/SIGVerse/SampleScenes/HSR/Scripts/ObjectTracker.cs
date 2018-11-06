using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.SampleScenes.Hsr
{
	public class ObjectTracker : MonoBehaviour
	{
		public Transform target;

		private void Update()
		{
			this.Track();
		}

		private void Track()
		{
			this.transform.position = this.target.position;
			this.transform.rotation = this.target.rotation;
		}


		#if UNITY_EDITOR
		[CustomEditor(typeof(ObjectTracker))]
		public class ObjectTrackerEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI ();

				ObjectTracker trackingCameraController = base.target as ObjectTracker;

				trackingCameraController.Track();
			}
		}
		#endif
	}
}
