using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


namespace SIGVerse.Common
{
	/// <summary>
	/// Modified XRGrabInteractable because the parent of the interactable object is changed.
	/// See below for details.
	/// https://forum.unity.com/threads/xr-interactables-parent-becomes-root.1014748/
	/// </summary>
	public partial class XRGrabInteractableNoParentChange : XRGrabInteractable
	{
		public bool changeTransformParent = false;

		protected override void Awake()
		{
			if (!this.changeTransformParent)
			{
				this.retainTransformParent = false;
			}
			base.Awake();
		}

		protected override void Grab()
		{
			if(this.changeTransformParent)
			{
				base.Grab();
			}
			else
			{
				Type type = typeof(XRGrabInteractable);

				MethodInfo mUpdateCurrentMovementType = type.GetMethod("UpdateCurrentMovementType", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo fM_Rigidbody = type.GetField("m_Rigidbody", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo fM_DetachLinearVelocity = type.GetField("m_DetachLinearVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo fM_DetachAngularVelocity = type.GetField("m_DetachAngularVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
				MethodInfo mInitializeTargetPoseAndScale = type.GetMethod("InitializeTargetPoseAndScale", BindingFlags.NonPublic | BindingFlags.Instance);

				//var thisTransform = transform;
				//m_OriginalSceneParent = thisTransform.parent;
				//thisTransform.SetParent(null);
				
				mUpdateCurrentMovementType.Invoke(this, null);
				SetupRigidbodyGrab((Rigidbody)(fM_Rigidbody.GetValue(this)));

				// Reset detach velocities
				fM_DetachLinearVelocity.SetValue(this, Vector3.zero);
				fM_DetachAngularVelocity.SetValue(this, Vector3.zero);

				// Initialize target pose and scale
				mInitializeTargetPoseAndScale.Invoke(this, new object[] { transform });
			}
		}
	}
}

