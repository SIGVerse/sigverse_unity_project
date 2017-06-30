using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.SampleScenes.Playbacker
{
	public class CameraBehavior : MonoBehaviour
	{
		public SubviewManager.SubviewType subviewType;

		// Use this for initialization
		void Start ()
		{
			SubviewManager.SetSubviewCamera(this.subviewType, this.GetComponent<Camera>());
		}
	
		// Update is called once per frame
		void Update ()
		{
		}
	}
}
