using UnityEngine;

namespace SIGVerse.TurtleBot
{
	public class TurtleBot2SubTwistForceSample : TurtleBot2SubTwist
	{
		public float coefForce  = 80.0f; // Unfounded parameter
		public float coefTorque =  1.6f; // Unfounded parameter

		//--------------------------------------------------

		private Rigidbody baseRigidbody;

		protected override void Awake()
		{
			base.Awake();

			this.baseRigidbody = base.baseFootprint.GetComponent<Rigidbody>();
		}

		protected override void FixedUpdate()
		{
			if (Mathf.Abs(this.baseFootprint.forward.y) < WheelInclinationThreshold) { return; }

			if (!this.isMoving) { return; }

			float coefAccelForce  = Mathf.Clamp(Mathf.Exp(0.12f/Mathf.Abs(linearVelX)),  1.0f, 3.0f); // Unfounded adjustment
			float coefAccelTorque = Mathf.Clamp(Mathf.Exp(0.75f/Mathf.Abs(angularVelZ)), 1.0f, 3.5f); // Unfounded adjustment


//			Debug.Log("val =" + this.baseRigidbody.transform.InverseTransformDirection(this.baseRigidbody.velocity).x + ", coef=" + coefAccelForce);

			this.baseRigidbody.AddForce (this.coefForce  * this.baseRigidbody.mass * (coefAccelForce  * base.baseFootprint.transform.right   * (-linearVelX)  - this.baseRigidbody.velocity));
			this.baseRigidbody.AddTorque(this.coefTorque * this.baseRigidbody.mass * (coefAccelTorque * base.baseFootprint.transform.forward * (-angularVelZ) - this.baseRigidbody.angularVelocity));
		}
	}
}

