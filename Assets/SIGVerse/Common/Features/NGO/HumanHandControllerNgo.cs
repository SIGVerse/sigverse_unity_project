using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;
using UnityEngine.XR;

namespace SIGVerse.Common
{
	public class HumanHandControllerNgo : HumanHandController
	{
		private NetworkObject networkObject;
		private HandDataSync handDataSync;

		public override void Awake()
		{
			base.Awake();

			this.handDataSync = this.GetComponent<HandDataSync>();
		}

		public override void Start()
		{
			this.networkObject = GetComponent<NetworkObject>();
			
			if(this.networkObject.IsOwner || !this.networkObject.IsSpawned)
			{
				if(this.handType==HandType.Left) { StartCoroutine(GetXrDevice(XRNode.LeftHand)); }
				if(this.handType==HandType.Right){ StartCoroutine(GetXrDevice(XRNode.RightHand)); }
			}

			InitializeFingerPostures();
		}

		protected override float GetHandPostureRatio()
		{
			if(this.networkObject.IsOwner || !this.networkObject.IsSpawned)
			{
				if(this.handDevice.TryGetFeatureValue(CommonUsages.grip, out float handTriggerValue))
				{
					if(this.handType==HandType.Left) { this.handDataSync.SetLeftHandPostureRatio (handTriggerValue); }
					if(this.handType==HandType.Right){ this.handDataSync.SetRightHandPostureRatio(handTriggerValue); }
				}
			}

			if(this.handType==HandType.Left) { return this.handDataSync.GetLeftHandPostureRatio(); }
			if(this.handType==HandType.Right){ return this.handDataSync.GetRightHandPostureRatio(); }

			throw new Exception("Invalid HandType="+this.handType);
		}
	}
}

