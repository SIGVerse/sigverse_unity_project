using SIGVerse.Human.VR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SIGVerse.Common
{
	public class HumanInitializer : CommonInitializer
	{
		public GameObject avatar;
		public GameObject xrOrigin;
		public GameObject personalPanel;

		private bool isInitialized = false;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			this.Initialize();
		}

		void Start()
		{
			this.Initialize();
		}

		private void Initialize()
		{
			// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkbehavior/#spawning

			if (this.isInitialized) { return; }

			this.isInitialized = true;

			if (IsOwner || !IsSpawned)
			{
				this.personalPanel.SetActive(true);

				this.avatar.GetComponent<SimpleHumanVRControllerNgo>().enabled = true;

				this.xrOrigin.GetComponentInChildren<XROrigin>().enabled = true;
				this.xrOrigin.GetComponentInChildren<InputActionManager>().enabled = true;

				this.xrOrigin.GetComponentInChildren<Camera>().enabled = true;
				this.xrOrigin.GetComponentInChildren<AudioListener>().enabled = true;
				Array.ForEach(this.xrOrigin.GetComponentsInChildren<TrackedPoseDriver>(), x => x.enabled = true);
				this.xrOrigin.GetComponentInChildren<SIGVerse.Human.IK.AnchorPostureCalculator>().enabled = true;

				Array.ForEach(this.xrOrigin.GetComponentsInChildren<HapticImpulsePlayer>(), x => x.enabled = true);
				Array.ForEach(this.xrOrigin.GetComponentsInChildren<XRDirectInteractor>(), x=>x.enabled = true);
				this.xrOrigin.GetComponentsInChildren<SphereCollider>()
					.Where(col => !col.GetComponent<XRDirectInteractor>()) //If an XRDirectInteractor is attached to the same GameObject, do nothing.
					.ToList().ForEach(col => col.enabled = true);
				Array.ForEach(this.xrOrigin.GetComponentsInChildren<SimpleHapticFeedback>(), x=>x.enabled = true);
				Array.ForEach(this.xrOrigin.GetComponentsInChildren<GraspableOutlineRenderer>(), x=>x.enabled = true);

				SubviewController.EnableSubview(this.gameObject);
				if (TryGetComponent<HumanChat>(out var chat)) 
				{ 
					chat.enabled = true;
					chat.SendAddChatUser();
				}
			}
		}
	}
}
