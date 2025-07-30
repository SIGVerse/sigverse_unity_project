using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.Common.Recorder
{
	public class MotionRecorderPanelController : MonoBehaviour
	{
		public GameObject mainPanel;
		public GameObject confirmationPanel;
		public GameObject playbackPanel;

		public Button   recordButton;
		public Button   goToPlaybackModeButton;
		public TMP_Text mainPanelStatusText;

		public Image recordImage;

		public Sprite recordIcon;
		public Sprite stopIcon;

		public WorldPlaybackRecorder worldPlaybackRecorder;
		public WorldPlaybackPlayer   worldPlaybackPlayer;

		// ---------------------------------------

		void Start()
		{
			this.confirmationPanel.SetActive(false);
			this.playbackPanel    .SetActive(false);
			this.mainPanelStatusText.enabled = false;
		}

		public void OnRecordButtonClick()
		{
			this.mainPanelStatusText.enabled = true;

			try
			{
				if (this.worldPlaybackRecorder.IsWaiting())
				{
					if (!this.worldPlaybackRecorder.Initialize()){ throw new Exception("Start Recording(initialize) Error"); }
					if (!this.worldPlaybackRecorder.Record())    { throw new Exception("Start Recording Error"); }

					this.recordImage.sprite = this.stopIcon;
					this.goToPlaybackModeButton.interactable = false;

					return;
				}

				if (this.worldPlaybackRecorder.IsRecording())
				{
					if (!this.worldPlaybackRecorder.Stop()){ throw new Exception("Stop Recording Error"); }

					this.recordImage.sprite = this.recordIcon;
					this.goToPlaybackModeButton.interactable = true;

					return;
				}
			}
			catch
			{
				this.mainPanelStatusText.text = "Error!!";
				return;
			}
		}


		public void OnGoToPlaybackModeButtonClick()
		{
			this.mainPanelStatusText.enabled = false;

			this.confirmationPanel.SetActive(true);

			this.recordButton          .interactable = false;
			this.goToPlaybackModeButton.interactable = false;
		}


		public void OnConfirmYesButtonClick()
		{
			this.mainPanel        .SetActive(false);
			this.confirmationPanel.SetActive(false);
			this.playbackPanel    .SetActive(true);

			this.worldPlaybackRecorder.enabled = false;

			this.StopXR();
		}

		private void StopXR()
		{
			var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;

			if (xrManagerSettings == null) { return; }

			if(xrManagerSettings.activeLoader != null)
			{
				xrManagerSettings.activeLoader.Stop();
				UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
			}
		}

		public void OnConfirmNoButtonClick()
		{
			this.confirmationPanel.SetActive(false);

			this.recordButton          .interactable = true;
			this.goToPlaybackModeButton.interactable = true;
		}
		public void OnReadFileButtonClick()
		{
			string folderPath = this.worldPlaybackPlayer.GetComponent<WorldPlaybackCommon>().GetFolderPath();

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			string path = string.Empty;

#if UNITY_EDITOR
			path = UnityEditor.EditorUtility.OpenFilePanel("Select a motion file", folderPath, "dat");

#elif UNITY_STANDALONE_WIN
			System.Windows.Forms.OpenFileDialog open_file_dialog = new System.Windows.Forms.OpenFileDialog();
			open_file_dialog.ShowDialog();

			path = open_file_dialog.FileName;
#endif

			if (path == string.Empty) { return; }

			this.worldPlaybackPlayer.Initialize(path);
		}
	}
}

