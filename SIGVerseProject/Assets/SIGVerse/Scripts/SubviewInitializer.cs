using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.Common
{
	public class SubviewInitializer : MonoBehaviour
	{
		public Camera subview1Camera;
		public Camera subview2Camera;
		public Camera subview3Camera;
		public Camera subview4Camera;

		// Use this for initialization
		void Start ()
		{
			if(subview1Camera != null) { SubviewManager.SetSubviewCamera(SubviewManager.SubviewType.Subview1, this.subview1Camera); }
			if(subview2Camera != null) { SubviewManager.SetSubviewCamera(SubviewManager.SubviewType.Subview2, this.subview1Camera); }
			if(subview3Camera != null) { SubviewManager.SetSubviewCamera(SubviewManager.SubviewType.Subview3, this.subview1Camera); }
			if(subview4Camera != null) { SubviewManager.SetSubviewCamera(SubviewManager.SubviewType.Subview4, this.subview1Camera); }
		}
	
		// Update is called once per frame
		void Update ()
		{
		}
	}
}
