using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem.XR;
using SIGVerse.Human.VR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Feedback;

namespace SIGVerse.Common
{
	public class HumanInitializer : CommonInitializer
	{
		public GameObject avatar;
		public GameObject xritkSetup;
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

				this.xritkSetup.GetComponentInChildren<XROrigin>().enabled = true;
				this.xritkSetup.GetComponentInChildren<InputActionManager>().enabled = true;

				this.xritkSetup.GetComponentInChildren<Camera>().enabled = true;
				this.xritkSetup.GetComponentInChildren<AudioListener>().enabled = true;
				Array.ForEach(this.xritkSetup.GetComponentsInChildren<TrackedPoseDriver>(), x => x.enabled = true);
				this.xritkSetup.GetComponentInChildren<SIGVerse.Human.IK.AnchorPostureCalculator>().enabled = true;

				Array.ForEach(this.xritkSetup.GetComponentsInChildren<HapticImpulsePlayer>(), x => x.enabled = true);
				Array.ForEach(this.xritkSetup.GetComponentsInChildren<XRDirectInteractor>(), x=>x.enabled = true);
				Array.ForEach(this.xritkSetup.GetComponentsInChildren<SphereCollider>(), x=>x.enabled = true);
				Array.ForEach(this.xritkSetup.GetComponentsInChildren<SimpleHapticFeedback>(), x=>x.enabled = true);
				Array.ForEach(this.xritkSetup.GetComponentsInChildren<GraspableOutlineRenderer>(), x=>x.enabled = true);

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
