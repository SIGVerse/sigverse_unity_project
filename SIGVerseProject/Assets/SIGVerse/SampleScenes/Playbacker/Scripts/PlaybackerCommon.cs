using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.SampleScenes.Playbacker
{
	public enum ModeType
	{
		TextRecorder,
		TextPlayer,
		DatabaseRecorder,
		DatabasePlayer,
	}

	public class UpdatingTransformData
	{
		public Transform UpdatingTransform { get; set; }

		public Vector3  LocalPosition { get; set; }
		public Vector3  LocalRotation { get; set; }
		public Vector3? LocalScale    { get; set; }

		public void UpdateTransform()
		{
			this.UpdatingTransform.localPosition    = this.LocalPosition;
			this.UpdatingTransform.localEulerAngles = this.LocalRotation;

			if(this.LocalScale != null)
			{
				this.UpdatingTransform.localScale   = this.LocalScale.Value;
			}
		}
	}

	public class PlaybackerCommon : MonoBehaviour
	{
		public const string ButtonTextStart = "Start";
		public const string ButtonTextStop  = "Stop";

		public const int TypeDef = 0;
		public const int TypeVal = 1;

		public RectTransform mainPanelRectTransform;
		public Dropdown      changeModeDropdown;
		public Button        resetObjectsButton;

		public GameObject recorder;
		public GameObject player;

		public List<GameObject> targetObjects;

		//-----------------------------------------------------

		private List<UpdatingTransformData> initialTransforms = new List<UpdatingTransformData>();

		private List<Rigidbody> targetRigidbodies = new List<Rigidbody>();


		void Awake()
		{
			// Save initial transforms of targets
			foreach (GameObject targetObj in targetObjects)
			{
				Transform[] transforms = targetObj.transform.GetComponentsInChildren<Transform>(true);

				foreach (Transform transform in transforms)
				{
					UpdatingTransformData initialTransform = new UpdatingTransformData();
					initialTransform.UpdatingTransform = transform;

					initialTransform.LocalPosition = transform.localPosition;
					initialTransform.LocalRotation = transform.localEulerAngles;
					initialTransform.LocalScale    = transform.localScale;

					this.initialTransforms.Add(initialTransform);
				}

				Rigidbody[] rigidbodies = targetObj.transform.GetComponentsInChildren<Rigidbody>(true);

				foreach (Rigidbody rigidbody in rigidbodies)
				{
					this.targetRigidbodies.Add(rigidbody);
				}
			}

			this.ChangeModeDropdownValueChanged(this.changeModeDropdown);
		}

		// Use this for initialization
		void Start ()
		{
			this.recorder.SetActive(true);
			this.player  .SetActive(false);
		}


		public void ChangeModeDropdownValueChanged(Dropdown dropdown)
		{
			// Change the main panel layout
			switch((ModeType)dropdown.value)
			{
				case ModeType.TextRecorder:
				{
					this.recorder.SetActive(true);
					this.player  .SetActive(false);

					this.mainPanelRectTransform.sizeDelta = new Vector2(500, 155);
					break;
				}
				case ModeType.TextPlayer:
				{
					this.recorder.SetActive(false);
					this.player  .SetActive(true);

					this.mainPanelRectTransform.sizeDelta = new Vector2(500, 150);
					break;
				}
				case ModeType.DatabaseRecorder:
				{
					this.recorder.SetActive(true);
					this.player  .SetActive(false);

					this.mainPanelRectTransform.sizeDelta = new Vector2(500, 265);
					break;
				}
				case ModeType.DatabasePlayer:
				{
					this.recorder.SetActive(false);
					this.player  .SetActive(true);

					this.mainPanelRectTransform.sizeDelta = new Vector2(500, 225);
					break;
				}
			}
		}

		public void OnResetObjectsButtonClick()
		{
			this.RestoreInitialTransformsOfTargets();
		}

		private void RestoreInitialTransformsOfTargets()
		{
			foreach (UpdatingTransformData initialTransform in this.initialTransforms)
			{
				initialTransform.UpdateTransform();
			}

			foreach (Rigidbody rigidbody in this.targetRigidbodies)
			{
				rigidbody.velocity        = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}
		}
	}
}


