using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Common
{
	public class SubviewOptionController : MonoBehaviour
	{
		public Camera subviewCamera;

		public SubviewType subviewType;
		public SubviewPositionType subviewPositionType;

		public float offsetX = 15.0f;
		public float offsetY = 15.0f;

		public float sizeX = 480;
		public float sizeY = 320;

		private SubviewPositionType preSubviewPositionType;

		// Use this for initialization
		void Start ()
		{
			SubviewManager.SetSubviewCamera(this.subviewType, this.subviewCamera);

			SubviewManager.SetSubviewPosition(this.subviewType, this.subviewPositionType, this.offsetX, this.offsetY, this.sizeX, this.sizeY);

			this.preSubviewPositionType = this.subviewPositionType;
		}
	
		// Update is called once per frame
		void Update ()
		{
			if(this.subviewPositionType != this.preSubviewPositionType)
			{
				SubviewManager.SetSubviewPosition(this.subviewType, this.subviewPositionType, this.offsetX, this.offsetY, this.sizeX, this.sizeY);

				this.preSubviewPositionType = this.subviewPositionType;
			}
		}
	}
}
