using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Common
{
	public class RobotPubSynchronizer : MonoBehaviour
	{
		public bool useThread = false;

		//-------------------------------------

		private int sequenceNumberForAssignment = 0;

		private bool executed = false;

		private List<int> waitingSequenceNumbers = new List<int>();

		private bool isInitialized = false;

		void Start()
		{
			this.isInitialized = true;
		}

		public int GetAssignedSequenceNumber()
		{
			if(this.isInitialized) { throw new Exception("Please call " + System.Reflection.MethodBase.GetCurrentMethod().Name + " in Awake. ("+this.GetType().FullName+")"); }

			this.sequenceNumberForAssignment++;

			return this.sequenceNumberForAssignment;
		}

		public bool CanExecute(int sequenceNumber)
		{
			if (!this.executed && (this.waitingSequenceNumbers.Count==0 || this.waitingSequenceNumbers[0]==sequenceNumber))
			{
				this.executed = true;

				if(this.waitingSequenceNumbers.Count!=0)
				{
					this.waitingSequenceNumbers.RemoveAt(0);
				}

				return true;
			}
			else
			{
				if(!this.waitingSequenceNumbers.Contains(sequenceNumber))
				{
					this.waitingSequenceNumbers.Add(sequenceNumber);
				}

				return false;
			}
		}
	
		void LateUpdate()
		{
			this.executed = false;
		}
	}
}
