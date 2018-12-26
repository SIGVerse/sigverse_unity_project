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
		private const float OpeningAngleThreshold = 0.0f; // This parameter is meaningless currently.

		public GameObject handPalm;
		public GameObject leftGripper;  // 0   -- +34
		public GameObject rightGripper; // -34 -- 0

		public List<string> graspableTags;

		public List<GameObject> graspingNotificationDestinations;

		//------------------------

		private float leftGripperAngle;
		private float rightGripperAngle;

		private float preLeftGripperAngle;
		private float preRightGripperAngle;

		private List<Rigidbody> graspableRigidbodies;

		private Rigidbody graspedRigidbody;
		private Transform savedParentObj;

		private bool  isGripperClosing;
		private float openingAngle;

		private HashSet<Rigidbody> leftCollidingObjects;
		private HashSet<Rigidbody> rightCollidingObjects;

		private float latestReleaseTime = 0.0f;


		protected void Awake()
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

//			Debug.Log("(GraspingDetector)graspable collider num=" + this.graspableColliders.Count);

			this.leftCollidingObjects  = new HashSet<Rigidbody>();
			this.rightCollidingObjects = new HashSet<Rigidbody>();
		}
		
		// Use this for initialization
		void Start()
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
		void Update()
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


		public void OnTransferredTriggerEnter(Rigidbody targetRigidbody, GripperType gripperType)
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

		public void OnTransferredTriggerExit(Rigidbody targetRigidbody, GripperType gripperType)
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

		private bool IsGraspable(Rigidbody targetRigidbody)
		{
			foreach(Rigidbody graspableRigidbody in this.graspableRigidbodies)
			{
				if(targetRigidbody==graspableRigidbody) { return true; }
			}

			return false;
		}

		private void Grasp(Rigidbody collidedRigidbody)
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

		private void Release()
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

		private void SendGraspedObjectInfo(GameObject graspedObject)
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


		public GameObject GetGraspedObject()
		{
			if(this.graspedRigidbody==null) { return null; }

			return this.graspedRigidbody.gameObject;
		}

		public float GetLatestReleaseTime()
		{
			return this.latestReleaseTime;
		}
	}
}
