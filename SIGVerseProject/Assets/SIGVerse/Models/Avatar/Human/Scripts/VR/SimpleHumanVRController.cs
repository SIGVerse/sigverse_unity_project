using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets_1_1_2.CrossPlatformInput;

namespace SIGVerse.Human.VR
{
	public class SimpleHumanVRController : MonoBehaviour
	{
		public Transform vrRoot;
		public Transform eyeAnchor;
		public Transform bodyAnchor;

		public List<Transform> fixedParts;

		public float moveSpeedByController = 1.0f;
		public float moveSpeedByHmd        = 2.0f;
		//////////////////////////////

		private Animator animator;

		private Transform[]  fixedTransforms;
		private Quaternion[] fixedQuaternionsOrg;

		private void Awake()
		{
			this.animator = GetComponent<Animator>();

			List<Transform> fixedTransformList = new List<Transform>();

			foreach (Transform fixedPart in this.fixedParts)
			{
				fixedTransformList.AddRange(fixedPart.GetComponentsInChildren<Transform>());
			}

			this.fixedTransforms = fixedTransformList.ToArray();
		}

		private void Start()
		{
			this.fixedQuaternionsOrg = new Quaternion[this.fixedTransforms.Length];

			for (int i=0; i<this.fixedTransforms.Length; i++)
			{
				this.fixedQuaternionsOrg[i] = this.fixedTransforms[i].localRotation;
			}
		}

		private void Update()
		{
			if(Time.time > 0.1f && this.eyeAnchor.up.y > 0.1f)
			{
				this.animator.applyRootMotion = true;
			}
			else
			{
				this.animator.applyRootMotion = false;
			}

			// read inputs
			float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			float vertical   = CrossPlatformInputManager.GetAxis("Vertical");

			if(Mathf.Abs(horizontal) > 0.01 || Mathf.Abs(vertical) > 0.01)
			{
				Vector3 destination = vertical * this.transform.forward + horizontal * this.transform.right;

				this.Move(destination  * this.moveSpeedByController, this.moveSpeedByController);

				Vector3 newVrRoot = this.animator.rootPosition - (this.bodyAnchor.position - this.vrRoot.transform.position);

				this.vrRoot.transform.position = new Vector3(newVrRoot.x, 0.0f, newVrRoot.z);
			}
			else
			{
				Vector3 destination = new Vector3(this.bodyAnchor.position.x - this.transform.position.x, 0, this.bodyAnchor.position.z - this.transform.position.z);

				this.Move(destination, this.moveSpeedByHmd);
				
				this.transform.rotation = this.bodyAnchor.rotation;
			}
		}

		private void Move(Vector3 move, float animSpeedMultiplier)
		{
			if (move.magnitude > 1f) move.Normalize();

			move = this.transform.InverseTransformDirection(move);

			move = Vector3.ProjectOnPlane(move, Vector3.up);

			this.animator.SetFloat("Forward", move.z, 0.01f, Time.deltaTime);
			this.animator.SetFloat("Turn",    move.x, 0.01f, Time.deltaTime);

			this.animator.speed = animSpeedMultiplier;
		}

		// Update is called once per frame
		void LateUpdate()
		{
			for (int i=0; i<this.fixedTransforms.Length; i++)
			{
				this.fixedTransforms[i].localRotation = this.fixedQuaternionsOrg[i];
			}
		}
	}
}

