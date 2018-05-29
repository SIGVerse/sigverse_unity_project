using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.SampleScenes.TurtleBot3
{
	public class MoveArm : MonoBehaviour
	{
		public Transform  rightArmObject;
		public GameObject destinationOfRosMessage;

		/////////////

		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{
			// Move the right arm
			if(Input.GetKey(KeyCode.RightArrow))
			{
				float newY = Mathf.Max(this.rightArmObject.localEulerAngles.y - 30.0f * Time.deltaTime, 285.0f);

				this.rightArmObject.localEulerAngles = new Vector3(this.rightArmObject.localEulerAngles.x, newY, this.rightArmObject.localEulerAngles.z);
			}

			// Move the right arm
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				float newY = Mathf.Min(this.rightArmObject.localEulerAngles.y + 30.0f * Time.deltaTime, 358.0f);

				this.rightArmObject.localEulerAngles = new Vector3(this.rightArmObject.localEulerAngles.x, newY, this.rightArmObject.localEulerAngles.z);
			}

			// Send the instruction message
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ExecuteEvents.Execute<SIGVerse.RosBridge.IRosBridgeStringHandler>
				(
					target: this.destinationOfRosMessage, 
					eventData: null, 
					functor: (reciever, eventData) => reciever.OnReceiveMessage("Rotate the arm to this side")
				);

				SIGVerseLogger.Info("Sent the instruction message");
			}
		}
	}
}

