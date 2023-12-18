using UnityEngine;

namespace SIGVerse.Common
{
	public class BirdsEyeViewCameraController : MonoBehaviour
	{
		public GameObject robot;

		private float cameraPosY;

		private void Start()
		{
			this.cameraPosY = this.transform.position.y;
		}

		private void LateUpdate()
		{
			this.transform.position = new Vector3(this.robot.transform.position.x, this.cameraPosY, this.robot.transform.position.z);
		}
	}
}
