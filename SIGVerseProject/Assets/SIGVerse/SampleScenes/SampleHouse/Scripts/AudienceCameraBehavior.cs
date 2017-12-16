using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.SampleScenes.SampleHouse
{
	public class AudienceCameraBehavior : MonoBehaviour
	{
		public SIGVerse.Common.SubviewType subviewType;

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
