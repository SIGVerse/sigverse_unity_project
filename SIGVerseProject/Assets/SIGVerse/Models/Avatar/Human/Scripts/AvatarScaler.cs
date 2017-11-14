using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Human
{
	public class AvatarScaler : MonoBehaviour
	{
		[TooltipAttribute("[m]")]
		public float userHeight = 1.7f;

		[HeaderAttribute ("Avatar Info")]
		public Transform avatar;

		[TooltipAttribute("[m]")]
		public float avatarHeight = 1.5f;


		[HeaderAttribute("Object list proportional to the height of the avatar")]
		public Transform[] objectsProportionalToHeight;


		////////////////////

		private float preUserHeight;

		private Vector3   orgAvatarScale;
		private Vector3[] orgObjectsScale;

		// Use this for initialization
		void Start ()
		{
			this.orgAvatarScale = avatar.localScale;

			Array.Resize(ref this.orgObjectsScale, this.objectsProportionalToHeight.Length);

			for(int i=0; i<this.objectsProportionalToHeight.Length; i++)
			{
				this.orgObjectsScale[i] = this.objectsProportionalToHeight[i].localScale;
			}

			this.ChangeScale();
			this.preUserHeight = this.userHeight;
		}
	
		// Update is called once per frame
		void Update ()
		{
			if(this.preUserHeight!=this.userHeight)
			{
				SIGVerse.Common.SIGVerseLogger.Info("Changed Avatar scale. " + this.preUserHeight + "->" + this.userHeight);

				this.ChangeScale();
				this.preUserHeight = this.userHeight;
			}
		}

		private void ChangeScale()
		{
			// Change avatar scale
			this.avatar.localScale = this.orgAvatarScale * this.userHeight / this.avatarHeight;

			for(int i=0; i<this.objectsProportionalToHeight.Length; i++)
			{
				this.objectsProportionalToHeight[i].localScale = this.orgObjectsScale[i] * this.userHeight / this.avatarHeight;
			}
		}
	}
}

