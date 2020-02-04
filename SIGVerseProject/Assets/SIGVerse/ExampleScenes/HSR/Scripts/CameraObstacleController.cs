using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Competition
{
	public class CameraObstacleController : MonoBehaviour
	{
		public Transform rayTarget;

		public LayerMask layerMask;

		public Material transparentMaterial;

		//--------------------------------------------------

		private Camera targetCamera;

		private MeshRenderer targetMeshRenderer;

		private Material[] savedMaterials;

		void Awake()
		{
			this.targetCamera = this.GetComponent<Camera>();
		}

		private void OnPreRender()
		{
			Vector3 rayDirection = this.targetCamera.transform.position - this.rayTarget.position;

			Ray ray = new Ray(this.rayTarget.position, rayDirection);

			RaycastHit hit;

			Physics.Raycast(ray, out hit, 5.0f, layerMask);

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

		private void OnPostRender()
		{
			if (this.targetMeshRenderer != null)
			{
				this.targetMeshRenderer.materials = this.savedMaterials;

				this.targetMeshRenderer = null;
			}
		}

		private static Material[] CreateTemporaryMaterials(Material[] sourceMaterials, Material transparentMaterial)
		{
			Material[] tmpMaterials = new Material[sourceMaterials.Length];

			for (int i = 0; i < sourceMaterials.Length; i++)
			{
				tmpMaterials[i] = new Material(transparentMaterial);

				if (sourceMaterials[i].shader.name == "Standard")
				{
					Color srcColor = sourceMaterials[i].GetColor("_Color");
					tmpMaterials[i].SetColor("_Color", new Color(srcColor.r, srcColor.g, srcColor.b, transparentMaterial.GetColor("_Color").a));
				}
			}

			return tmpMaterials;
		}
	}
}

