using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ToyotaHSR
{
	public class HSRPubSynchronizer : MonoBehaviour
	{
		public bool useThread = false;

		//-------------------------------------

		private int sequenceNumberForAssignment = 0;

		private bool executed = false;

		private List<int> waitingSequenceNumbers = new List<int>();

		private bool isInitialized = false;

		void Start()
		{
			isInitialized = true;
		}

		public int GetAssignedSequenceNumber()
		{
			if(isInitialized) { throw new Exception("Please call " + System.Reflection.MethodBase.GetCurrentMethod().Name + " in Awake. ("+this.GetType().FullName+")"); }

			sequenceNumberForAssignment++;

			return sequenceNumberForAssignment;
		}

		public bool CanExecute(int sequenceNumber)
		{
			if (!executed && (waitingSequenceNumbers.Count==0 || waitingSequenceNumbers[0]==sequenceNumber))
			{
				executed = true;

				if(waitingSequenceNumbers.Count!=0)
				{
					waitingSequenceNumbers.RemoveAt(0);
				}

				return true;
			}
			else
			{
				if(!waitingSequenceNumbers.Contains(sequenceNumber))
				{
					waitingSequenceNumbers.Add(sequenceNumber);
				}

				return false;
			}
		}
	
		void LateUpdate()
		{
			executed = false;
		}
	}
}
