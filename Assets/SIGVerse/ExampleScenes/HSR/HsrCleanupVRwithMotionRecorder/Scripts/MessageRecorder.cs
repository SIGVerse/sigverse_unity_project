using SIGVerse.Common;
using SIGVerse.Common.Recorder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ExampleScenes.MotionRecorder
{
	public class MessageRecorder : MonoBehaviour, IChatMessageHandler
	{
		public GameObject motionRecorder;

		public void OnReceiveChatMessage(string senderName, string message)
		{
			ExecuteEvents.Execute<IPlaybackStringHandler>
			(
				target: this.motionRecorder,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnReceiveString(senderName + "\t" + message)
			);
		}
	}
}

