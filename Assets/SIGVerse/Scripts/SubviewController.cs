using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Common
{
	public class SubviewController : MonoBehaviour
	{
		public const string SubViewControllerStr = "SubviewController";

		public static void EnableSubview(GameObject operationTarget)
		{
			operationTarget.transform.root.Find(SubViewControllerStr).gameObject.SetActive(true);

			// Update the camera list before enable SubviewOptionController
			GameObject.FindObjectOfType<SubviewManager>().UpdateCameraList();

			SubviewOptionController[] subviewOptionControllers = operationTarget.GetComponentsInChildren<SubviewOptionController>();

			foreach (SubviewOptionController subviewOptionController in subviewOptionControllers)
			{
				subviewOptionController.enabled = true;
			}
		}
	}
}
