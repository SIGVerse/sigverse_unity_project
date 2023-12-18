using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace SIGVerse.ExampleScenes.OvrHandTracking
{
	public class XRInitializer : MonoBehaviour
	{
		private XRLoader activeLoader;

		void Start()
		{
			StartCoroutine(this.InitializeXR());
		}

		public IEnumerator InitializeXR()
		{
			// Initialize XR System
			XRManagerSettings xrManagerSettings = XRGeneralSettings.Instance.Manager;

			if (xrManagerSettings == null) 
			{
				SIGVerseLogger.Error("XR initialization failed. xrManagerSettings == null"); 
				yield break; 
			}

			if(xrManagerSettings.activeLoader == null)
			{
				yield return xrManagerSettings.InitializeLoader();
			}

			this.activeLoader = xrManagerSettings.activeLoader;

			if (this.activeLoader == null)
			{
				Debug.LogError("XR initialization failed. activeLoader == null");
				yield break;
			}

			xrManagerSettings.activeLoader.Start(); 
		}

		void OnDestroy()
		{
			if(this.activeLoader != null)
			{
				this.activeLoader.Stop();
				XRGeneralSettings.Instance.Manager.DeinitializeLoader();
			}
		}
	}
}

