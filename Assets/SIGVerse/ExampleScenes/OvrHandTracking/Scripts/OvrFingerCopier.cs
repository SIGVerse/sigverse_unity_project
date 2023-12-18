using Newtonsoft.Json;
using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SIGVerse.ExampleScenes.OvrHandTracking
{
	public class OvrFingerCopier : MonoBehaviour
	{
		public Transform ovrLeftHand;
		public Transform ovrRightHand;

		public string fileName = "OvrHandConversionAngleForVRoid";

		//-----------

		private bool isHandAppearing = false;

		private Dictionary<HumanBodyBones, Transform> avatarTransformMap;
		private Dictionary<HumanBodyBones, Transform> ovrTransformMap;

		private Dictionary<HumanBodyBones, Quaternion> conversionAngleMap;

		void Start()
		{
			this.conversionAngleMap = OvrHandTrackingUtils.GetJsonData(this.fileName);

			this.avatarTransformMap = OvrHandTrackingUtils.GetAvatarFingerTransformMap(this.GetComponent<Animator>());

			StartCoroutine(this.SetOvrTransformMapCoroutine());
		}


		private IEnumerator SetOvrTransformMapCoroutine()
		{
			while(!this.isHandAppearing)
			{
				if(this.ovrLeftHand.childCount>0 && this.ovrRightHand.childCount>0)
				{
					this.ovrTransformMap = OvrHandTrackingUtils.GetOvrTransformMap(this.ovrLeftHand, this.ovrRightHand);

					this.isHandAppearing = true;
				}

				yield return new WaitForSeconds(1.0f);
			}
		}

		void LateUpdate()
		{
			if (!this.isHandAppearing) { return; }

			foreach(HumanBodyBones bone in this.avatarTransformMap.Keys)
			{
				this.avatarTransformMap[bone].rotation = this.ovrTransformMap[bone].rotation * this.conversionAngleMap[bone];
			}
		}
	}
}

