using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Common
{
	public class GraspableOutlineRenderer : MonoBehaviour
	{
		public const string OutlineGraspableLayerName = "OutlineYellow";
		public const string OutlineGraspedLayerName   = "OutlineGreen";

		private const string TagGraspable = "Graspable";

		private List<Rigidbody> outlinedObjects = new List<Rigidbody>();

		private Rigidbody graspedObj = null;

		public bool SetGraspedObject(Rigidbody graspedObject)
		{
			if (!this.outlinedObjects.Contains(graspedObject)) { return false; }
			
			MeshRenderer[] renderers = graspedObject.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in renderers)
			{
				renderer.renderingLayerMask &= ~RenderingLayerMask.GetMask(OutlineGraspableLayerName);
				renderer.renderingLayerMask |= RenderingLayerMask.GetMask(OutlineGraspedLayerName);
			}

			this.graspedObj = graspedObject;
			return true;
		}

		public void UnsetGraspedObject()
		{
			if (this.graspedObj == null) { return; }

			MeshRenderer[] renderers = this.graspedObj.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in renderers)
			{
				renderer.renderingLayerMask &= ~RenderingLayerMask.GetMask(OutlineGraspedLayerName);
				renderer.renderingLayerMask |= RenderingLayerMask.GetMask(OutlineGraspableLayerName);
			}
		}

		public bool AddGraspableObj(Rigidbody graspableObj)
		{
			if (this.outlinedObjects.Contains(graspableObj)) { return false; }

			MeshRenderer[] renderers = graspableObj.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in renderers)
			{
				string layerMaskName = (this.graspedObj==graspableObj)? OutlineGraspedLayerName : OutlineGraspableLayerName;
				renderer.renderingLayerMask |= RenderingLayerMask.GetMask(layerMaskName); // This outline will also be displayed on other cameras with the same script attached.
			}

			this.outlinedObjects.Add(graspableObj);

			return true;
		}

		public bool RemoveGraspableObj(Rigidbody graspableObj)
		{
			if ( !this.outlinedObjects.Contains(graspableObj) ) { return false; }

			MeshRenderer[] renderers = graspableObj.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in renderers)
			{
				renderer.renderingLayerMask &= ~RenderingLayerMask.GetMask(OutlineGraspableLayerName);
				renderer.renderingLayerMask &= ~RenderingLayerMask.GetMask(OutlineGraspedLayerName);
			}
			
			this.outlinedObjects.Remove(graspableObj);

			if (this.graspedObj!=null && graspableObj==this.graspedObj) { this.graspedObj = null; }

			return true;
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody == null) { return; }

			Debug.Log(other.name + ", other.attachedRigidbody="+other.attachedRigidbody);

			if(other.attachedRigidbody.tag==TagGraspable)
			{
				this.AddGraspableObj(other.attachedRigidbody);
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.attachedRigidbody == null) { return; }

			if (other.attachedRigidbody.tag==TagGraspable)
			{
				this.RemoveGraspableObj(other.attachedRigidbody);
			}
		}
	}
}
