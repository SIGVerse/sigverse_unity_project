using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;
using System.Collections.Generic;

namespace SIGVerse.TurtleBot
{
	public class TurtleBot2SubTwist : RosSubMessage<SIGVerse.RosBridge.geometry_msgs.Twist>
	{
		public const float MaxSpeedBaseTrans = 0.7f;     // [m/s]
		public const float MaxSpeedBaseRot   = Mathf.PI; // [rad/s]

//		protected const float WheelInclinationThreshold = 0.985f; // 80[deg]
		protected const float WheelInclinationThreshold = 0.965f; // 75[deg]
//		protected const float WheelInclinationThreshold = 0.940f; // 70[deg]

		//--------------------------------------------------

		protected Transform baseFootprint;

		protected float linearVelX  = 0.0f;
		protected float angularVelZ = 0.0f;

		protected bool isMoving = false;


		protected virtual void Awake()
		{
			this.baseFootprint = SIGVerseUtils.FindTransformFromChild(this.transform.root, "base_footprint");
		}

		protected override void SubscribeMessageCallback(SIGVerse.RosBridge.geometry_msgs.Twist twist)
		{
			float linearVelAbs = Mathf.Abs(twist.linear.x);

			float linearVelAbsClamped = Mathf.Clamp(linearVelAbs, 0.0f, MaxSpeedBaseTrans);

			if(linearVelAbs >= 0.001)
			{
				this.linearVelX  = twist.linear.x * linearVelAbsClamped / linearVelAbs;
			}
			else
			{
				this.linearVelX = 0.0f;
			}

			this.angularVelZ = Mathf.Sign(twist.angular.z) * Mathf.Clamp(Mathf.Abs(twist.angular.z), 0.0f, MaxSpeedBaseRot);

//			Debug.Log("linearVel=" + this.linearVelX + ", angularVel=" + this.angularVelZ);

			this.isMoving = Mathf.Abs(this.linearVelX) >= 0.001f || Mathf.Abs(this.angularVelZ) >= 0.001f;
		}
		
		protected virtual void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootprint.forward.y) < WheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			this.baseFootprint.transform.position += (this.baseFootprint.transform.right * (-linearVelX)) * Time.fixedDeltaTime;
			this.baseFootprint.transform.rotation *= Quaternion.Euler(new Vector3(0.0f, 0.0f, -angularVelZ * Mathf.Rad2Deg * Time.fixedDeltaTime));
		}
	}
}

