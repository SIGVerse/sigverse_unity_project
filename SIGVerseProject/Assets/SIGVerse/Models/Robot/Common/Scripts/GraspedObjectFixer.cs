using UnityEngine;
using static UnityEngine.UI.Image;

namespace SIGVerse.Common
{
	public class GraspedObjectFixer : MonoBehaviour
	{
		public bool canChangeGraspPoint = false;

		private Vector3    graspPos;
		private Quaternion graspRot;

		private Rigidbody graspedRigidbody;

		void Awake()
		{
			this.graspPos = this.transform.localPosition;
			this.graspRot = this.transform.localRotation;

			this.graspedRigidbody = this.GetComponent<Rigidbody>();
		}

		void LateUpdate()
		{
			if(this.graspedRigidbody.constraints == RigidbodyConstraints.FreezeAll)
			{
				this.transform.localPosition = this.graspPos;
				this.transform.localRotation = this.graspRot;
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.collider.transform.root == this.transform.root) { return; }

			if(!this.enabled){ return; }

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

//			Debug.LogWarning("GraspedObjectFixer OnCollisionEnter obj=" + collision.name);
		}

		void OnCollisionExit(Collision collision)
		{
			if(collision.collider.transform.root == this.transform.root) { return; }

			if(!this.enabled){ return; }

			this.graspedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			if(this.canChangeGraspPoint)
			{
				this.graspPos = this.transform.localPosition;
				this.graspRot = this.transform.localRotation;
			}
		}
	}
}

