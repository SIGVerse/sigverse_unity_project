using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public interface IHSRGraspedObjectHandler : IEventSystemHandler
	{
		void OnChangeGraspedObject(GameObject graspedObject);
	}

	public class HSRGraspingDetector : MonoBehaviour, IFingerTriggerHandler
	{
		private const float OpeningAngleThreshold = 0.0f; // This parameter is meaningless currently.

		public GameObject handPalm;
		public GameObject handLeftProximalLink;  // localEularAngle.x : 0 - 34
		public GameObject handRightProximalLink; // localEularAngle.x : -34 - 0
		public GameObject handLeftFingerTip;
		public GameObject handRightFingerTip;

		public List<string> graspableTags;

		public List<GameObject> graspingNotificationDestinations;

		//------------------------

		private float handLeftAngleX;
		private float handRightAngleX;

		private float preHandLeftAngleX;
		private float preHandRightAngleX;

		private List<Rigidbody> graspableRigidbodies;

		private Rigidbody graspedRigidbody;
		private Transform savedParentObj;

		private bool  isHandClosing;
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

//			Debug.Log("(HSRGraspingDetector)graspable collider num=" + this.graspableColliders.Count);

			this.leftCollidingObjects  = new HashSet<Rigidbody>();
			this.rightCollidingObjects = new HashSet<Rigidbody>();
		}
		
		// Use this for initialization
		void Start()
		{
			this.handLeftAngleX  = this.handLeftProximalLink .transform.localEulerAngles.x;
			this.handRightAngleX = this.handRightProximalLink.transform.localEulerAngles.x;

			this.preHandLeftAngleX  = this.handLeftAngleX;
			this.preHandRightAngleX = this.handRightAngleX;

			this.graspedRigidbody    = null;
			this.isHandClosing = false;

			this.openingAngle = 0.0f;
		}

		// Update is called once per frame
		void Update()
		{
			this.handLeftAngleX  = this.handLeftProximalLink .transform.localEulerAngles.x;
			this.handRightAngleX = this.handRightProximalLink.transform.localEulerAngles.x;

			// Check hand closing
			if(this.handLeftAngleX < this.preHandLeftAngleX && this.handRightAngleX > this.preHandRightAngleX)
			{
				this.isHandClosing = true;
			}
			else
			{
				this.isHandClosing = false;
			}

			// Calc opening angle
			if(this.handLeftAngleX > this.preHandLeftAngleX && this.handRightAngleX < this.preHandRightAngleX)
			{
				this.openingAngle += (this.handLeftAngleX - this.preHandLeftAngleX) + (this.preHandRightAngleX - this.handRightAngleX);
			}
			else
			{
				this.openingAngle = 0.0f;
			}

			if(this.openingAngle > OpeningAngleThreshold && this.graspedRigidbody!=null)
			{
				this.Release();
			}

			this.preHandLeftAngleX  = this.handLeftAngleX;
			this.preHandRightAngleX = this.handRightAngleX;
		}


		public void OnTransferredTriggerEnter(Rigidbody targetRigidbody, FingerType fingerType)
		{
			if(!this.IsGraspable(targetRigidbody)) { return; }

			if(fingerType==FingerType.Left)
			{
				this.leftCollidingObjects.Add(targetRigidbody);
			}
			if(fingerType==FingerType.Right)
			{
				this.rightCollidingObjects.Add(targetRigidbody);
			}

			if(this.isHandClosing && this.graspedRigidbody==null && this.leftCollidingObjects.Contains(targetRigidbody) && this.rightCollidingObjects.Contains(targetRigidbody))
			{
				this.Grasp(targetRigidbody);
			}
		}

		public void OnTransferredTriggerExit(Rigidbody targetRigidbody, FingerType fingerType)
		{
			if(!this.IsGraspable(targetRigidbody)) { return; }

			if(fingerType==FingerType.Left)
			{
				this.leftCollidingObjects.Remove(targetRigidbody);
			}
			if(fingerType==FingerType.Right)
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

			collidedRigidbody.gameObject.AddComponent<HSRGraspedObjectFixer>();

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

			HSRGraspedObjectFixer graspedObjectFixer = this.graspedRigidbody.gameObject.GetComponent<HSRGraspedObjectFixer>();
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
				ExecuteEvents.Execute<IHSRGraspedObjectHandler>
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

