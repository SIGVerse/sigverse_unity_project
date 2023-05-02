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

		public BoxCollider[] raySourceBoxes;

		protected HashSet<Rigidbody> currentRigidbodiesInEnterArea = new HashSet<Rigidbody>();
		protected HashSet<Rigidbody> currentRigidbodiesInExitArea  = new HashSet<Rigidbody>();
		
		protected HashSet<Rigidbody> previousRigidbodiesInEnterArea = new HashSet<Rigidbody>();
		protected HashSet<Rigidbody> previousRigidbodiesInExitArea  = new HashSet<Rigidbody>();


		protected virtual void Awake()
		{
			if(eventDestination==null)
			{
				this.eventDestination = this.transform.root.gameObject;
			}

			this.raySourceBoxes.ToList().ForEach(x => x.enabled = false);

			foreach(BoxCollider raySourceBox in this.raySourceBoxes)
			{
				if(raySourceBox.center.x!=0 || raySourceBox.center.y!=0)
				{
					string msg = "center.x or .center.y is NOT zero. ("+raySourceBox.name+")";
					Debug.LogError(msg);
					throw new Exception(msg);
				}
			}
		}

		protected virtual void FixedUpdate()
		{
			for (int i= 0; i < this.raySourceBoxes.Length; i++)
			{
				ManageRigidbodiesFor1Box(this.raySourceBoxes[i], this.layerMask, this.enableDebugRay, out HashSet<Rigidbody> rigidbodiesInEnterArea, out HashSet<Rigidbody> rigidbodiesInExitArea);

				this.currentRigidbodiesInEnterArea.UnionWith(rigidbodiesInEnterArea);
				this.currentRigidbodiesInExitArea .UnionWith(rigidbodiesInExitArea);
			}

			this.SendEnterEvent(this.currentRigidbodiesInEnterArea.Except(this.previousRigidbodiesInEnterArea));
			this.SendExitEvent (this.previousRigidbodiesInExitArea.Except(this.currentRigidbodiesInExitArea));

			this.previousRigidbodiesInEnterArea = new HashSet<Rigidbody>(this.currentRigidbodiesInEnterArea);
			this.previousRigidbodiesInExitArea  = new HashSet<Rigidbody>(this.currentRigidbodiesInExitArea);

			this.currentRigidbodiesInEnterArea.Clear();
			this.currentRigidbodiesInExitArea .Clear();
		}

		protected static void ManageRigidbodiesFor1Box(BoxCollider raySourceBox, LayerMask layerMask, bool enableDebugRay, out HashSet<Rigidbody> rigidbodiesInEnterArea, out HashSet<Rigidbody> rigidbodiesInExitArea)
		{
			rigidbodiesInEnterArea = new HashSet<Rigidbody>();
			rigidbodiesInExitArea  = new HashSet<Rigidbody>();

			float xSize = raySourceBox.size.x;
			float ySize = raySourceBox.size.y;

			int xDivisionNumber = Convert.ToInt32(Math.Ceiling(xSize / RaySpacing));
			int yDivisionNumber = Convert.ToInt32(Math.Ceiling(ySize / RaySpacing));

			for(int i=0; i<=xDivisionNumber; i++)
			{
				for(int j=0; j<=yDivisionNumber; j++)
				{
					float xPos = -xSize/2 + i*xSize/xDivisionNumber;
					float yPos = -ySize/2 + j*ySize/yDivisionNumber;

					Vector3 origin = raySourceBox.transform.TransformPoint(xPos, yPos, RaySourcePosZ);

					Ray ray = new Ray (origin, raySourceBox.transform.forward);

					RaycastHit hit;

					if (Physics.Raycast(ray, out hit, MaxRayDistance, layerMask))
					{
						if(!IsValidTrigger(hit.collider)){ continue; }

						if(hit.distance < EnterDistance-RaySourcePosZ)
						{
							rigidbodiesInEnterArea.Add(hit.rigidbody);
						}
						if(hit.distance < ExitDistance-RaySourcePosZ)
						{
							rigidbodiesInExitArea.Add(hit.rigidbody);
						}
					}

					if (enableDebugRay)
					{
						Debug.DrawRay(origin, raySourceBox.transform.forward * MaxRayDistance, Color.red);
					}
				}
			}
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

		protected static bool IsValidTrigger(Collider other)
		{
			if(other.isTrigger) { return false; }

			if(other.attachedRigidbody == null) { return false; }

			return true;
		}
	}
}

