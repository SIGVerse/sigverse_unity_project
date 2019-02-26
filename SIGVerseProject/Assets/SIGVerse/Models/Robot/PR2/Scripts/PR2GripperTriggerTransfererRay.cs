using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.PR2
{
	public class PR2GripperTriggerTransfererRay : GripperTriggerTransfererRay
	{
		public HandType handType;

		protected override void SendEnterEvent(IEnumerable<Rigidbody> rigidbodies)
		{
			foreach (Rigidbody rigidbody in rigidbodies)
			{
				ExecuteEvents.Execute<IPr2GripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerEnter(this.handType, this.gripperType, rigidbody)
				);
				Debug.Log("SendEnterEvent name="+rigidbody.name);
			}
		}

		protected override void SendExitEvent(IEnumerable<Rigidbody> rigidbodies)
		{
			foreach (Rigidbody rigidbody in rigidbodies)
			{
				ExecuteEvents.Execute<IPr2GripperTriggerHandler>
				(
					target: this.eventDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredTriggerExit(this.handType, this.gripperType, rigidbody)
				);
				Debug.Log("SendExitEvent name="+rigidbody.name);
			}
		}
	}
}

