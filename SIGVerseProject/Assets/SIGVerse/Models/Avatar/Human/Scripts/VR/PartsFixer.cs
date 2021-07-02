using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets_1_1_2.CrossPlatformInput;

namespace SIGVerse.Human.VR
{
	public class PartsFixer : MonoBehaviour
	{
		public List<Transform> fixedParts;

		////////////////////////////////////
		 
		private Transform[]  fixedTransforms;
		private Quaternion[] fixedQuaternionsOrg;

		protected virtual void Awake()
		{
			List<Transform> fixedTransformList = new List<Transform>();

			foreach (Transform fixedPart in this.fixedParts)
			{
				fixedTransformList.AddRange(fixedPart.GetComponentsInChildren<Transform>());
			}

			this.fixedTransforms = fixedTransformList.ToArray();
		}

		protected virtual void Start()
		{
			this.fixedQuaternionsOrg = new Quaternion[this.fixedTransforms.Length];

			for (int i=0; i<this.fixedTransforms.Length; i++)
			{
				this.fixedQuaternionsOrg[i] = this.fixedTransforms[i].localRotation;
			}
		}

		// Update is called once per frame
		protected virtual void LateUpdate()
		{
			for (int i=0; i<this.fixedTransforms.Length; i++)
			{
				this.fixedTransforms[i].localRotation = this.fixedQuaternionsOrg[i];
			}
		}
	}
}

