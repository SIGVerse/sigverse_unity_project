using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SIGVerse.ExampleScenes.OvrHandTracking
{
	public class AvatarMotionCopier : MonoBehaviour
	{
		public Animator sourceAnim;
		public Animator targetAnim;

		[TooltipAttribute("degree")]
		public float additionalRotationY;

		//-----------
		private List<(Transform, Transform)> updateTransforms = new List<(Transform, Transform)>();

		void Start()
		{
			foreach(HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
			{
				if(bone==HumanBodyBones.LastBone){ continue; }

				Transform sourceTransform = this.sourceAnim.GetBoneTransform(bone);
				Transform targetTransform = this.targetAnim.GetBoneTransform(bone);

				if(sourceTransform!=null && targetTransform!=null)
				{
					this.updateTransforms.Add((sourceTransform, targetTransform));
				}
			}
		}

		void LateUpdate()
		{
			foreach((Transform sourceTransform, Transform targetTransform) in this.updateTransforms)
			{
				targetTransform.localPosition = sourceTransform.localPosition;
				targetTransform.localRotation = sourceTransform.localRotation;
			}

			this.targetAnim.transform.root.localRotation = this.sourceAnim.transform.root.localRotation;
			this.targetAnim.transform.root.transform.Rotate(Vector3.up, additionalRotationY, Space.World);
		}
	}
}

