using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Human.IK
{
	[RequireComponent(typeof(Animator))]
	public class SimpleIK : MonoBehaviour
	{
		[HeaderAttribute ("IK Effectors")]
		public Transform bodyAnchor;
		public Transform lookAtAnchor;
		public Transform leftHandAnchor;
		public Transform rightHandAnchor;
		public Transform leftFootAnchor;
		public Transform rightFootAnchor;
		public Transform leftElbowAnchor;
		public Transform rightElbowAnchor;
		public Transform leftKneeAnchor;
		public Transform rightKneeAnchor;


		[HeaderAttribute ("IK weights")]
		public float lookAtWeight = 1.0f;

		public float leftHandWeightPosition = 1.0f;
		public float leftHandWeightRotation = 1.0f;
	
		public float rightHandWeightPosition = 1.0f;
		public float rightHandWeightRotation = 1.0f;

		public float leftFootWeightPosition = 1.0f;
		public float leftFootWeightRotation = 1.0f;

		public float rightFootWeightPosition = 1.0f;
		public float rightFootWeightRotation = 1.0f;

		public float leftElbowWeight  = 0.3f;
		public float rightElbowWeight = 0.3f;

		public float leftKneeWeight  = 0.3f;
		public float rightKneeWeight = 0.3f;

		/////////////////////////

		private Animator animator;

		// Use this for initialization
		void Start ()
		{
			this.animator = GetComponent<Animator>();
		}


		void OnAnimatorIK(int layerIndex)
		{
			if (this.animator==null) { return; }

//			this.animator.SetLookAtWeight(this.lookAtWeight,0.3f,0.6f,1.0f,0.5f);
			this.animator.SetLookAtWeight(this.lookAtWeight);

			this.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, this.leftHandWeightPosition);
			this.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, this.leftHandWeightRotation);

			this.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, this.rightHandWeightPosition);
			this.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, this.rightHandWeightRotation);

			this.animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, this.leftFootWeightPosition);
			this.animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, this.leftFootWeightRotation);

			this.animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, this.rightFootWeightPosition);
			this.animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, this.rightFootWeightRotation);

			if (this.bodyAnchor != null)
			{
				this.animator.rootPosition = new Vector3(this.bodyAnchor.position.x, 0.0f, this.bodyAnchor.position.z);
				this.animator.rootRotation = this.bodyAnchor.rotation;

				this.animator.bodyPosition = this.bodyAnchor.position;
				this.animator.bodyRotation = this.bodyAnchor.rotation;
			}

			if (this.lookAtAnchor != null)
			{
				this.animator.SetLookAtPosition(this.lookAtAnchor.position);
			}

			if (this.leftHandAnchor != null)
			{
				this.animator.SetIKPosition(AvatarIKGoal.LeftHand, this.leftHandAnchor.position);
				this.animator.SetIKRotation(AvatarIKGoal.LeftHand, this.leftHandAnchor.rotation);
			}
			
			if(this.rightHandAnchor != null)
			{
				this.animator.SetIKPosition(AvatarIKGoal.RightHand, this.rightHandAnchor.position);
				this.animator.SetIKRotation(AvatarIKGoal.RightHand, this.rightHandAnchor.rotation);
			}

			if(this.leftFootAnchor != null)
			{
//				this.animator.SetIKPosition(AvatarIKGoal.LeftFoot, this.leftFootAnchor.position);
				this.animator.SetIKRotation(AvatarIKGoal.LeftFoot, this.leftFootAnchor.rotation);
			}
			
			if(this.rightFootAnchor != null)
			{
//				this.animator.SetIKPosition(AvatarIKGoal.RightFoot, this.rightFootAnchor.position);
				this.animator.SetIKRotation(AvatarIKGoal.RightFoot, this.rightFootAnchor.rotation);
			}

			if(this.leftElbowAnchor != null)
			{
				this.animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow,  this.leftElbowWeight);
				this.animator.SetIKHintPosition(AvatarIKHint.LeftElbow, this.leftElbowAnchor.position);
			}

			if(this.rightElbowAnchor != null)
			{
				this.animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, this.rightElbowWeight);
				this.animator.SetIKHintPosition(AvatarIKHint.RightElbow, this.rightElbowAnchor.position);
			}

			if(this.leftKneeAnchor != null)
			{
				this.animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee,  this.leftKneeWeight);
				this.animator.SetIKHintPosition(AvatarIKHint.LeftKnee, this.leftKneeAnchor.position);
			}

			if(this.rightKneeAnchor != null)
			{
				this.animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, this.rightKneeWeight);
				this.animator.SetIKHintPosition(AvatarIKHint.RightKnee, this.rightKneeAnchor.position);
			}
		}
	}
}

