using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SIGVerse.SampleScenes.Hsr
{
	public class CleanupAvatarVRHandController : MonoBehaviour
	{
		private const float PointingDuration = 3.0f;

		public enum HandType
		{
			LeftHand,
			RightHand,
		}

		private enum State
		{
			Wait,
			PointTarget,
			PointDestination,
			Return,
		}

		public HandType  handType;

		//-----------
		private State state = State.Wait;

		private Animator animator;

		private AvatarIKGoal avatarIkGoalHand;

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
		private GameObject destination;

		private float pointingRatio = 0.0f;

		void Awake()
		{
			this.animator = this.GetComponent<Animator>();

			if(this.handType == HandType.LeftHand)
			{
				this.avatarIkGoalHand = AvatarIKGoal.LeftHand;

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
				this.avatarIkGoalHand = AvatarIKGoal.RightHand;

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
			if(this.state == State.PointTarget || this.state == State.PointDestination)
			{
				this.pointingRatio = Mathf.Min(1.0f, this.pointingRatio + Time.deltaTime / PointingDuration);
			}
			if (this.state == State.Return)
			{
				this.pointingRatio = Mathf.Max(0.0f, this.pointingRatio - Time.deltaTime / PointingDuration);
			}
		}

		public void PointTarget(GameObject graspingTarget)
		{
			this.graspingTarget = graspingTarget;

			this.SetState(State.PointTarget, 0.0f);
		}

		public void PointDestination(GameObject destination)
		{
			this.destination = destination;

			this.SetState(State.PointDestination, 0.0f);
		}

		public void Return()
		{
			this.SetState(State.Return, 1.0f);
		}

		public void Wait()
		{
			this.SetState(State.Wait, 0.0f);
		}

		private void SetState(State state, float pointingRatio)
		{
			this.state = state;

			this.pointingRatio = pointingRatio;
		}


		void OnAnimatorIK()
		{
			if(this.state == State.Wait) { return; }

			// Move arm
			if (this.state == State.PointTarget)
			{
				this.animator.SetIKPositionWeight(this.avatarIkGoalHand, this.pointingRatio);
				this.animator.SetIKRotationWeight(this.avatarIkGoalHand, this.pointingRatio);
				this.animator.SetIKPosition(this.avatarIkGoalHand, this.graspingTarget.transform.position);
				this.animator.SetIKRotation(this.avatarIkGoalHand, Quaternion.LookRotation(this.graspingTarget.transform.position - this.upperArm.position));

				this.animator.SetLookAtWeight(Mathf.Min(this.pointingRatio * 2, 0.7f), Mathf.Min(this.pointingRatio * 2, 0.7f));
				this.animator.SetLookAtPosition(this.graspingTarget.transform.position);
			}
			if (this.state == State.PointDestination)
			{
				Vector3 pos = (1.0f - this.pointingRatio) * this.graspingTarget.transform.position + this.pointingRatio * this.destination.transform.position;

				this.animator.SetIKPositionWeight(this.avatarIkGoalHand, 1.0f);
				this.animator.SetIKRotationWeight(this.avatarIkGoalHand, 1.0f);
				this.animator.SetIKPosition(this.avatarIkGoalHand, pos);
				this.animator.SetIKRotation(this.avatarIkGoalHand, Quaternion.LookRotation(pos - this.upperArm.position));

				this.animator.SetLookAtWeight(0.7f, 0.7f);
				this.animator.SetLookAtPosition(pos);
			}
			if (this.state == State.Return)
			{
				this.animator.SetIKPositionWeight(this.avatarIkGoalHand, this.pointingRatio);
				this.animator.SetIKRotationWeight(this.avatarIkGoalHand, this.pointingRatio);
				this.animator.SetIKPosition(this.avatarIkGoalHand, this.destination.transform.position);
				this.animator.SetIKRotation(this.avatarIkGoalHand, Quaternion.LookRotation(this.destination.transform.position - this.upperArm.position));

				this.animator.SetLookAtWeight(Mathf.Min(this.pointingRatio * 2, 0.7f), Mathf.Min(this.pointingRatio * 2, 0.7f));
				this.animator.SetLookAtPosition(this.destination.transform.position);
			}
		}

		private void LateUpdate()
		{
			if(this.state == State.Wait) { return; }

			float ratio = this.state != State.PointDestination ? this.pointingRatio : 1.0f;

			// Change hand posture
			this.thumb1.localRotation = Quaternion.Slerp(this.thumb1.localRotation, this.thumb1End, ratio);
			this.thumb2.localRotation = Quaternion.Slerp(this.thumb2.localRotation, this.thumb2End, ratio);
			this.thumb3.localRotation = Quaternion.Slerp(this.thumb3.localRotation, this.thumb3End, ratio);

			this.index1.localRotation = Quaternion.Slerp(this.index1.localRotation, this.index1End, ratio);
			this.index2.localRotation = Quaternion.Slerp(this.index2.localRotation, this.index2End, ratio);
			this.index3.localRotation = Quaternion.Slerp(this.index3.localRotation, this.index3End, ratio);

			this.middle1.localRotation = Quaternion.Slerp(this.middle1.localRotation, this.middle1End, ratio);
			this.middle2.localRotation = Quaternion.Slerp(this.middle2.localRotation, this.middle2End, ratio);
			this.middle3.localRotation = Quaternion.Slerp(this.middle3.localRotation, this.middle3End, ratio);

			this.ring1.localRotation = Quaternion.Slerp(this.ring1.localRotation, this.ring1End, ratio);
			this.ring2.localRotation = Quaternion.Slerp(this.ring2.localRotation, this.ring2End, ratio);
			this.ring3.localRotation = Quaternion.Slerp(this.ring3.localRotation, this.ring3End, ratio);

			this.little1.localRotation = Quaternion.Slerp(this.little1.localRotation, this.little1End, ratio);
			this.little2.localRotation = Quaternion.Slerp(this.little2.localRotation, this.little2End, ratio);
			this.little3.localRotation = Quaternion.Slerp(this.little3.localRotation, this.little3End, ratio);
		}

		public bool IsWaiting()
		{
			return (this.state == State.PointTarget      && this.pointingRatio >= 1.0f) || 
			       (this.state == State.PointDestination && this.pointingRatio >= 1.0f) ||
			       (this.state == State.Return           && this.pointingRatio <= 0.0f);
		}
	}
}

