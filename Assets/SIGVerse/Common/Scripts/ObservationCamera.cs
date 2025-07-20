using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SIGVerse.Common
{
	public class ObservationCamera : MonoBehaviour
	{
		public float speed = 3.0f;
		public float rotSpeed = 90.0f;

		private void Update()
		{
			if (EventSystem.current.IsPointerOverGameObject()) { return; }
			
			float dtSpeed    = this.speed * Time.deltaTime;
			float dtRotSpeed = this.rotSpeed * Time.deltaTime;

			Keyboard key = Keyboard.current;
			Mouse mouse = Mouse.current;
			
			Transform trans = this.transform;

			if (key.shiftKey.isPressed)
			{
				if (key.upArrowKey   .isPressed || key.wKey.isPressed) { trans.position += trans.up    * dtSpeed; }
				if (key.downArrowKey .isPressed || key.sKey.isPressed) { trans.position -= trans.up    * dtSpeed; }
				if (key.rightArrowKey.isPressed || key.dKey.isPressed) { trans.position += trans.right * dtSpeed; }
				if (key.leftArrowKey .isPressed || key.aKey.isPressed) { trans.position -= trans.right * dtSpeed; }
			}
			else if (mouse.leftButton.isPressed || mouse.middleButton.isPressed)
			{
				float x = mouse.delta.x.ReadValue();
				float y = mouse.delta.y.ReadValue();

				if (y < 0) { trans.position -= trans.up    * dtSpeed * y/5; }
				if (y > 0) { trans.position -= trans.up    * dtSpeed * y/5; }
				if (x < 0) { trans.position -= trans.right * dtSpeed * x/5; }
				if (x > 0) { trans.position -= trans.right * dtSpeed * x/5; }
			}
			else if (key.ctrlKey.isPressed)
			{
				if (key.upArrowKey   .isPressed || key.wKey.isPressed) { trans.Rotate(new Vector3(-dtRotSpeed, 0, 0)); }
				if (key.downArrowKey .isPressed || key.sKey.isPressed) { trans.Rotate(new Vector3(+dtRotSpeed, 0, 0)); }
				if (key.rightArrowKey.isPressed || key.dKey.isPressed) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y+dtRotSpeed, 0); }
				if (key.leftArrowKey .isPressed || key.aKey.isPressed) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y-dtRotSpeed, 0); }
			}
			else if (mouse.rightButton.isPressed)
			{
				float x = mouse.delta.x.ReadValue();
				float y = mouse.delta.y.ReadValue();

				if(key.altKey.isPressed)
				{
					if (y < 0) { trans.position -= trans.forward * dtSpeed * y/5; }
					if (y > 0) { trans.position -= trans.forward * dtSpeed * y/5; }
				}
				else
				{
					if (y < 0) { trans.Rotate(new Vector3(-dtRotSpeed * y/5, 0, 0)); }
					if (y > 0) { trans.Rotate(new Vector3(-dtRotSpeed * y/5, 0, 0)); }
					if (x < 0) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y+dtRotSpeed * x/5, 0); }
					if (x > 0) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y+dtRotSpeed * x/5, 0); }
				}
			}
			else
			{
				if (key.upArrowKey   .isPressed || key.wKey.isPressed) { trans.position += trans.forward * dtSpeed; }
				if (key.downArrowKey .isPressed || key.sKey.isPressed) { trans.position -= trans.forward * dtSpeed; }
				if (key.rightArrowKey.isPressed || key.dKey.isPressed) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y+dtRotSpeed, 0); }
				if (key.leftArrowKey .isPressed || key.aKey.isPressed) { trans.localEulerAngles = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y-dtRotSpeed, 0); }

				if (mouse.scroll.y.ReadValue()>0) { trans.position += trans.forward * dtSpeed * 10f; }
				if (mouse.scroll.y.ReadValue()<0) { trans.position -= trans.forward * dtSpeed * 10f; }
			}
		}
	}
}

