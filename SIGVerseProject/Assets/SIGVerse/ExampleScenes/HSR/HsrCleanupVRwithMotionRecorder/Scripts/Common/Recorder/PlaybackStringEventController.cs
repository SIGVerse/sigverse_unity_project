using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

namespace SIGVerse.Common.Recorder
{
	public interface IPlaybackStringHandler : IEventSystemHandler
	{
		void OnReceiveString(string stringData);
	}

	public class PlaybackStringEvent : PlaybackEventBase
	{
		public string StringData { get; set; }
		public GameObject[] Destinations{ get; set; }

		public void Execute()
		{
			foreach(GameObject destionation in this.Destinations)
			{
				ExecuteEvents.Execute<IPlaybackStringHandler>
				(
					target: destionation,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveString(StringData)
				);
			}
		}
	}


	public class PlaybackStringEventList : PlaybackEventListBase<PlaybackStringEvent>
	{
		public PlaybackStringEventList()
		{
			this.EventList = new List<PlaybackStringEvent>();
		}

		public PlaybackStringEventList(PlaybackStringEventList stringEventList)
		{
			this.ElapsedTime = stringEventList.ElapsedTime;
			this.EventList   = new List<PlaybackStringEvent>();

			foreach(PlaybackStringEvent stringEventOrg in stringEventList.EventList)
			{
				PlaybackStringEvent stringEvent = new PlaybackStringEvent();
				stringEvent.StringData   = stringEventOrg.StringData;
				stringEvent.Destinations = stringEventOrg.Destinations;

				this.EventList.Add(stringEvent);
			}
		}
	}

	// ------------------------------------------------------------------

	public class PlaybackStringEventController : PlaybackEventControllerBase<PlaybackStringEventList, PlaybackStringEvent>
	{
		private GameObject[] destinations;

		public PlaybackStringEventController(GameObject[] destinations)
		{
			this.destinations = destinations;
		}

		public override void StartInitializingEvents()
		{
			this.eventLists = new List<PlaybackStringEventList>();
		}

		public override bool ReadEvents(string[] headerArray, string dataStr)
		{
			// String Data
			if (headerArray[1] == WorldPlaybackCommon.DataType1String)
			{
				PlaybackStringEvent stringEvent = new PlaybackStringEvent();

				string[] dataArray = dataStr.Split('\t');

				string stringData  = Regex.Unescape(dataArray[0]);

				stringEvent.StringData   = stringData;
				stringEvent.Destinations = this.destinations;

				PlaybackStringEventList stringEventList = new PlaybackStringEventList();
				stringEventList.ElapsedTime = float.Parse(headerArray[0]);
				stringEventList.EventList.Add(stringEvent);

				this.eventLists.Add(stringEventList);

				return true;
			}

			return false;
		}


		public static string GetDataLine(string elapsedTime, string stringData)
		{
			string dataLine = elapsedTime + "," + WorldPlaybackCommon.DataType1String;

			dataLine += "\t" + Regex.Escape(stringData);

			return dataLine;
		}
	}
}

