using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.Common
{
	public interface IGraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(GameObject graspedObject);
	}

	public class GraspingDetector : MonoBehaviour, IGripperTriggerHandler
	{
		protected const float OpeningAngleThreshold = 0.0f; // This parameter is meaningless currently.

		public GameObject handPalm;
		public GameObject leftGripper;
		public GameObject rightGripper;

		public List<string> graspableTags;

		public List<GameObject> graspingNotificationDestinations;

		//------------------------

		protected float leftGripperAngle;
		protected float rightGripperAngle;

		protected float preLeftGripperAngle;
		protected float preRightGripperAngle;

		protected List<Rigidbody> graspableRigidbodies;

		protected Rigidbody graspedRigidbody;
		protected Transform savedParentObj;

		protected bool  isGripperClosing;
		protected float openingAngle;

		protected HashSet<Rigidbody> leftCollidingObjects;
		protected HashSet<Rigidbody> rightCollidingObjects;

		protected float latestReleaseTime = 0.0f;


		protected virtual void Awake()
		{
			this.graspableRigidbodies = new List<Rigidbody>();

			foreach(string graspableTag in graspableTags)
			{
				List<GameObject> graspableObjects = GameObject.FindGameObjectsWithTag(graspableTag).ToList<GameObject>();

				foreach(GameObject graspableObject in graspableObjects)
				{
					List<Rigidbody> rigidbodies = graspableObject.GetComponentsInChildren<Rigidbody>().ToList<Rigidbody>();

					this.graspableRigidbodies.AddRange(rigidbodies);
				}
			}

			this.leftCollidingObjects  = new HashSet<Rigidbody>();
			this.rightCollidingObjects = new HashSet<Rigidbody>();
		}
		
		// Use this for initialization
		protected virtual void Start()
		{
			this.leftGripperAngle  = this.leftGripper .transform.localEulerAngles.x;
			this.rightGripperAngle = this.rightGripper.transform.localEulerAngles.x;

			this.preLeftGripperAngle  = this.leftGripperAngle;
			this.preRightGripperAngle = this.rightGripperAngle;

			this.graspedRigidbody    = null;
			this.isGripperClosing = false;

			this.openingAngle = 0.0f;
		}

		// Update is called once per frame
		protected virtual void FixedUpdate()
		{
			this.leftGripperAngle  = this.leftGripper .transform.localEulerAngles.x;
			this.rightGripperAngle = this.rightGripper.transform.localEulerAngles.x;

			// Check hand closing
			if(this.leftGripperAngle < this.preLeftGripperAngle && this.rightGripperAngle > this.preRightGripperAngle)
			{
				this.isGripperClosing = true;
			}
			else
			{
				this.isGripperClosing = false;
			}

			// Calc opening angle
			if(this.leftGripperAngle > this.preLeftGripperAngle && this.rightGripperAngle < this.preRightGripperAngle)
			{
				this.openingAngle += (this.leftGripperAngle - this.preLeftGripperAngle) + (this.preRightGripperAngle - this.rightGripperAngle);
			}
			else
			{
				this.openingAngle = 0.0f;
			}

			if(this.openingAngle > OpeningAngleThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preLeftGripperAngle  = this.leftGripperAngle;
			this.preRightGripperAngle = this.rightGripperAngle;
		}


		public virtual void OnTransferredTriggerEnter(Rigidbody targetRigidbody, GripperType gripperType)
		{
			if(!this.IsGraspable(targetRigidbody)) { return; }

			if(gripperType==GripperType.Left)
			{
				this.leftCollidingObjects.Add(targetRigidbody);
			}
			if(gripperType==GripperType.Right)
			{
				this.rightCollidingObjects.Add(targetRigidbody);
			}

			if(this.isGripperClosing && this.graspedRigidbody==null && this.leftCollidingObjects.Contains(targetRigidbody) && this.rightCollidingObjects.Contains(targetRigidbody))
			{
				this.Grasp(targetRigidbody);
			}
		}

		public virtual void OnTransferredTriggerExit(Rigidbody targetRigidbody, GripperType gripperType)
		{
			if(!this.IsGraspable(targetRigidbody)) { return; }

			if(gripperType==GripperType.Left)
			{
				this.leftCollidingObjects.Remove(targetRigidbody);
			}
			if(gripperType==GripperType.Right)
			{
				this.rightCollidingObjects.Remove(targetRigidbody);
			}

			if (this.graspedRigidbody != null)
			{
				if (!this.leftCollidingObjects.Contains(this.graspedRigidbody) && !this.rightCollidingObjects.Contains(this.graspedRigidbody))
				{
					if(this.graspedRigidbody.constraints == RigidbodyConstraints.FreezeAll) { return; }

					this.Release();
				}
			}
		}

		protected virtual bool IsGraspable(Rigidbody targetRigidbody)
		{
			foreach(Rigidbody graspableRigidbody in this.graspableRigidbodies)
			{
				if(targetRigidbody==graspableRigidbody) { return true; }
			}

			return false;
		}

		protected virtual void Grasp(Rigidbody collidedRigidbody)
		{
			this.savedParentObj = collidedRigidbody.gameObject.transform.parent;

			collidedRigidbody.gameObject.transform.parent = this.handPalm.transform;

			collidedRigidbody.useGravity  = false;
//			collidedRigidbody.isKinematic = true;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			collidedRigidbody.gameObject.AddComponent<GraspedObjectFixer>();

			this.graspedRigidbody = collidedRigidbody;

			this.SendGraspedObjectInfo(this.graspedRigidbody.gameObject);

			SIGVerseLogger.Info("Grasped: "+this.graspedRigidbody.gameObject.name);

			this.latestReleaseTime = 0.0f;
		}

		protected virtual void Release()
		{
			this.graspedRigidbody.transform.parent = this.savedParentObj;

			this.graspedRigidbody.useGravity  = true;
//			this.graspedRigidbody.isKinematic = false;

			GraspedObjectFixer graspedObjectFixer = this.graspedRigidbody.gameObject.GetComponent<GraspedObjectFixer>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

			this.graspedRigidbody = null;
			this.savedParentObj = null;

			this.SendGraspedObjectInfo(null);

			SIGVerseLogger.Info("Released the object");

			this.latestReleaseTime = Time.time;
		}

		protected virtual void SendGraspedObjectInfo(GameObject graspedObject)
		{
			foreach(GameObject graspingNotificationDestination in graspingNotificationDestinations)
			{
				ExecuteEvents.Execute<IGraspedObjectHandler>
				(
					target: graspingNotificationDestination, 
					eventData: null, 
					functor: (reciever, eventData) => reciever.OnChangeGraspedObject(graspedObject)
				);
			}
		}


		public virtual GameObject GetGraspedObject()
		{
			if(this.graspedRigidbody==null) { return null; }

			return this.graspedRigidbody.gameObject;
		}

		public virtual float GetLatestReleaseTime()
		{
			return this.latestReleaseTime;
		}
	}
}

