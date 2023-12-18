using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.TurtleBot3
{
	public interface ITurtleBot3GraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(GameObject graspedObject);
	}

	public class TurtleBot3GraspingDetector : MonoBehaviour, IGripperTriggerHandler
	{
		private const float GripDistanceThreshold = 0.0f; // This parameter is meaningless currently.

		public GameObject gripRoot;
		public GameObject gripLeftLink;  // localPosition.x : -0.035 -- +0.01
		public GameObject gripRightLink; // localPosition.x : -0.01  -- +0.035

		public List<string> graspableTags;

		public List<GameObject> graspingNotificationDestinations;

		//------------------------

		private float gripLeftY;
		private float gripRightY;

		private float preGripLeftY;
		private float preGripRightY;

		private List<Rigidbody> graspableRigidbodies;

		private Rigidbody graspedRigidbody;
		private Transform savedParentObj;

		private bool  isGripperClosing;
		private float gripDistance;

		private HashSet<Rigidbody> leftCollidingObjects;
		private HashSet<Rigidbody> rightCollidingObjects;

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

//			Debug.Log("(TurtleBot3GraspingDetector)graspable collider num=" + this.graspableColliders.Count);

			this.leftCollidingObjects  = new HashSet<Rigidbody>();
			this.rightCollidingObjects = new HashSet<Rigidbody>();
		}
		
		// Use this for initialization
		void Start()
		{
			this.gripLeftY  = this.gripLeftLink .transform.localPosition.y;
			this.gripRightY = this.gripRightLink.transform.localPosition.y;

			this.preGripLeftY  = this.gripLeftY;
			this.preGripRightY = this.gripRightY;

			this.graspedRigidbody    = null;
			this.isGripperClosing = false;

			this.gripDistance = 0.0f;
		}

		// Update is called once per frame
		void Update()
		{
			this.gripLeftY  = this.gripLeftLink .transform.localPosition.y;
			this.gripRightY = this.gripRightLink.transform.localPosition.y;

			// Check hand closing
			if(this.gripLeftY < this.preGripLeftY && this.gripRightY > this.preGripRightY)
			{
				this.isGripperClosing = true;
			}
			else
			{
				this.isGripperClosing = false;
			}

			// Calc distance between grips
			if(this.gripLeftY > this.preGripLeftY && this.gripRightY < this.preGripRightY)
			{
				this.gripDistance += (this.gripLeftY - this.preGripLeftY) + (this.preGripRightY - this.gripRightY);
			}
			else
			{
				this.gripDistance = 0.0f;
			}

			if(this.gripDistance > GripDistanceThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preGripLeftY  = this.gripLeftY;
			this.preGripRightY = this.gripRightY;
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

			collidedRigidbody.gameObject.transform.parent = this.gripRoot.transform;

			collidedRigidbody.useGravity  = false;
//			collidedRigidbody.isKinematic = true;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			collidedRigidbody.gameObject.AddComponent<TurtleBot3GraspedObjectFixer>();

			this.graspedRigidbody = collidedRigidbody;

			this.SendGraspedObjectInfo(this.graspedRigidbody.gameObject);

			SIGVerseLogger.Info("Grasped: "+this.graspedRigidbody.gameObject.name);
		}

		private void Release()
		{
			this.graspedRigidbody.transform.parent = this.savedParentObj;

			this.graspedRigidbody.useGravity  = true;
//			this.graspedRigidbody.isKinematic = false;

			TurtleBot3GraspedObjectFixer graspedObjectFixer = this.graspedRigidbody.gameObject.GetComponent<TurtleBot3GraspedObjectFixer>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.graspedRigidbody.constraints = RigidbodyConstraints.None;

			this.graspedRigidbody = null;
			this.savedParentObj = null;

			this.SendGraspedObjectInfo(null);

			SIGVerseLogger.Info("Released the object");
		}

		private void SendGraspedObjectInfo(GameObject graspedObject)
		{
			foreach(GameObject graspingNotificationDestination in graspingNotificationDestinations)
			{
				ExecuteEvents.Execute<ITurtleBot3GraspedObjectHandler>
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
	}
}

