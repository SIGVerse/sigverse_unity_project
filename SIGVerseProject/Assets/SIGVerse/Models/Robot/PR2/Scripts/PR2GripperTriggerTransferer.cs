using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.PR2
{
	public enum HandType
	{
		Left,
		Right,
	}

	public interface IPr2GripperTriggerHandler : IEventSystemHandler
	{
		void OnTransferredTriggerEnter(HandType handType, GripperType gripperType, Rigidbody targetRigidbody);
		void OnTransferredTriggerExit (HandType handType, GripperType gripperType, Rigidbody targetRigidbody);
	}

	public class PR2GripperTriggerTransferer : GripperTriggerTransferer
	{
		public HandType handType;

		protected override void OnTriggerEnter(Collider other)
		{
			if(!this.IsValidTriggerEnter(other)){ return; }

			if(this.triggerType==TriggerType.Entrance && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IPr2GripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(this.handType, this.gripperType, other.attachedRigidbody)
				);
			}

			this.rigidbodyMap.Add(other, other.attachedRigidbody);
		}

		protected override void OnTriggerExit(Collider other)
		{
			if(!this.IsValidTriggerExit(other)){ return; }

			this.rigidbodyMap.Remove(other);

			if(this.triggerType==TriggerType.Exit && !this.rigidbodyMap.ContainsValue(other.attachedRigidbody))
			{
				ExecuteEvents.Execute<IPr2GripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(this.handType, this.gripperType, other.attachedRigidbody)
				);
			}
		}
	}
}

