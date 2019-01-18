using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.Common
{
	public enum TriggerType
	{
		Entrance,
		Exit,
	}

	public enum GripperType
	{
		Left,
		Right,
	}

	public interface IGripperTriggerHandler : IEventSystemHandler
	{
		void OnTransferredTriggerEnter(Rigidbody targetRigidbody, GripperType gripperType);
		void OnTransferredTriggerExit (Rigidbody targetRigidbody, GripperType gripperType);
	}

	public class GripperTriggerTransferer : MonoBehaviour
	{
		public TriggerType triggerType;
		public GripperType gripperType;

		public GameObject eventDestination;

		protected Dictionary<Collider, Rigidbody> rigidbodyMap;

		protected virtual void Awake()
		{
			if(eventDestination==null)
			{
				this.eventDestination = this.transform.root.gameObject;
			}
		}

		protected virtual void Start()
		{
			this.rigidbodyMap = new Dictionary<Collider, Rigidbody>();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if(!this.IsValidTriggerEnter(other)){ return; }

			if(this.triggerType==TriggerType.Entrance && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IGripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(other.attachedRigidbody, this.gripperType)
				);
			}

			this.rigidbodyMap.Add(other, other.attachedRigidbody);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if(!this.IsValidTriggerExit(other)){ return; }

			this.rigidbodyMap.Remove(other);

			if(this.triggerType==TriggerType.Exit && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IGripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(other.attachedRigidbody, this.gripperType)
				);
			}
		}

		protected bool IsValidTriggerEnter(Collider other)
		{
			if(other.isTrigger) { return false; }

			if(other.attachedRigidbody == null) { return false; }

			if(this.rigidbodyMap.ContainsKey(other))
			{
				SIGVerseLogger.Warn("This Collider has already been added. ("+this.GetType().FullName+")  name=" + SIGVerseUtils.GetHierarchyPath(other.transform));
				return false;
			}

			return true;
		}

		protected bool IsValidTriggerExit(Collider other)
		{
			if(other.isTrigger) { return false; }

			if(other.attachedRigidbody == null) { return false; }

			if(!this.rigidbodyMap.ContainsKey(other))
			{
				SIGVerseLogger.Warn("This Collider does not exist in the Dictionary. ("+this.GetType().FullName+")  name=" + SIGVerseUtils.GetHierarchyPath(other.transform));
				return false;
			}

			return true;
		}
	}
}

