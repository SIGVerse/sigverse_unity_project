using UnityEngine;
using SIGVerse.Common;


namespace SIGVerse.ToyotaHSR
{
	public class HSRGraspedObjectFixer : MonoBehaviour
	{
		private Vector3    prePos;
		private Quaternion preRot;

		private Rigidbody graspedRigidbody;

		void Awake()
		{
			this.prePos = this.transform.localPosition;
			this.preRot = this.transform.localRotation;

			this.graspedRigidbody = this.GetComponent<Rigidbody>();
		}

		void LateUpdate()
		{
			if(this.graspedRigidbody.constraints == RigidbodyConstraints.FreezeAll)
			{
				this.transform.localPosition = this.prePos;
				this.transform.localRotation = this.preRot;
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.collider.transform.root == this.transform.root) { return; }

			if(!this.enabled){ return; }

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;
		}

		void OnCollisionExit(Collision collision)
		{
			if(collision.collider.transform.root == this.transform.root) { return; }

			if(!this.enabled){ return; }

			this.graspedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			this.prePos = this.transform.localPosition;
			this.preRot = this.transform.localRotation;
		}
	}
}

