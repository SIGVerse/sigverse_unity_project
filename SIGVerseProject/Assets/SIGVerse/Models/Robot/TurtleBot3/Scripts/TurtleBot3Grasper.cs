using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.TurtleBot3
{
	public interface ITurtleBot3GraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(GameObject graspedObject);
	}

	public class TurtleBot3Grasper : MonoBehaviour
	{
		public Transform gripperLeftLink;
		public Transform gripperRightLink;
		public Transform graspingCenterPoint;

		public GameObject collisionEffect;

		public GameObject rosBridgeScripts;

		public List<Collider> exclusionColliderList;

		public List<GameObject> graspables;

		//------------------------

		private float preGripperLeftPosY;
		private float preGripperRightPosY;

		private Rigidbody graspedRigidbody;

		private bool                 backupedRigidbodyUseGravity;
		private bool                 backupedRigidbodyIsKinematic;
		private RigidbodyConstraints backupedRigidbodyConstraints;
		private Transform            backupedRigidbodyParent;

		private bool isClosing;

		// Use this for initialization
		void Start()
		{
			this.preGripperLeftPosY  = this.gripperLeftLink .localPosition.y;
			this.preGripperRightPosY = this.gripperRightLink.localPosition.y;

			this.graspedRigidbody = null;
			this.isClosing = false;
		}

		// Update is called once per frame
		void Update()
		{
			if (this.gripperLeftLink.localPosition.y < this.preGripperLeftPosY && this.gripperRightLink.localPosition.y > this.preGripperRightPosY)
			{
				this.isClosing = true;
			}
			else
			{
				this.isClosing = false;
			}

			if (this.gripperLeftLink.localPosition.y > this.preGripperLeftPosY && this.gripperRightLink.localPosition.y < this.preGripperRightPosY && this.graspedRigidbody!=null)
			{
				this.RestoreRigidbodyParam(this.graspedRigidbody);

				SIGVerseLogger.Info("Released: " + this.graspedRigidbody.gameObject.name);

				this.graspedRigidbody = null;
				this.SendGraspedObjectInfo(null);
			}

			this.preGripperLeftPosY  = this.gripperLeftLink .localPosition.y;
			this.preGripperRightPosY = this.gripperRightLink.localPosition.y;
		}

		void OnTriggerEnter(Collider otherCollider)
		{
			if (otherCollider.transform.root == this.transform.root) { return; }

			foreach (Collider collider in exclusionColliderList)
			{
				if (otherCollider == collider) { return; }
			}

//			Debug.Log("OnTriggerEnter name=" + otherCollider.name);

			if (this.isThisCollidedWithGraspableObject(otherCollider))
			{
				if (this.isClosing)
				{
					this.BackupRigidbodyParam(otherCollider.attachedRigidbody);

					otherCollider.attachedRigidbody.useGravity  = false;
					otherCollider.attachedRigidbody.isKinematic = true;
					otherCollider.attachedRigidbody.constraints = RigidbodyConstraints.None;

					otherCollider.attachedRigidbody.gameObject.transform.position = this.graspingCenterPoint.position - otherCollider.attachedRigidbody.centerOfMass;

					otherCollider.attachedRigidbody.gameObject.transform.parent = this.graspingCenterPoint;

					this.graspedRigidbody = otherCollider.attachedRigidbody;
					this.SendGraspedObjectInfo(this.graspedRigidbody.gameObject);

					SIGVerseLogger.Info("Grasped: " + this.graspedRigidbody.gameObject.name);
				}
			}
		}

		private void BackupRigidbodyParam(Rigidbody rigidbody)
		{
			this.backupedRigidbodyUseGravity  = rigidbody.useGravity;
			this.backupedRigidbodyIsKinematic = rigidbody.isKinematic;
			this.backupedRigidbodyConstraints = rigidbody.constraints;

			this.backupedRigidbodyParent = rigidbody.gameObject.transform.parent;
		}

		private void RestoreRigidbodyParam(Rigidbody rigidbody)
		{
			rigidbody.useGravity  = this.backupedRigidbodyUseGravity;
			rigidbody.isKinematic = this.backupedRigidbodyIsKinematic;
			rigidbody.constraints = this.backupedRigidbodyConstraints;

			rigidbody.transform.parent = this.backupedRigidbodyParent;
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.transform.root == this.transform.root) { return; }

			Debug.Log("OnCollisionEnter name=" + collision.collider.name);

			foreach (Collider collider in exclusionColliderList)
			{
				if (collision.collider == collider) { return; }
			}

			if (!this.isThisCollidedWithGraspableObject(collision.collider))
			{
				this.CollisionEffect(collision);
			}
		}


		protected bool isThisCollidedWithGraspableObject(Collider otherCollider)
		{
			Rigidbody rigidbody = otherCollider.attachedRigidbody;

			if (rigidbody == null)
			{
//				SIGVerseLogger.Info("The attachedRigidbody of grasped object is null in isThisCollidedWithGraspableObject.");
				return false;
			}

			foreach (GameObject graspableObj in this.graspables)
			{
				if (rigidbody.gameObject == graspableObj)
				{
					return true;
				}
			}
			return false;
		}

		protected void CollisionEffect(Collision collision)
		{
			SIGVerseLogger.Info("Collision detection! Collided object=" + collision.collider.name);

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			effect.transform.position = collision.contacts[0].point;
//			effect.transform.position = otherCollider.bounds.ClosestPoint(this.gameObject.transform.position); // Rough position
			Destroy(effect, 1.0f);
		}

		private void SendGraspedObjectInfo(GameObject graspedObject)
		{
			ExecuteEvents.Execute<ITurtleBot3GraspedObjectHandler>
			(
				target: this.rosBridgeScripts, 
				eventData: null, 
				functor: (reciever, eventData) => reciever.OnChangeGraspedObject(graspedObject)
			);
		}
	}
}


