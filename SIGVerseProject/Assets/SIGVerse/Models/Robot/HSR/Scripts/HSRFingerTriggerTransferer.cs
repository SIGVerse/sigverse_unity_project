using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public enum TriggerType
	{
		Entrance,
		Exit,
	}

	public enum FingerType
	{
		Left,
		Right,
	}

	public interface IFingerTriggerHandler : IEventSystemHandler
	{
		void OnTransferredTriggerEnter(Rigidbody targetRigidbody, FingerType fingerType);
		void OnTransferredTriggerExit (Rigidbody targetRigidbody, FingerType fingerType);
	}

	public class HSRFingerTriggerTransferer : MonoBehaviour
	{
		public TriggerType triggerType;
		public FingerType  fingerType;

		private Dictionary<Collider, Rigidbody> rigidbodyMap;

		void Start()
		{
			this.rigidbodyMap = new Dictionary<Collider, Rigidbody>();
		}

		void OnTriggerEnter(Collider other)
		{
			if(other.isTrigger) { return; }

			if(other.attachedRigidbody == null) { return; }

			if(this.rigidbodyMap.ContainsKey(other))
			{
				SIGVerseLogger.Warn("This Collider has already been added. ("+this.GetType().FullName+")  name=" + SIGVerseUtils.GetHierarchyPath(other.transform));
				return;
			}


			if(this.triggerType==TriggerType.Entrance && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IFingerTriggerHandler>
				(
					target: this.transform.root.gameObject,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(other.attachedRigidbody, this.fingerType)
				);
			}

			this.rigidbodyMap.Add(other, other.attachedRigidbody);
		}

		void OnTriggerExit(Collider other)
		{
			if(other.isTrigger) { return; }

			if(other.attachedRigidbody == null) { return; }

			if(!this.rigidbodyMap.ContainsKey(other))
			{
				SIGVerseLogger.Warn("This Collider does not exist in the Dictionary. ("+this.GetType().FullName+")  name=" + SIGVerseUtils.GetHierarchyPath(other.transform));
				return;
			}

			this.rigidbodyMap.Remove(other);

			if(this.triggerType==TriggerType.Exit && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IFingerTriggerHandler>
				(
					target: this.transform.root.gameObject,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(other.attachedRigidbody, this.fingerType)
				);
			}
		}
	}
}

