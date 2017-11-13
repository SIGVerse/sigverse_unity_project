using System;
using UnityEngine;
using UnityStandardAssets_1_1_2.CrossPlatformInput;

namespace SIGVerse.Human.VR
{
	public class SimpleHumanVRController : MonoBehaviour
	{
		public Transform vrRoot;
		public Transform eyeAnchor;
		public Transform bodyAnchor;

		//////////////////////////////

		private Animator animator;

		private void Start()
		{
			this.animator = GetComponent<Animator>();
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

			if(Mathf.Abs(horizontal) > 0.1 || Mathf.Abs(vertical) > 0.1)
			{
				Vector3 destination = vertical * this.transform.forward + horizontal * this.transform.right;

				this.Move(destination, 1.0f);

				Vector3 newVrRoot = this.animator.rootPosition - (this.bodyAnchor.position - this.vrRoot.transform.position);

				this.vrRoot.transform.position = new Vector3(newVrRoot.x, 0.0f, newVrRoot.z);
			}
			else
			{
				Vector3 destination = new Vector3(this.bodyAnchor.position.x - this.transform.position.x, 0, this.bodyAnchor.position.z - this.transform.position.z);

				this.Move(destination, 4.0f);
				
				this.transform.rotation = this.bodyAnchor.rotation;
			}
		}

		private void Move(Vector3 move, float animSpeedMultiplier)
		{
			if (move.magnitude > 1f) move.Normalize();

			move = this.transform.InverseTransformDirection(move);

			move = Vector3.ProjectOnPlane(move, Vector3.up);

			this.animator.SetFloat("Forward", move.z, 0.1f, Time.deltaTime);
			this.animator.SetFloat("Turn",    move.x, 0.1f, Time.deltaTime);

			this.animator.speed = animSpeedMultiplier;
		}
	}
}

