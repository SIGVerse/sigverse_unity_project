using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System;
using static SIGVerse.PR2.PR2Common;

namespace SIGVerse.PR2
{
	public class PR2SubTwist : RosSubMessage<SIGVerse.RosBridge.geometry_msgs.Twist>
	{
//		private const float WheelInclinationThreshold = 0.985f; // 80[deg]
		private const float WheelInclinationThreshold = 0.965f; // 75[deg]
//		private const float WheelInclinationThreshold = 0.940f; // 70[deg]

		//--------------------------------------------------

		private Transform baseFootprint;

		private float linearVelX  = 0.0f;
		private float linearVelY  = 0.0f;
		private float angularVelZ = 0.0f;

		private bool isMoving = false;


		void Awake()
		{
			this.baseFootprint = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.base_footprint.ToString());
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.geometry_msgs.Twist twist)
		{
			float linearVel = Mathf.Sqrt(Mathf.Pow(twist.linear.x, 2) + Mathf.Pow(twist.linear.y, 2));

			float linearVelClamped = Mathf.Clamp(linearVel, 0.0f, PR2Common.MaxSpeedBase);

			if(linearVel >= 0.001)
			{
				this.linearVelX  = twist.linear.x * linearVelClamped / linearVel;
				this.linearVelY  = twist.linear.y * linearVelClamped / linearVel;
			}
			else
			{
				this.linearVelX = 0.0f;
				this.linearVelY = 0.0f;
			}

			this.angularVelZ = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, PR2Common.MaxSpeedBaseRad);

//			Debug.Log("linearVel=" + linearVel + ", angularVel=" + angularVel);
			this.isMoving = Mathf.Abs(this.linearVelX) >= 0.001f || Mathf.Abs(this.linearVelY) >= 0.001f || Mathf.Abs(this.angularVelZ) >= 0.001f;
		}


		void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootprint.forward.y) < WheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			Vector3 deltaPosition = (-this.baseFootprint.right * linearVelX + this.baseFootprint.up * linearVelY ) * Time.fixedDeltaTime;

			this.baseFootprint.position += deltaPosition;


			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -angularVelZ  * Mathf.Rad2Deg * Time.fixedDeltaTime));

			this.baseFootprint.rotation *= deltaRotation;
		}
	}
}

