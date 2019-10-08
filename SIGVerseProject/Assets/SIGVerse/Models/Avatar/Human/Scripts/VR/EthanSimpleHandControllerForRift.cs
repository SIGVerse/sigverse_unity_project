using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SIGVerse.Human.VR
{
	public class EthanSimpleHandControllerForRift : MonoBehaviour
	{
#if SIGVERSE_OCULUS
		public enum HandType
		{
			LeftHand,
			RightHand,
		}

		public HandType  handType;
		public Transform hand;

		//-----------
		private Transform thumb1, index1, middle1, ring1, pinky1;
		private Transform thumb2, index2, middle2, ring2, pinky2;
		private Transform thumb3, index3, middle3, ring3, pinky3;

		private Quaternion thumb1Start , thumb1End;
		private Quaternion index1Start , index1End;
		private Quaternion middle1Start, middle1End, middle2Start, middle2End;
		private Quaternion ring1Start  , ring1End;
		private Quaternion pinky1Start , pinky1End;

		private Quaternion thumb2Start , thumb3Start;
		private Quaternion index2Start , index3Start;
		private Quaternion               middle3Start;
		private Quaternion ring2Start  , ring3Start;
		private Quaternion pinky2Start , pinky3Start;


		void Awake()
		{
			string typeStr = (this.handType == HandType.LeftHand)? "Left" : "Right";

			this.thumb1  = this.hand.Find("Ethan"+typeStr+"HandThumb1");
			this.thumb2  = this.hand.Find("Ethan"+typeStr+"HandThumb1/Ethan"+typeStr+"HandThumb2");  
			this.thumb3  = this.hand.Find("Ethan"+typeStr+"HandThumb1/Ethan"+typeStr+"HandThumb2/Ethan"+typeStr+"HandThumb3");  
			this.index1  = this.hand.Find("Ethan"+typeStr+"HandIndex1");
			this.index2  = this.hand.Find("Ethan"+typeStr+"HandIndex1/Ethan"+typeStr+"HandIndex2");  
			this.index3  = this.hand.Find("Ethan"+typeStr+"HandIndex1/Ethan"+typeStr+"HandIndex2/Ethan"+typeStr+"HandIndex3");  
			this.middle1 = this.hand.Find("Ethan"+typeStr+"HandMiddle1");
			this.middle2 = this.hand.Find("Ethan"+typeStr+"HandMiddle1/Ethan"+typeStr+"HandMiddle2");  
			this.middle3 = this.hand.Find("Ethan"+typeStr+"HandMiddle1/Ethan"+typeStr+"HandMiddle2/Ethan"+typeStr+"HandMiddle3");  
			this.ring1   = this.hand.Find("Ethan"+typeStr+"HandRing1");
			this.ring2   = this.hand.Find("Ethan"+typeStr+"HandRing1/Ethan"+typeStr+"HandRing2");  
			this.ring3   = this.hand.Find("Ethan"+typeStr+"HandRing1/Ethan"+typeStr+"HandRing2/Ethan"+typeStr+"HandRing3");  
			this.pinky1  = this.hand.Find("Ethan"+typeStr+"HandPinky1");
			this.pinky2  = this.hand.Find("Ethan"+typeStr+"HandPinky1/Ethan"+typeStr+"HandPinky2");  
			this.pinky3  = this.hand.Find("Ethan"+typeStr+"HandPinky1/Ethan"+typeStr+"HandPinky2/Ethan"+typeStr+"HandPinky3");  
		}


		// Use this for initialization
		void Start()
		{
			this.thumb1Start  = this.thumb1 .localRotation;
			this.thumb2Start  = this.thumb2 .localRotation;
			this.thumb3Start  = this.thumb3 .localRotation;
			this.index1Start  = this.index1 .localRotation;
			this.index2Start  = this.index2 .localRotation;
			this.index3Start  = this.index3 .localRotation;
			this.middle1Start = this.middle1.localRotation;
			this.middle2Start = this.middle2.localRotation;
			this.middle3Start = this.middle3.localRotation;
			this.ring1Start   = this.ring1  .localRotation;
			this.ring2Start   = this.ring2  .localRotation;
			this.ring3Start   = this.ring3  .localRotation;
			this.pinky1Start  = this.pinky1 .localRotation;
			this.pinky2Start  = this.pinky2 .localRotation;
			this.pinky3Start  = this.pinky3 .localRotation;

			float xySign = (this.handType == HandType.LeftHand)? 1.0f : -1.0f;

			this.thumb1End  = Quaternion.Euler(xySign * (+49.32f), xySign * (-125.52f), -39.94f);
			this.index1End  = Quaternion.Euler(xySign * (+ 8.43f), xySign * (- 28.12f), +28.70f);
			this.middle1End = Quaternion.Euler(xySign * (- 5.20f), xySign * (-  4.96f), +23.35f);
			this.middle2End = Quaternion.Euler(xySign * (- 1.57f), xySign * (- 10.52f), - 8.52f);
			this.ring1End   = Quaternion.Euler(xySign * (+ 2.71f), xySign * (- 13.94f), +19.27f);
			this.pinky1End  = Quaternion.Euler(xySign * (+14.85f), xySign * (- 11.87f), - 8.14f);
		}


		// Update is called once per frame
		void LateUpdate()
		{
			float handTrigger1D = (this.handType == HandType.LeftHand)? OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) : OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

			// Change hand posture
			this.thumb1 .localRotation = Quaternion.Slerp(this.thumb1Start , this.thumb1End , handTrigger1D);
			this.index1 .localRotation = Quaternion.Slerp(this.index1Start , this.index1End , handTrigger1D);
			this.middle1.localRotation = Quaternion.Slerp(this.middle1Start, this.middle1End, handTrigger1D);
			this.middle2.localRotation = Quaternion.Slerp(this.middle2Start, this.middle2End, handTrigger1D);
			this.ring1  .localRotation = Quaternion.Slerp(this.ring1Start  , this.ring1End  , handTrigger1D);
			this.pinky1 .localRotation = Quaternion.Slerp(this.pinky1Start , this.pinky1End , handTrigger1D);

			this.thumb2 .localRotation = this.thumb2Start;
			this.thumb3 .localRotation = this.thumb3Start;
			this.index2 .localRotation = this.index2Start;
			this.index3 .localRotation = this.index3Start;
			this.middle3.localRotation = this.middle3Start;
			this.ring2  .localRotation = this.ring2Start;
			this.ring3  .localRotation = this.ring3Start;
			this.pinky2 .localRotation = this.pinky2Start;
			this.pinky3 .localRotation = this.pinky3Start;
		}
#endif
	}
}

