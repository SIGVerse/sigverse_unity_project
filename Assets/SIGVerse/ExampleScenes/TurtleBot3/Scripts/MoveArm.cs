using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.ExampleScenes.TurtleBot3
{
	public class MoveArm : MonoBehaviour
	{
		public Transform  rightArmObject;
		public GameObject destinationOfRosMessage;

		/////////////
		[SerializeField] private float yawSpeed = 60f; // Rotation speed (deg/sec)
		[SerializeField] private float yawLimit = 30f; // +/- limit from initial (deg)

		private Quaternion initialLocalRot;
		private float yawOffsetDeg = 0f;
	
		private void Start()
		{
			this.initialLocalRot = this.rightArmObject.localRotation;
		}

		// Update is called once per frame
		void Update ()
		{
			float input = 0f;
			if (Input.GetKey(KeyCode.RightArrow)){ input -= 1f; }
			if (Input.GetKey(KeyCode.LeftArrow)) { input += 1f; }

			// Update only when there is some input
			if (Mathf.Abs(input) > 0f)
			{
				this.yawOffsetDeg += input * this.yawSpeed * Time.deltaTime;
				this.yawOffsetDeg = Mathf.Clamp(this.yawOffsetDeg, -this.yawLimit, this.yawLimit);

				// Apply rotation: use the startup rotation combined with relative yaw around local Y
				this.rightArmObject.localRotation = this.initialLocalRot * Quaternion.Euler(0f, this.yawOffsetDeg, 0f);
			}

			// Send the instruction message
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ExecuteEvents.Execute<SIGVerse.RosBridge.IRosSendingStringMsgHandler>
				(
					target: this.destinationOfRosMessage, 
					eventData: null, 
					functor: (reciever, eventData) => reciever.OnSendRosStringMsg("Rotate the arm to this side")
				);

				SIGVerseLogger.Info("Sent the instruction message");
			}
		}
	}
}

