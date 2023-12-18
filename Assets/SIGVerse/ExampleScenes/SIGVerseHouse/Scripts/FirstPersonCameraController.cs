using UnityEngine;
using System.Collections;

namespace SIGVerse.ExampleScenes.SIGVerseHouse
{
	public class FirstPersonCameraController : MonoBehaviour
	{
		public Camera firstPersonCamera;
		float startGyroY;

		// Use this for initialization
		void Start()
		{
#if UNITY_ANDROID
		Input.gyro.enabled = true;
#else
			Input.gyro.enabled = false;
#endif
			this.startGyroY = Input.gyro.attitude.eulerAngles.y;
		}

		// Update is called once per frame
		void Update()
		{
			if (Input.gyro.enabled)
			{
				Quaternion gyro = Input.gyro.attitude;

				gyro = Quaternion.Euler(90, 0, 0) * (new Quaternion(-gyro.x, -gyro.y, gyro.z, gyro.w));

				this.firstPersonCamera.transform.localRotation = gyro;

				this.firstPersonCamera.transform.localEulerAngles += new Vector3(0.0f, -this.startGyroY, 0.0f);
			}
		}
	}
}

