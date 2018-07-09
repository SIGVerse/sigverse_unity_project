using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3RigidbodyInitializer : MonoBehaviour
	{
		void Awake()
		{
			Rigidbody bodyRigidbody = GetComponent<Rigidbody>();

			bodyRigidbody.centerOfMass = new Vector3(0.03f, 0.0f, 0.03f);

			bodyRigidbody.maxDepenetrationVelocity = 3.0f;
		}
	}
}
