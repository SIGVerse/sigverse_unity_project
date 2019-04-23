using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SIGVerse.SampleScenes.Hsr
{
	public class CleanupAvatarVRHandController : MonoBehaviour
	{
		public enum HandType
		{
			LeftHand,
			RightHand,
		}

		public HandType  handType;

		private Animator animator;

		private AvatarIKGoal avatarIkGoal;

		//-----------
		private Transform upperArm;

		private Transform thumb1, index1, middle1, ring1, little1;
		private Transform thumb2, index2, middle2, ring2, little2;
		private Transform thumb3, index3, middle3, ring3, little3;

		private Quaternion thumb1End , thumb2End , thumb3End;
		private Quaternion index1End , index2End , index3End;
		private Quaternion middle1End, middle2End, middle3End;
		private Quaternion ring1End  , ring2End  , ring3End;
		private Quaternion little1End, little2End, little3End;

		private GameObject graspingTarget;

		private float? pointingRatio = null;

		void Awake()
		{
			this.animator = GetComponent<Animator>();

			if(this.handType == HandType.LeftHand)
			{
				this.avatarIkGoal = AvatarIKGoal.LeftHand;

				this.upperArm = this.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

				this.thumb1 = this.animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
				this.thumb2 = this.animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
				this.thumb3 = this.animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

				this.index1 = this.animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
				this.index2 = this.animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
				this.index3 = this.animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

				this.middle1 = this.animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
				this.middle2 = this.animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
				this.middle3 = this.animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

				this.ring1 = this.animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
				this.ring2 = this.animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
				this.ring3 = this.animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);

				this.little1 = this.animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
				this.little2 = this.animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
				this.little3 = this.animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
			}
			else
			{
				this.avatarIkGoal = AvatarIKGoal.RightHand;

				this.upperArm = this.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

				this.thumb1 = this.animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
				this.thumb2 = this.animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
				this.thumb3 = this.animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);

				this.index1 = this.animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
				this.index2 = this.animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
				this.index3 = this.animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

				this.middle1 = this.animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
				this.middle2 = this.animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
				this.middle3 = this.animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

				this.ring1 = this.animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
				this.ring2 = this.animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
				this.ring3 = this.animator.GetBoneTransform(HumanBodyBones.RightRingDistal);

				this.little1 = this.animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
				this.little2 = this.animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
				this.little3 = this.animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
			}
		}


		// Use this for initialization
		void Start()
		{
			float xySign = (this.handType == HandType.LeftHand)? 1.0f : -1.0f;

			this.thumb1End  = Quaternion.Euler(xySign*(+63), xySign*(-102), -53);
			this.thumb2End  = Quaternion.Euler(xySign*(- 6), xySign*(- 24), +24);
			this.thumb3End  = Quaternion.Euler(xySign*(  1), xySign*(-  3), +14);

			this.index1End  = Quaternion.Euler(xySign*(-10), xySign*(+10), -16);
			this.index2End  = Quaternion.Euler(xySign*( 0), xySign*(  0), -30);
			this.index3End  = Quaternion.Euler(xySign*(+2), xySign*(+ 5), -30);

			this.middle1End = Quaternion.Euler(xySign*(+ 2), xySign*(+12), +60);
			this.middle2End = Quaternion.Euler(xySign*(+34), xySign*(-34), +26);
			this.middle3End = Quaternion.Euler(xySign*(  0), xySign*(  0), +19);

			this.ring1End   = Quaternion.Euler(xySign*(+33), xySign*(- 2), +29);
			this.ring2End   = Quaternion.Euler(xySign*(+25), xySign*(+13), +29);
			this.ring3End   = Quaternion.Euler(xySign*(+17), xySign*(+ 5), +14);

			this.little1End = Quaternion.Euler(xySign*(+34), xySign*(+17), + 4);
			this.little2End = Quaternion.Euler(xySign*(+47), xySign*(+30), +19);
			this.little3End = Quaternion.Euler(xySign*(-20), xySign*(+41), +12);
		}

		private void Update()
		{
			if(this.pointingRatio!=null)
			{
				this.pointingRatio = Mathf.Min(1.0f, (float)this.pointingRatio+Time.deltaTime);
			}
		}

		public void PointTargetObject(GameObject graspingTarget)
		{
			this.graspingTarget = graspingTarget;

			this.pointingRatio = 0.0f;
		}


		void OnAnimatorIK()
		{
			if(this.pointingRatio==null){ return; }

			this.animator.SetIKPositionWeight(this.avatarIkGoal, (float)this.pointingRatio);
			this.animator.SetIKRotationWeight(this.avatarIkGoal, (float)this.pointingRatio);
			this.animator.SetIKPosition(this.avatarIkGoal, this.graspingTarget.transform.position);
			this.animator.SetIKRotation(this.avatarIkGoal, Quaternion.LookRotation(this.graspingTarget.transform.position - this.upperArm.position));
		}

		private void LateUpdate()
		{
			if(this.pointingRatio==null){ return; }

			// Change hand posture
			this.thumb1 .localRotation = Quaternion.Slerp(this.thumb1 .localRotation, this.thumb1End , (float)this.pointingRatio);
			this.thumb2 .localRotation = Quaternion.Slerp(this.thumb2 .localRotation, this.thumb2End , (float)this.pointingRatio);
			this.thumb3 .localRotation = Quaternion.Slerp(this.thumb3 .localRotation, this.thumb3End , (float)this.pointingRatio);

			this.index1 .localRotation = Quaternion.Slerp(this.index1 .localRotation, this.index1End , (float)this.pointingRatio);
			this.index2 .localRotation = Quaternion.Slerp(this.index2 .localRotation, this.index2End , (float)this.pointingRatio);
			this.index3 .localRotation = Quaternion.Slerp(this.index3 .localRotation, this.index3End , (float)this.pointingRatio);

			this.middle1.localRotation = Quaternion.Slerp(this.middle1.localRotation, this.middle1End, (float)this.pointingRatio);
			this.middle2.localRotation = Quaternion.Slerp(this.middle2.localRotation, this.middle2End, (float)this.pointingRatio);
			this.middle3.localRotation = Quaternion.Slerp(this.middle3.localRotation, this.middle3End, (float)this.pointingRatio);

			this.ring1  .localRotation = Quaternion.Slerp(this.ring1  .localRotation, this.ring1End  , (float)this.pointingRatio);
			this.ring2  .localRotation = Quaternion.Slerp(this.ring2  .localRotation, this.ring2End  , (float)this.pointingRatio);
			this.ring3  .localRotation = Quaternion.Slerp(this.ring3  .localRotation, this.ring3End  , (float)this.pointingRatio);

			this.little1.localRotation = Quaternion.Slerp(this.little1.localRotation, this.little1End, (float)this.pointingRatio);
			this.little2.localRotation = Quaternion.Slerp(this.little2.localRotation, this.little2End, (float)this.pointingRatio);
			this.little3.localRotation = Quaternion.Slerp(this.little3.localRotation, this.little3End, (float)this.pointingRatio);
		}
	}
}

