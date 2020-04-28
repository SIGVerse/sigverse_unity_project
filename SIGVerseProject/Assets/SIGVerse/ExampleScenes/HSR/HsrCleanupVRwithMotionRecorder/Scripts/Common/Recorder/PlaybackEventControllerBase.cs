using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIGVerse.Common.Recorder
{
	public interface PlaybackEventBase
	{
		void Execute();
	}

	public abstract class PlaybackEventListBase<TPlaybackEvent> where TPlaybackEvent : PlaybackEventBase
	{
		public float ElapsedTime { get; set; }
		public List<TPlaybackEvent> EventList { get; set; }
	}

	// ------------------------------------------------------------------

	public abstract class PlaybackEventControllerBase<TPlaybackEventList, TPlaybackEvent> where TPlaybackEventList : PlaybackEventListBase<TPlaybackEvent> where TPlaybackEvent : PlaybackEventBase
	{
		public List<TPlaybackEventList> eventLists;

		public int index = -1;


		public virtual void StartInitializingEvents()
		{
		}


		public abstract bool ReadEvents(string[] headerArray, string dataStr);


		public virtual void ExecutePassedAllEvents(float elapsedTime, float deltaTime)
		{
			List<TPlaybackEventList> executionEventLists = new List<TPlaybackEventList>();

			if(deltaTime >= 0.0f)
			{
				if(this.IsEventDataFinished()){ return; }

				while (elapsedTime >= this.eventLists[this.index + 1].ElapsedTime)
				{
					executionEventLists.Add(this.ModifyExecutionEventList(this.eventLists[this.index + 1]));

					this.index++;

					if (this.IsEventDataFinished()) { break; }
				}
			}
			else
			{
				if(this.index < 0){ return; }

				while (elapsedTime <= this.eventLists[this.index].ElapsedTime)
				{
					executionEventLists.Add(this.ModifyExecutionEventList(this.eventLists[this.index]));

					this.index--;

					if (this.index < 0) { break; }
				}
			}

			if (executionEventLists.Count == 0) { return; }

			foreach (TPlaybackEventList executionEventList in executionEventLists)
			{
				List<TPlaybackEvent> executionEvents = executionEventList.EventList;

				foreach(TPlaybackEvent executionEvent in executionEvents)
				{
					executionEvent.Execute();
				}
			}
		}


		public virtual void ExecutePassedLatestEvents(float elapsedTime, float deltaTime)
		{
			TPlaybackEventList executionEventList = null;

			if(deltaTime >= 0.0f)
			{
				if(this.IsEventDataFinished()){ return; }

				while (elapsedTime >= this.eventLists[this.index + 1].ElapsedTime)
				{
					executionEventList = this.ModifyExecutionEventList(this.eventLists[this.index + 1]);

					this.index++;

					if (this.IsEventDataFinished()) { break; }
				}
			}
			else
			{
				if(this.index < 0){ return; }

				while (elapsedTime <= this.eventLists[this.index].ElapsedTime)
				{
					executionEventList = this.ModifyExecutionEventList(this.eventLists[this.index]);

					this.index--;

					if (this.index < 0) { break; }
				}

				// Execute latest event
				if(this.index >= 0){ executionEventList = this.ModifyExecutionEventList(this.eventLists[this.index]); }
			}

			if (executionEventList == null) { return; }

			List<TPlaybackEvent> executionEvents = executionEventList.EventList;

			foreach(TPlaybackEvent executionEvent in executionEvents)
			{
				executionEvent.Execute();
			}
		}


		protected virtual TPlaybackEventList ModifyExecutionEventList(TPlaybackEventList executionEventList)
		{
			return executionEventList;
		}


		public void ExecuteLatestEvents()
		{
			if(this.eventLists.Count > 0)
			{
				int index = (this.index < 0)? 0 : this.index;

				foreach (TPlaybackEvent playbackEvent in this.eventLists[index].EventList)
				{
					playbackEvent.Execute();
				}
			}
		}

		public void ExecuteFirstEvent()
		{
			if(this.eventLists.Count > 0)
			{
				foreach (TPlaybackEvent playbackEvent in this.eventLists[0].EventList)
				{
					playbackEvent.Execute();
				}
			}
		}

		public TPlaybackEventList GetFirstEvents()
		{
			return this.eventLists[0];
		}


		protected bool IsEventDataFinished()
		{
			return this.index >= this.eventLists.Count-1;
		}

		public void UpdateIndex(float elapsedTime)
		{
			for(int index=0; index < this.eventLists.Count; index++)
			{
				if(this.eventLists[index].ElapsedTime > elapsedTime)
				{
					this.index = index-1;
					return;
				}
			}

			this.index = this.eventLists.Count-1;
		}

		public float GetTotalTime()
		{
			return (this.eventLists.Count > 0) ? this.eventLists.Last().ElapsedTime : 0.0f;
		}
	}
}

