using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.Common
{
	public class SIGVerseWarningWindow : MonoBehaviour
	{
		public Text message;

		// Use this for initialization
		void Start ()
		{	
		}
	
		// Update is called once per frame
		void Update ()
		{	
		}

		public void OnClickOKButton()
		{
			Destroy(this.gameObject);
		}
	}
}
