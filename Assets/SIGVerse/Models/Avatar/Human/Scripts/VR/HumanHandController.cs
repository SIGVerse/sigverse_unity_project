using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace SIGVerse.Common
{
	public class HumanHandController : MonoBehaviour
	{
		public enum HandType
		{
			Left,
			Right,
		}

		public HandType   handType;
		public GameObject avatarBase;
		public GameObject avatarPointing;

		//-----------
		protected Transform thumb1, index1, middle1, ring1, little1;
		protected Transform thumb2, index2, middle2, ring2, little2;
		protected Transform thumb3, index3, middle3, ring3, little3;

		protected Quaternion thumb1Start, thumb2Start, thumb3Start;
		protected Quaternion index1Start, index2Start, index3Start;
		protected Quaternion middle1Start, middle2Start, middle3Start;
		protected Quaternion ring1Start, ring2Start, ring3Start;
		protected Quaternion little1Start, little2Start, little3Start;

		protected Quaternion thumb1End , thumb2End , thumb3End;
		protected Quaternion index1End , index2End , index3End;
		protected Quaternion middle1End, middle2End, middle3End;
		protected Quaternion ring1End  , ring2End  , ring3End;
		protected Quaternion little1End, little2End, little3End;

		protected InputDevice handDevice;

		private float handPostureRatio;

		public virtual void Awake()
		{
		}

		public virtual void Start()
		{
			if(this.handType==HandType.Left) { StartCoroutine(GetXrDevice(XRNode.LeftHand)); }
			if(this.handType==HandType.Right){ StartCoroutine(GetXrDevice(XRNode.RightHand)); }

			InitializeFingerPostures();
		}

		protected virtual IEnumerator GetXrDevice(XRNode xrNode)
		{
			yield return StartCoroutine(SIGVerseUtils.GetXrDevice(xrNode, x => this.handDevice = x));
		}

		protected virtual void InitializeFingerPostures()
		{
			Transform[] baseTransforms;

			GameObject avatarBaseInstance     = Instantiate(this.avatarBase,     Vector3.zero, Quaternion.identity);  // If not instantiated, GetBoneTransform returns null. https://issuetracker.unity3d.com/issues/animator-dot-getbonetransform-doesnt-get-bones-transform-from-prefab
			GameObject avatarPointingInstance = Instantiate(this.avatarPointing, Vector3.zero, Quaternion.identity);  // If not instantiated, GetBoneTransform returns null.

			this.GetFingerRotations(out baseTransforms, avatarBaseInstance.GetComponent<Animator>());

			this.thumb1Start  = baseTransforms[ 0].localRotation; this.thumb2Start  = baseTransforms[ 1].localRotation; this.thumb3Start  = baseTransforms[ 2].localRotation;
			this.index1Start  = baseTransforms[ 3].localRotation; this.index2Start  = baseTransforms[ 4].localRotation; this.index3Start  = baseTransforms[ 5].localRotation;
			this.middle1Start = baseTransforms[ 6].localRotation; this.middle2Start = baseTransforms[ 7].localRotation; this.middle3Start = baseTransforms[ 8].localRotation;
			this.ring1Start   = baseTransforms[ 9].localRotation; this.ring2Start   = baseTransforms[10].localRotation; this.ring3Start   = baseTransforms[11].localRotation;
			this.little1Start = baseTransforms[12].localRotation; this.little2Start = baseTransforms[13].localRotation; this.little3Start = baseTransforms[14].localRotation;

			Transform[] pointingTransforms;

			this.GetFingerRotations(out pointingTransforms, avatarPointingInstance.GetComponent<Animator>());

			this.thumb1End  = pointingTransforms[ 0].localRotation; this.thumb2End  = pointingTransforms[ 1].localRotation; this.thumb3End  = pointingTransforms[ 2].localRotation;
			this.index1End  = pointingTransforms[ 3].localRotation; this.index2End  = pointingTransforms[ 4].localRotation; this.index3End  = pointingTransforms[ 5].localRotation;
			this.middle1End = pointingTransforms[ 6].localRotation; this.middle2End = pointingTransforms[ 7].localRotation; this.middle3End = pointingTransforms[ 8].localRotation;
			this.ring1End   = pointingTransforms[ 9].localRotation; this.ring2End   = pointingTransforms[10].localRotation; this.ring3End   = pointingTransforms[11].localRotation;
			this.little1End = pointingTransforms[12].localRotation; this.little2End = pointingTransforms[13].localRotation; this.little3End = pointingTransforms[14].localRotation;

			Destroy(avatarBaseInstance);
			Destroy(avatarPointingInstance);

			Transform[] targetTransforms;

			this.GetFingerRotations(out targetTransforms, this.GetComponentInChildren<Animator>());

			this.thumb1  = targetTransforms[ 0]; this.thumb2  = targetTransforms[ 1]; this.thumb3  = targetTransforms[ 2];
			this.index1  = targetTransforms[ 3]; this.index2  = targetTransforms[ 4]; this.index3  = targetTransforms[ 5];
			this.middle1 = targetTransforms[ 6]; this.middle2 = targetTransforms[ 7]; this.middle3 = targetTransforms[ 8];
			this.ring1   = targetTransforms[ 9]; this.ring2   = targetTransforms[10]; this.ring3   = targetTransforms[11];
			this.little1 = targetTransforms[12]; this.little2 = targetTransforms[13]; this.little3 = targetTransforms[14];
		}

		protected virtual void GetFingerRotations(out Transform[] transforms, Animator animator)
		{
			transforms = new Transform[15];

			if (this.handType==HandType.Left)
			{
				transforms[ 0] = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
				transforms[ 1] = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
				transforms[ 2] = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

				transforms[ 3] = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
				transforms[ 4] = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
				transforms[ 5] = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

				transforms[ 6] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
				transforms[ 7] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
				transforms[ 8] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

				transforms[ 9] = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
				transforms[10] = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
				transforms[11] = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);

				transforms[12] = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
				transforms[13] = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
				transforms[14] = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
			}
			else
			{
				transforms[ 0] = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
				transforms[ 1] = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
				transforms[ 2] = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);

				transforms[ 3] = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
				transforms[ 4] = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
				transforms[ 5] = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

				transforms[ 6] = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
				transforms[ 7] = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
				transforms[ 8] = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

				transforms[ 9] = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
				transforms[10] = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
				transforms[11] = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);

				transforms[12] = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
				transforms[13] = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
				transforms[14] = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
			}
		}

		public void LateUpdate()
		{
			float ratio = GetHandPostureRatio();

			// Change hand posture
			this.thumb1.localRotation = Quaternion.Slerp(this.thumb1Start, this.thumb1End, ratio);
			this.thumb2.localRotation = Quaternion.Slerp(this.thumb2Start, this.thumb2End, ratio);
			this.thumb3.localRotation = Quaternion.Slerp(this.thumb3Start, this.thumb3End, ratio);

			this.index1.localRotation = Quaternion.Slerp(this.index1Start, this.index1End, ratio);
			this.index2.localRotation = Quaternion.Slerp(this.index2Start, this.index2End, ratio);
			this.index3.localRotation = Quaternion.Slerp(this.index3Start, this.index3End, ratio);

			this.middle1.localRotation = Quaternion.Slerp(this.middle1Start, this.middle1End, ratio);
			this.middle2.localRotation = Quaternion.Slerp(this.middle2Start, this.middle2End, ratio);
			this.middle3.localRotation = Quaternion.Slerp(this.middle3Start, this.middle3End, ratio);

			this.ring1.localRotation = Quaternion.Slerp(this.ring1Start, this.ring1End, ratio);
			this.ring2.localRotation = Quaternion.Slerp(this.ring2Start, this.ring2End, ratio);
			this.ring3.localRotation = Quaternion.Slerp(this.ring3Start, this.ring3End, ratio);

			this.little1.localRotation = Quaternion.Slerp(this.little1Start, this.little1End, ratio);
			this.little2.localRotation = Quaternion.Slerp(this.little2Start, this.little2End, ratio);
			this.little3.localRotation = Quaternion.Slerp(this.little3Start, this.little3End, ratio);
		}

		protected virtual float GetHandPostureRatio()
		{
			if(this.handDevice.TryGetFeatureValue(CommonUsages.grip, out float handTriggerValue))
			{
				this.handPostureRatio = handTriggerValue;
			}

			return this.handPostureRatio;
		}
	}
}

