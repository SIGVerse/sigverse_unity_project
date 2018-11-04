using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	public class TrackingCameraController : MonoBehaviour
	{
		public Transform target;

		public Vector3 offset;

		private Vector3    relativePosition;
		private Quaternion relativeRotation;

		void Awake()
		{
//			this.relativePosition = this.transform.position - target.position;
//			this.relativeRotation = Quaternion.Inverse(target.rotation) * this.transform.rotation;
		}

		void LateUpdate()
		{
			this.transform.position = this.target.position + this.offset;
//			this.transform.position = this.target.position + this.relativePosition;
//			this.transform.rotation = this.target.rotation * this.relativeRotation;
		}
	}
}
