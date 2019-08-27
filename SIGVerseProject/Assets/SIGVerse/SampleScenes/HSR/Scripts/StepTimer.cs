using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	public class StepTimer
	{
		private int? stepId;
		private DateTime stepStartTime;

		public StepTimer()
		{
			this.stepId = null;
			this.stepStartTime = DateTime.Now;
		}

		public bool IsTimePassed(int stepId, int timeSpanMilli)
		{
			if (this.stepId == null || stepId != (int)this.stepId)
			{
				this.stepId = stepId;
				this.stepStartTime = DateTime.Now;

				return false;
			}

			if ((DateTime.Now - this.stepStartTime).TotalMilliseconds > timeSpanMilli)
			{
				this.stepStartTime = DateTime.Now;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
