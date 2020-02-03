using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Common
{
	public class GripperTriggerTransfererRay : MonoBehaviour
	{
		private const float RaySourcePosZ = -0.003f;  // From -3[mm]

		private const float EnterDistance = 0.002f; // 2[mm]
		private const float ExitDistance  = 0.008f; // 8[mm]

		private const float RaySpacing = 0.003f; // 3[mm]

		private const float MaxRayDistance = ExitDistance - RaySourcePosZ + 0.001f;

		public GripperType gripperType;

		public GameObject eventDestination;

		public LayerMask layerMask = -1;

		public bool enableDebugRay = false;

		protected HashSet<Rigidbody> currentEnterRigidbodies;
		protected HashSet<Rigidbody> currentExitRigidbodies;

		protected HashSet<Rigidbody> previousEnterRigidbodies;
		protected HashSet<Rigidbody> previousExitRigidbodies;

		protected BoxCollider raySourceBox;


		protected virtual void Awake()
		{
			if(eventDestination==null)
			{
				this.eventDestination = this.transform.root.gameObject;
			}

			this.raySourceBox = this.transform.Find("RaySourceBox").GetComponent<BoxCollider>();

			this.raySourceBox.enabled = false;

			if(raySourceBox.center.x!=0 || raySourceBox.center.y!=0)
			{
				string msg = "raySourceBox.center.x or raySourceBox.center.y is NOT zero. ("+this.gameObject.name+")";
				Debug.LogError(msg);
				throw new Exception(msg);
			}
		}

		protected virtual void Start()
		{
			this.currentEnterRigidbodies  = new HashSet<Rigidbody>();
			this.currentExitRigidbodies   = new HashSet<Rigidbody>();

			this.previousEnterRigidbodies = new HashSet<Rigidbody>();
			this.previousExitRigidbodies  = new HashSet<Rigidbody>();
		}

		protected virtual void FixedUpdate()
		{
			float xSize = this.raySourceBox.size.x;
			float ySize = this.raySourceBox.size.y;

			int xDivisionNumber = Convert.ToInt32(Math.Ceiling(xSize / RaySpacing));
			int yDivisionNumber = Convert.ToInt32(Math.Ceiling(ySize / RaySpacing));

			for(int i=0; i<=xDivisionNumber; i++)
			{
				for(int j=0; j<=yDivisionNumber; j++)
				{
					float xPos = -xSize/2 + i*xSize/xDivisionNumber;
					float yPos = -ySize/2 + j*ySize/yDivisionNumber;

					Vector3 origin = this.raySourceBox.transform.TransformPoint(xPos, yPos, RaySourcePosZ);

					Ray ray = new Ray (origin, this.raySourceBox.transform.forward);

					RaycastHit hit;

					if (Physics.Raycast(ray, out hit, MaxRayDistance, this.layerMask))
					{
						if(!this.IsValidTrigger(hit.collider)){ continue; }

						if(hit.distance < EnterDistance-RaySourcePosZ)
						{
							this.currentEnterRigidbodies.Add(hit.rigidbody);
						}
						if(hit.distance < ExitDistance-RaySourcePosZ)
						{
							this.currentExitRigidbodies.Add(hit.rigidbody);
						}
					}

					if (this.enableDebugRay)
					{
						Debug.DrawRay(origin, this.raySourceBox.transform.forward * MaxRayDistance, Color.red);
					}
				}
			}

			this.SendEnterEvent(this.currentEnterRigidbodies.Except(this.previousEnterRigidbodies));
			this.SendExitEvent (this.previousExitRigidbodies.Except(this.currentExitRigidbodies));

			this.previousEnterRigidbodies = new HashSet<Rigidbody>(this.currentEnterRigidbodies);
			this.previousExitRigidbodies  = new HashSet<Rigidbody>(this.currentExitRigidbodies);

			this.currentEnterRigidbodies.Clear();
			this.currentExitRigidbodies .Clear();
		}

		protected virtual void SendEnterEvent(IEnumerable<Rigidbody> rigidbodies)
		{
			foreach(Rigidbody rigidbody in rigidbodies)
			{
				ExecuteEvents.Execute<IGripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(rigidbody, this.gripperType)
				);
//				Debug.Log("SendEnterEvent name="+rigidbody.name);
			}
		}

		protected virtual void SendExitEvent(IEnumerable<Rigidbody> rigidbodies)
		{
			foreach(Rigidbody rigidbody in rigidbodies)
			{
				ExecuteEvents.Execute<IGripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(rigidbody, this.gripperType)
				);
//				Debug.Log("SendExitEvent name="+rigidbody.name);
			}
		}

		protected bool IsValidTrigger(Collider other)
		{
			if(other.isTrigger) { return false; }

			if(other.attachedRigidbody == null) { return false; }

			return true;
		}
	}
}

