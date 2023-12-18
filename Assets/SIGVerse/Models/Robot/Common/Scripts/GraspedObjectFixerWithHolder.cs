using System.Collections;
using UnityEngine;

namespace SIGVerse.Common
{
	public class GraspedObjectFixerWithHolder : MonoBehaviour
	{
		private const float EnableCollisionTime = 0.3f;

		public bool canChangeGraspPoint = false;

		public Transform holder;

		private Vector3    graspRelativePos;
		private Quaternion graspRelativeRot;

		private Rigidbody graspedRigidbody;

		private bool collisionEnabled = false;

		void Awake()
		{
			this.graspedRigidbody = this.GetComponent<Rigidbody>();
		}

		void Start()
		{
			StartCoroutine(EnableCollision());
		}

		private IEnumerator EnableCollision() 
		{ 
			yield return new WaitForSeconds(EnableCollisionTime);

			this.collisionEnabled = true;
		}

		void FixedUpdate()
		{
			if(this.graspedRigidbody.constraints == RigidbodyConstraints.FreezeAll)
			{
				this.transform.position = this.holder.TransformPoint(this.graspRelativePos);
				this.transform.rotation = this.holder.rotation * this.graspRelativeRot;
			}
		}

		public void SetHolder(Transform holder)
		{
			this.holder = holder;

			SetRelativePoint();
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.transform.root == this.holder.root) { return; }

			if (!this.enabled){ return; }

			if (!this.collisionEnabled) { return; } // Immediately after grasping, a collision with the table on which the object was placed could occur. So added an avoidance process.

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

//			Debug.LogWarning("GraspedObjectFixer OnCollisionEnter obj=" + collision.gameObject.name);
		}

		void OnCollisionExit(Collision collision)
		{
			if(collision.collider.transform.root == this.holder.root) { return; }

			if(!this.enabled){ return; }

			this.graspedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			if(this.canChangeGraspPoint)
			{
				SetRelativePoint();
			}
		}

		private void SetRelativePoint()
		{
			this.graspRelativePos = this.holder.InverseTransformPoint(this.transform.position);
			this.graspRelativeRot = Quaternion.Inverse(this.holder.rotation) * this.transform.rotation;
		}
	}
}

