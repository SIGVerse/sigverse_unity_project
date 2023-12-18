using SIGVerse.Common.Recorder;
using SIGVerse.ExampleScenes.Hsr.HsrCleanupVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ExampleScenes.MotionRecorder
{
	public class MessageRecorder : MonoBehaviour, 
#if SIGVERSE_PUN
		SIGVerse.ExampleScenes.Hsr.HsrCleanupVR.IChatMessageHandler
#else
		SIGVerse.Common.IChatMessageHandler
#endif
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

