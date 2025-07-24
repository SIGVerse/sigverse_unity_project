using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SIGVerse.Common
{
	public class CameraObstacleFader: MonoBehaviour
	{
		[SerializeField]
		private Transform rayTarget;

		[SerializeField]
		private LayerMask layerMask;

		[SerializeField]
		private Material transparentMaterial;

		//--------------------------------------------------
		private Camera targetCamera;
		private MeshRenderer targetMeshRenderer;
		private Material[] savedMaterials;

		void OnEnable()
		{
			this.targetCamera = this.GetComponent<Camera>();

			if (this.rayTarget==null || this.transparentMaterial==null || this.targetCamera == null) 
			{
				Debug.LogError("Initialization failed. ("+typeof(CameraObstacleFader).Name+")");
				return;
			}

			RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
			RenderPipelineManager.endCameraRendering   += OnEndCameraRendering;
		}

		void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
			RenderPipelineManager.endCameraRendering   -= OnEndCameraRendering;
		}

		private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
		{
			if (camera!=this.targetCamera) { return; }

			// Raycast from rayTarget to camera
			Vector3 rayDirection = this.targetCamera.transform.position - this.rayTarget.position;

			Ray ray = new Ray(this.rayTarget.position, rayDirection);

			RaycastHit hit;

			Physics.Raycast(ray, out hit, rayDirection.magnitude, layerMask);

			if (hit.distance != 0 && hit.distance < rayDirection.magnitude - this.targetCamera.nearClipPlane)
			{
				this.targetMeshRenderer = hit.collider.gameObject.GetComponentInChildren<MeshRenderer>(); // Target only one MeshRenderer.

				if (this.targetMeshRenderer != null)
				{
					this.savedMaterials = this.targetMeshRenderer.materials;

					this.targetMeshRenderer.materials = CreateTemporaryMaterials(this.savedMaterials, this.transparentMaterial);
				}
			}
		}

		private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
		{
			if (camera!=this.targetCamera) { return; }

			if (this.targetMeshRenderer != null)
			{
				this.targetMeshRenderer.materials = this.savedMaterials;

				this.targetMeshRenderer = null;
			}
		}

		private static Material[] CreateTemporaryMaterials(Material[] sourceMaterials, Material transparentMaterial)
		{
			Material[] tmpMaterials = new Material[sourceMaterials.Length];

			for (int i = 0;i < sourceMaterials.Length;i++)
			{
				tmpMaterials[i] = new Material(transparentMaterial);
			}

			return tmpMaterials;
		}
	}
}
