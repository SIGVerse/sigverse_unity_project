using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;

namespace SIGVerse.SampleScenes.SampleHouse
{
	public class AudienceCameraBehavior : MonoBehaviour
	{
		public enum SubviewNumber
		{
			Subview1 = 1,
			Subview2,
			Subview3,
			Subview4,
		}
		

		public SubviewNumber subviewNumber;

		// Use this for initialization
		void Start ()
		{
			ExecuteEvents.Execute<ISubviewHandler>
			(
				target: GameObject.Find(SIGVerseMenu.SIGVerseMenuName),
				eventData: null,
				functor: (reciever, eventData) => reciever.OnUpdateSubviewCamera((int)this.subviewNumber, this.GetComponent<Camera>())
			);
		}
	
		// Update is called once per frame
		void Update ()
		{
		}
	}
}
