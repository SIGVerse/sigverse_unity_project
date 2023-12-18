using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using System.Collections;

namespace SIGVerse.Common
{
	public class SuctionDetector : MonoBehaviour
	{
		protected const float SuctionRadius = 0.002f; // [m]
		protected const int RayNum = 8;
		protected const float RayStartPos = -0.002f; // [m]  Ray from 2mm behind
		protected const float MaxRayDistance = 0.003f; // [m]

		public float releaseThresholdDistance = 0.05f; // [m] 
		public float releaseThresholdAngle = 10.0f;    // [deg]

		public Transform vacuum;
		public BoxCollider vacuumCollider;

		public LayerMask layerMask = -1;
		public bool enableDebugRay = false;

		public List<string> graspableTags;

		//------------------------

		protected bool isSuctionOn = false;

		protected Rigidbody[] hitRigidbodies = new Rigidbody[RayNum];

		protected Rigidbody suckedRigidbody = null;

		protected Transform savedParentObj;

		protected Vector3    initSuckedRelPos;
		protected Quaternion initSuckedRelRot;

		protected virtual void Awake()
		{
		}
		
		protected virtual void Start()
		{
		}

		public virtual void StartSuction()
		{
			if(this.isSuctionOn)
			{
				SIGVerseLogger.Info("Already suction_on = true.");
				return;
			}

			this.isSuctionOn = true;
		}


		public virtual void StopSuction()
		{
			if(!this.isSuctionOn)
			{
				SIGVerseLogger.Info("Already suction_on = false.");
				return;
			}

			this.isSuctionOn = false;
		}

		public virtual bool IsPressureSensorOn()
		{
			return this.suckedRigidbody != null;
		}

		// Update is called once per frame
		protected virtual void FixedUpdate()
		{
			if (this.isSuctionOn) 
			{
				if (this.suckedRigidbody==null)
				{
					for(int i=0; i<RayNum; i++)
					{
						this.hitRigidbodies[i] = null;

						float angle = i * 2.0f * Mathf.PI / RayNum;

						float xPos = Mathf.Cos(angle);
						float yPos = Mathf.Sin(angle);

						Vector3 origin = this.vacuum.position + this.vacuum.forward * RayStartPos + SuctionRadius*(this.vacuum.right * xPos + this.vacuum.up * yPos);

						Ray ray = new Ray (origin, this.vacuum.forward);

						RaycastHit hit;

						if (Physics.Raycast(ray, out hit, MaxRayDistance, layerMask))
						{
							this.hitRigidbodies[i] = hit.rigidbody;
						}

						if (this.enableDebugRay)
						{
							Debug.DrawRay(origin, this.vacuum.forward * MaxRayDistance, Color.red);
						}
					}

					// If all rigidbodies are the same
					if(this.hitRigidbodies[0]!=null && this.hitRigidbodies.All(x => x == this.hitRigidbodies[0]))
					{
						if(this.IsGraspable(this.hitRigidbodies[0]))
						{
							this.Suck(this.hitRigidbodies[0]);
						}
					}
				}
				else
				{
					Vector3    relPos = this.vacuum.InverseTransformPoint(this.suckedRigidbody.position);
					Quaternion relRot = Quaternion.Inverse(this.vacuum.rotation) * this.suckedRigidbody.rotation;

					bool isReleasePos = Vector3.Distance(this.initSuckedRelPos, relPos) > this.releaseThresholdDistance;
					bool isReleaseRot = Quaternion.Angle(this.initSuckedRelRot, relRot) > this.releaseThresholdAngle;

					if (isReleasePos || isReleaseRot)
					{
						if (isReleasePos) { SIGVerseLogger.Warn("Suction Pos Diff=" + Vector3.Distance(this.initSuckedRelPos, relPos)+" > "+this.releaseThresholdDistance); }
						if (isReleaseRot) { SIGVerseLogger.Warn("Suction Rot Diff=" + Quaternion.Angle(this.initSuckedRelRot, relRot)+" > "+this.releaseThresholdAngle); }

						this.Release();
						SIGVerseLogger.Warn("The sucked object was released. (Probably due to collisions)");
					}
				}
			}
			else if (this.suckedRigidbody!=null)
			{
				this.Release();
			}
		}


		protected virtual bool IsGraspable(Rigidbody targetRigidbody)
		{
			foreach(string graspableTag in this.graspableTags)
			{
				if(targetRigidbody.tag==graspableTag) { return true; }
			}

			return false;
		}

		protected virtual void Suck(Rigidbody collidedRigidbody)
		{
			this.savedParentObj = collidedRigidbody.gameObject.transform.parent;

			collidedRigidbody.gameObject.transform.parent = this.vacuum;

			collidedRigidbody.useGravity  = false;
//			collidedRigidbody.isKinematic = true;
			collidedRigidbody.constraints = RigidbodyConstraints.FreezeAll;

			collidedRigidbody.gameObject.AddComponent<GraspedObjectFixer>();

			this.suckedRigidbody = collidedRigidbody;
			
			this.initSuckedRelPos = this.vacuum.InverseTransformPoint(this.suckedRigidbody.position);
			this.initSuckedRelRot = Quaternion.Inverse(this.vacuum.rotation) * this.suckedRigidbody.rotation;

			SIGVerseLogger.Info("Suction: Sucked: " + this.suckedRigidbody.name);
		}

		protected virtual void Release()
		{
			this.suckedRigidbody.transform.parent = this.savedParentObj;

			this.suckedRigidbody.useGravity  = true;
//			this.suckedRigidbody.isKinematic = false;

			GraspedObjectFixer graspedObjectFixer = this.suckedRigidbody.gameObject.GetComponent<GraspedObjectFixer>();
			graspedObjectFixer.enabled = false;
			Destroy(graspedObjectFixer);

			this.suckedRigidbody.constraints = RigidbodyConstraints.None;

			this.suckedRigidbody = null;
			this.savedParentObj = null;

			SIGVerseLogger.Info("Suction: Released the object");
		}
	}
}

