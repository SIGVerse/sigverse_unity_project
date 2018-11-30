using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;


namespace SIGVerse.ToyotaHSR
{
	public class HSRSubTwist : RosSubMessage<SIGVerse.RosBridge.geometry_msgs.Twist>
	{
//		private const float WheelInclinationThreshold = 0.985f; // 80[deg]
		private const float WheelInclinationThreshold = 0.965f; // 75[deg]
//		private const float WheelInclinationThreshold = 0.940f; // 70[deg]

		//--------------------------------------------------

		private Transform baseFootprint;
		private Transform baseFootprintPosNoise;
		private Transform baseFootprintRotNoise;
		private Transform baseFootprintRigidbody;

		private float linearVelX  = 0.0f;
		private float linearVelY  = 0.0f;
		private float angularVelZ = 0.0f;

		private bool isMoving = false;


		void Awake()
		{
			this.baseFootprint          = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintName);
			this.baseFootprintPosNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintPosNoiseName);
			this.baseFootprintRotNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRotNoiseName);
			this.baseFootprintRigidbody = SIGVerseUtils.FindTransformFromChild(this.transform.root, HSRCommon.BaseFootPrintRigidbodyName);
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.geometry_msgs.Twist twist)
		{
			float linearVel = Mathf.Sqrt(Mathf.Pow(twist.linear.x, 2) + Mathf.Pow(twist.linear.y, 2));

			float linearVelClamped = Mathf.Clamp(linearVel, 0.0f, HSRCommon.MaxSpeedBase);

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

			this.angularVelZ = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, HSRCommon.MaxSpeedBaseRad);

//			Debug.Log("linearVel=" + linearVel + ", angularVel=" + angularVel);
			this.isMoving = Mathf.Abs(this.linearVelX) >= 0.001f || Mathf.Abs(this.linearVelY) >= 0.001f || Mathf.Abs(this.angularVelZ) >= 0.001f;
		}


		void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootprint.forward.y) < WheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			Vector3 deltaPosition = (-this.baseFootprint.right *                  linearVelX  + this.baseFootprint.up *                  linearVelY ) * Time.fixedDeltaTime;
			Vector3 deltaNoisePos = (-this.baseFootprint.right * this.GetPosNoise(linearVelX) + this.baseFootprint.up * this.GetPosNoise(linearVelY)) * Time.fixedDeltaTime;

			this.baseFootprintRigidbody.position += deltaPosition;
			this.baseFootprintPosNoise .position += deltaNoisePos;


			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -                 angularVelZ  * Mathf.Rad2Deg * Time.fixedDeltaTime));
			Quaternion deltaNoiseRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, -this.GetRotNoise(angularVelZ) * Mathf.Rad2Deg * Time.fixedDeltaTime));

			this.baseFootprintRigidbody.rotation *= deltaRotation;
			this.baseFootprintRotNoise .rotation *= deltaNoiseRot;
		}

		private float GetPosNoise(float val)
		{
			float randomNumber = SIGVerseUtils.GetRandomNumberFollowingNormalDistribution(0.6f); // sigma=0.6

			return val * Mathf.Clamp(randomNumber, -1.8f, +1.8f); // 3 * sigma
		}

		private float GetRotNoise(float val)
		{
			float randomNumber = SIGVerseUtils.GetRandomNumberFollowingNormalDistribution(0.3f); // sigma=0.3

			return val * Mathf.Clamp(randomNumber, -0.9f, +0.9f); // 3 * sigma
		}
	}
}

