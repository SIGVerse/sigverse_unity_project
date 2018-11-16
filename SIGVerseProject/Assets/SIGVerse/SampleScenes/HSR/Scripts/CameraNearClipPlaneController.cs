using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	public class CameraNearClipPlaneController : MonoBehaviour
	{
		public Camera targetCamera;

		public Transform rayTarget;

		public LayerMask layerMask;

		public bool showDebugRay = false;

		//--------------------------------------------------

		private float defaultNearClipPlane;
		

		void Awake()
		{
			this.defaultNearClipPlane = this.targetCamera.nearClipPlane;
		}

		void LateUpdate()
		{
			Vector3 rayDirection = this.targetCamera.transform.position - this.rayTarget.position;

			Ray ray = new Ray(this.rayTarget.position, rayDirection);

			RaycastHit hit;

			Physics.Raycast(ray, out hit, 5.0f, layerMask);

//			Debug.Log("hit distance: " + hit.distance);

			float newNearClipPlane;

			if (hit.distance != 0 && hit.distance < rayDirection.magnitude - this.defaultNearClipPlane)
			{
				newNearClipPlane = rayDirection.magnitude - hit.distance;
			}
			else
			{
				newNearClipPlane = defaultNearClipPlane;
			}

			if(this.targetCamera.nearClipPlane != newNearClipPlane)
			{
				this.targetCamera.nearClipPlane = Mathf.Clamp(newNearClipPlane, this.targetCamera.nearClipPlane - 0.01f, this.targetCamera.nearClipPlane + 0.01f);
			}

			if (this.showDebugRay)
			{
				Debug.DrawRay(ray.origin, ray.direction, Color.red);
			}
		}
	}
}
