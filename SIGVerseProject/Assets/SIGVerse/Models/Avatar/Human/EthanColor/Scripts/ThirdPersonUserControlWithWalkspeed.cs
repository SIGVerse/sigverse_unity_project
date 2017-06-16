using System;
using UnityEngine;
using UnityStandardAssets_1_1_2.CrossPlatformInput;
using UnityStandardAssets_1_1_2.Characters.ThirdPerson;

namespace SIGVerse.EthanColor
{
	[RequireComponent(typeof (ThirdPersonCharacter))]
	public class ThirdPersonUserControlWithWalkspeed : MonoBehaviour
	{
		private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
		private Transform m_Cam;                  // A reference to the main camera in the scenes transform
		private Vector3 m_CamForward;             // The current forward direction of the camera
		private Vector3 m_Move;
		private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

		public float walk_speed;
		public bool useMainCamera = true;

#if UNITY_ANDROID
		private Vector3 startPos;
		private Vector3 currentPos;
		private bool    isMoving;
#endif
		private void Start()
		{
			// get the transform of the main camera
			if(this.useMainCamera)
			{
				if (Camera.main != null)
				{
					m_Cam = Camera.main.transform;
				}
				else
				{
					Debug.LogWarning(
						"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
					// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
				}
			}
			else
			{
				m_Cam = this.transform;
			}

			// get the third person character ( this should never be null due to require component )
			m_Character = GetComponent<ThirdPersonCharacter>();

			m_Jump = false;

#if UNITY_ANDROID
			this.startPos = new Vector3(0.0f, 0.0f, 0.0f);
			this.isMoving = false;
#endif
		}


		private void Update()
		{
			// read inputs
#if !UNITY_ANDROID
			float h = CrossPlatformInputManager.GetAxis("Horizontal") * walk_speed;
			float v = CrossPlatformInputManager.GetAxis("Vertical") * walk_speed;

			//h=+0.5 right, h=-0.5 left, v=0.5 forward
#else
			if (Input.GetMouseButtonDown(0))
			{
				this.isMoving = true;
				this.startPos = Input.mousePosition;
//				Debug.Log("mouse Down" + this.startPos);
			}

			if (Input.GetMouseButtonUp(0))
			{
				this.isMoving = false;
//				var mousePos = Input.mousePosition;
//				Debug.Log("mouse up" + mousePos);
			}

			float h = 0.0f;
			float v = 0.0f;

			if (this.isMoving && Input.GetMouseButton(0))
			{
				this.currentPos = Input.mousePosition - this.startPos;
//				Debug.Log("mouse pos:" + currentPos);

				h = -currentPos.x * walk_speed;
				v = +currentPos.y * walk_speed;

				if (Math.Abs(h) > Math.Abs(v)) { v = 0.0f; }
				if (Math.Abs(h) < Math.Abs(v)) { h = 0.0f; }

				v *= 0.01f;
			}

#endif

			bool crouch = Input.GetKey(KeyCode.C);

			// calculate move direction to pass to character
			if (m_Cam != null)
			{
				// calculate camera relative direction to move:
				m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
				m_Move = v * m_CamForward + h * m_Cam.right;
			}
			else
			{
				// we use world-relative directions in the case of no main camera
				m_Move = v * Vector3.forward + h * Vector3.right;
			}
#if !MOBILE_INPUT
			// walk speed multiplier
			if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

			// pass all parameters to the character control script
			m_Character.Move(m_Move, crouch, m_Jump);
			m_Jump = false;
		}
	}
}
