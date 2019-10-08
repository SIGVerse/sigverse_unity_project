using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.SampleScenes.Hsr
{
	public class BackgroundController : MonoBehaviour
	{
		public GameObject background;

		void Start()
		{
			this.background.SetActive(true);
		}
	}
}

