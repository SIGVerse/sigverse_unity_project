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
		public Transform leftFoot;
		public Transform rightFoot;

		public Transform leftWaist;
		public Transform rightWaist;

		public Transform leftKneeHint;
		public Transform rightKneeHint;

		private Quaternion preRot;

		private Vector3 initialBodyLocalPos;

		// Use this for initialization
		void Start ()
		{
			this.preRot = this.eye.rotation;

			this.initialBodyLocalPos = this.body.localPosition;
		}
	
		// Update is called once per frame
		void LateUpdate ()
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
			this.body.localPosition = this.initialBodyLocalPos;

			//if (this.neck.position.y < 1.2f)
			//{
			//	float bodyAngle = (1.2f - this.neck.position.y) * 90f / 1.2f;

			//	this.body.rotation *= Quaternion.Euler(bodyAngle, 0, 0);
			//	this.body.localPosition = Mathf.Abs(this.initialBodyLocalPos.y) * new Vector3(0f, -Mathf.Cos(Mathf.Deg2Rad * bodyAngle), -Mathf.Sin(Mathf.Deg2Rad * bodyAngle));
			//}

			this.leftKnee .position = new Vector3(this.leftKneeHint .position.x, Mathf.Max(this.body.position.y, 0.2f), this.leftKneeHint .position.z);
			this.rightKnee.position = new Vector3(this.rightKneeHint.position.x, Mathf.Max(this.body.position.y, 0.2f), this.rightKneeHint.position.z);

			this.leftFoot .position = new Vector3((this.neck.position.x + this.leftWaist .position.x)/2, 0.0f, (this.neck.position.z + this.leftWaist .position.z)/2);
			this.rightFoot.position = new Vector3((this.neck.position.x + this.rightWaist.position.x)/2, 0.0f, (this.neck.position.z + this.rightWaist.position.z)/2);

			Quaternion eyeForward = Quaternion.LookRotation(new Vector3(this.eye.forward.x, 0.0f, this.eye.forward.z));

			this.leftFoot .rotation = eyeForward;
			this.rightFoot.rotation = eyeForward;

			this.preRot = this.eye.rotation;
		}
	}
}

