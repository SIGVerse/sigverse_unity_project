using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using SIGVerse.Common;

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
		public float strideMax = 0.3f;
		//////////////////////////////

		private Animator animator;

		private Transform[]  fixedTransforms;
		private Quaternion[] fixedQuaternionsOrg;

		private float preHorizontal = 0.0f;
		private float preVertical   = 0.0f;

		private InputDevice leftHandDevice;

		protected virtual void Awake()
		{
			this.animator = GetComponent<Animator>();

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

			StartCoroutine(GetXrDevice(XRNode.LeftHand));
		}

		private IEnumerator GetXrDevice(XRNode xrNode)
		{
			yield return StartCoroutine(SIGVerseUtils.GetXrDevice(xrNode, x => this.leftHandDevice = x));
		}

		protected virtual (float, float) GetInput()
		{
			// Read inputs
			if(this.leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftHandStickValue))
			{
				return (leftHandStickValue.x, leftHandStickValue.y);
			}

			return (0f, 0f);
		}

		protected virtual void Update()
		{
			if(Time.time > 0.1f && this.eyeAnchor.up.y > 0.1f)
			{
				this.animator.applyRootMotion = true;
			}
			else
			{
				this.animator.applyRootMotion = false;
			}

			(float horizontal, float vertical) = this.GetInput();

			(horizontal, vertical) = this.GetRestrictedInput(horizontal, vertical);

			if (Mathf.Abs(horizontal) > 0.01 || Mathf.Abs(vertical) > 0.01)
			{
				Vector3 destination = vertical * this.transform.forward + horizontal * this.transform.right;

				this.Move(destination, this.moveSpeedByController);

				Vector3 newVrRoot = this.animator.rootPosition - (this.bodyAnchor.position - this.vrRoot.transform.position);

				this.vrRoot.transform.position = new Vector3(newVrRoot.x, 0.0f, newVrRoot.z);
			}
			else
			{
				Vector3 destination = new Vector3(this.bodyAnchor.position.x - this.transform.position.x, 0, this.bodyAnchor.position.z - this.transform.position.z);

				this.Move(destination, this.moveSpeedByHmd);

				this.transform.rotation = this.bodyAnchor.rotation;

				this.vrRoot.transform.position = new Vector3(this.vrRoot.transform.position.x, 0.0f, this.vrRoot.transform.position.z);
			}

			this.preHorizontal = horizontal;
			this.preVertical   = vertical;
		}

		private (float, float) GetRestrictedInput(float horizontal, float vertical)
		{
			float maxSpeedInc = this.moveSpeedByController / 10f;

			if (Mathf.Abs(horizontal - this.preHorizontal) > maxSpeedInc) { horizontal = horizontal > this.preHorizontal ? this.preHorizontal + maxSpeedInc : this.preHorizontal - maxSpeedInc; }
			if (Mathf.Abs(vertical   - this.preVertical)   > maxSpeedInc) { vertical   = vertical   > this.preVertical   ? this.preVertical   + maxSpeedInc : this.preVertical   - maxSpeedInc; }

			return (horizontal, vertical);
		}

		private void Move(Vector3 move, float animSpeedMultiplier)
		{
			if (move.magnitude > 1f) move.Normalize();

			move = this.transform.InverseTransformDirection(move);

			move = Vector3.ProjectOnPlane(move, Vector3.up);

			this.animator.SetFloat("Forward", Mathf.Clamp(move.z, -this.strideMax, +this.strideMax), 0.01f, Time.deltaTime);
			this.animator.SetFloat("Turn",    Mathf.Clamp(move.x, -this.strideMax, +this.strideMax), 0.01f, Time.deltaTime);

			this.animator.speed = animSpeedMultiplier;
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

