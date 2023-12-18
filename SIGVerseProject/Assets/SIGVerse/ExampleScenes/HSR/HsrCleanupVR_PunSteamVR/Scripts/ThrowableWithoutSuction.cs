using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if SIGVERSE_STEAMVR
using Valve.VR.InteractionSystem;
#endif

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_STEAMVR && SIGVERSE_PUN
	public class ThrowableWithoutSuction : Throwable
#else
	public class ThrowableWithoutSuction : MonoBehaviour
#endif
	{
		public bool useSIGVerseDefault = true;

#if SIGVERSE_STEAMVR && SIGVERSE_PUN

		private Rigidbody[] rigidbodies;
		private PhotonView photonView;

		protected override void Awake()
		{
			if(this.useSIGVerseDefault)
			{
				this.SetSIGVerseDefault();
			}

			base.Awake();

			this.photonView = this.GetComponent<PhotonView>();

			this.rigidbodies = this.GetComponentsInChildren<Rigidbody>();
		}

		private void SetSIGVerseDefault()
		{
			this.attachmentFlags = Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.VelocityMovement | Hand.AttachmentFlags.TurnOffGravity;

			this.releaseVelocityStyle = ReleaseStyle.AdvancedEstimation;
		}

		protected override void OnHandHoverBegin(Hand hand)
		{
			this.attachmentOffset = hand.transform; // If this line set in OnAttachedToHand, only the first grasp will have unnatural behavior.

			base.OnHandHoverBegin(hand);
		}

		protected override void OnAttachedToHand(Hand hand)
		{
			base.OnAttachedToHand(hand);

			this.ChangeOwner();

			this.photonView.RPC("UseGravityRPC", RpcTarget.Others, false);
		}

		/// <summary>
		/// Change PUN Owner
		/// </summary>
		public void ChangeOwner()
		{
			this.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}

		protected override void OnDetachedFromHand(Hand hand)
		{
			base.OnDetachedFromHand(hand);

			this.photonView.RPC("UseGravityRPC", RpcTarget.Others, true);
		}

		[PunRPC]
		protected void UseGravityRPC(bool useGravity)
		{
			if(this.photonView.Owner != PhotonNetwork.LocalPlayer)
			{
				foreach(Rigidbody rigidbody in this.rigidbodies)
				{
					rigidbody.useGravity = useGravity;
				}
			}
		}
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ThrowableWithoutSuction))]
	public class ThrowableWithoutSuctionEditor : Editor
	{
		private ThrowableWithoutSuction throwableWithoutSuction;

		private void Awake()
		{
			this.throwableWithoutSuction = (ThrowableWithoutSuction)target;
		}

		public override void OnInspectorGUI()
		{
			ThrowableWithoutSuction ThrowableWithoutSuction = (ThrowableWithoutSuction)target;

			this.throwableWithoutSuction.useSIGVerseDefault = EditorGUILayout.ToggleLeft("Use SIGVerse Default", this.throwableWithoutSuction.useSIGVerseDefault);

			if(!ThrowableWithoutSuction.useSIGVerseDefault)
			{
				base.OnInspectorGUI();
			}
		}
	}
#endif
}
