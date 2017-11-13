using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Human.IK
{
	public class AnchorPostureCalculator : MonoBehaviour
	{
		private const float NeckAngleRatioXUp   = 0.0f;
		private const float NeckAngleRatioXDown = 0.5f;
		private const float NeckAngleRatioZ     = 0.1f;

		public Transform eye;
		public Transform neck;
		public Transform body;
		public Transform leftKnee;
		public Transform rightKnee;

		private Quaternion preRot;

		// Use this for initialization
		void Start ()
		{
			this.preRot = this.eye.rotation;
		}
	
		// Update is called once per frame
		void Update ()
		{
			if(this.eye.up.y <= 0.1f)
			{
				this.eye.rotation = this.preRot;
				return;
			}

			float headAngleX = Vector3.Angle(this.eye.forward, Vector3.up) - 90.0f;
			float headAngleZ = Vector3.Angle(this.eye.right,   Vector3.up) - 90.0f;

			float neckAngleRatioX = (headAngleX > 0) ? NeckAngleRatioXDown : NeckAngleRatioXUp;

			float neckAngleX = headAngleX * neckAngleRatioX;
			float neckAngleZ = headAngleZ * NeckAngleRatioZ;

			float neckEulerAngleX = +neckAngleX - headAngleX;
			float neckEulerAngleZ = -neckAngleZ + headAngleZ;

			this.neck.localEulerAngles = new Vector3(neckEulerAngleX, 0.0f, neckEulerAngleZ);

			this.body.rotation = Quaternion.LookRotation(new Vector3(this.eye.forward.x, 0.0f, this.eye.forward.z));

			this.leftKnee .position = new Vector3(this.leftKnee .position.x, this.body.position.y / 2.0f, this.leftKnee .position.z);
			this.rightKnee.position = new Vector3(this.rightKnee.position.x, this.body.position.y / 2.0f, this.rightKnee.position.z);

			this.preRot = this.eye.rotation;
		}
	}
}

