using UnityEngine;
using SIGVerse.Common;


namespace SIGVerse.TurtleBot3
{
	public class Turtlebot3TransformFixer : MonoBehaviour
	{
		private Vector3    posOrg;
		private Quaternion rotOrg;

		void Awake()
		{
			this.posOrg = this.transform.localPosition;
			this.rotOrg = this.transform.localRotation;
		}

		void LateUpdate()
		{
			this.transform.localPosition = this.posOrg;
			this.transform.localRotation = this.rotOrg;
		}
	}
}

