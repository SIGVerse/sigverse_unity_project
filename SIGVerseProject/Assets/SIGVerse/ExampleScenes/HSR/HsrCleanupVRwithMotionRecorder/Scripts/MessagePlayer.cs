using SIGVerse.Common.Recorder;
using SIGVerse.ExampleScenes.Hsr;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ExampleScenes.MotionRecorder
{
	public class MessagePlayer : MonoBehaviour, IPlaybackStringHandler
	{
		public GameObject mainMenu;

		public void OnReceiveString(string stringData)
		{
			string[] stringArray = stringData.Split('\t');

			PanelNoticeStatus panelNoticeStatus = new PanelNoticeStatus(stringArray[0], stringArray[1], PanelNoticeStatus.Green);

			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(panelNoticeStatus)
			);
		}
	}
}

