using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ToyotaHSR
{
	public class HSRRigidbodyInitializer : MonoBehaviour
	{
		void Awake()
		{
			Rigidbody bodyRigidbody = GetComponent<Rigidbody>();

			bodyRigidbody.centerOfMass = new Vector3(0.0f, 0.0f, 0.15f);

			bodyRigidbody.maxDepenetrationVelocity = 3.0f;
		}
	}
}
