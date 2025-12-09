using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SIGVerse.Common
{
	public class HsrInitializer : CommonInitializer
	{
		public GameObject rosBridgeScripts;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			this.EnableScripts();
		}

		private void EnableScripts()
		{
			if (IsOwner || !IsSpawned)
			{
				this.rosBridgeScripts.SetActive(true);

				SubviewController.EnableSubview(this.gameObject);

				this.GetComponent<AudioSource>().enabled = true;
				this.GetComponentInChildren<AudioListener>().enabled = true;

				this.GetComponentInChildren<CollisionDetector>(true).enabled = true;
				Array.ForEach(GetComponentsInChildren<GripperTriggerTransfererRay>(true), x=>x.enabled = true);

				this.GetComponent<GraspingDetectorNgo>().enabled = true;
//				this.GetComponent<SuctionDetectorNgo>().enabled = true;

				if (TryGetComponent<RobotChat>(out var chat)) 
				{ 
					chat.enabled = true;
					chat.SendAddChatUser();
				}
			}
			else
			{
				this.GetComponentInChildren<CollisionDetector>().enabled = false;
			}
		}
	}
}
