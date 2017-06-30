using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace SIGVerse.SampleScenes.Playbacker
{
	public class PlaybackerRecorder : MonoBehaviour
	{
		public List<GameObject> targetObjects;
	
		[Header("Recorder Properties")]
		public Button recordButton;
		public InputField outputFilePath;
		public InputField recordInterval;

		[Header("Player Properties")]
		public Button     playButton;


		//-----------------------------------------------------
		private Text recordButtonText;

		private bool isRecording = false;

		private StreamWriter motionsDataWriter;

		private float elapsedTime = 0.0f;
		private float previousRecordedTime = 0.0f;

		private List<Transform> targetTransformInstances;
		private List<string> savedMotionStrings;


		// Use this for initialization
		void Start ()
		{
			this.recordButtonText = this.recordButton.GetComponentInChildren<Text>();
		}


		// Update is called once per frame
		void Update ()
		{
			this.elapsedTime += Time.deltaTime;

			if (this.isRecording)
			{
				this.SaveMotions();
			}
		}

		void SaveMotions()
		{
			if (1000.0*(this.elapsedTime-this.previousRecordedTime) < float.Parse(recordInterval.text)) { return; }

			string lineStr = string.Empty;

			lineStr += Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero) + "," + PlaybackerCommon.TypeVal;

			foreach(Transform transform in this.targetTransformInstances)
			{
				lineStr += "\t" +
					Math.Round(transform.localPosition.x,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localPosition.y,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localPosition.z,    4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.x, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.y, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.z, 4, MidpointRounding.AwayFromZero);
	//				Math.Round(transform.localScale.x,       4, MidpointRounding.AwayFromZero) + "," +
	//				Math.Round(transform.localScale.y,       4, MidpointRounding.AwayFromZero) + "," +
	//				Math.Round(transform.localScale.z,       4, MidpointRounding.AwayFromZero);
			}

			this.savedMotionStrings.Add(lineStr);

			this.previousRecordedTime = this.elapsedTime;
		}


		public void Record()
		{
			if(!this.isRecording)
			{
				this.StartRecording();
			}
			else
			{
				this.StopRecording();
			}
		}

		private void StartRecording()
		{
			DateTime dateTime = DateTime.Now;

			Debug.Log("outputFilePath=" + this.outputFilePath.text);

			// File open
			this.motionsDataWriter = new StreamWriter(this.outputFilePath.text, false, Encoding.UTF8);

			this.targetTransformInstances = new List<Transform>();

			// Write header line and get transform instances
			string definitionLine = string.Empty;

			definitionLine += "0.0," + PlaybackerCommon.TypeDef; // Elapsed time is dummy.

			foreach (GameObject targetObj in targetObjects)
			{
				foreach (Transform transform in targetObj.transform.GetComponentsInChildren<Transform>())
				{
					// Save Transform instance list
					this.targetTransformInstances.Add(transform);

					// Make a header line
					definitionLine += "\t" + GetLinkPath(transform);
				}
			}

			this.motionsDataWriter.WriteLine(definitionLine);

			this.savedMotionStrings = new List<string>();

			// Change Buttons
			this.recordButtonText.text = PlaybackerCommon.ButtonTextStop;
			this.playButton.interactable = false;

			// Reset elapsed time
			this.elapsedTime = 0.0f;
			this.previousRecordedTime = 0.0f;

			this.isRecording = true;
		}

		private void StopRecording()
		{
			this.isRecording = false;

			foreach (string savedMotionString in savedMotionStrings)
			{
				this.motionsDataWriter.WriteLine(savedMotionString);
			}

			this.motionsDataWriter.Flush();
			this.motionsDataWriter.Close();

			// Change Buttons
			this.recordButtonText.text = PlaybackerCommon.ButtonTextStart;
			this.playButton.interactable = true;
		}



		private static string GetLinkPath(Transform transform)
		{
			string path = transform.name;

			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			return path;
		}
	}
}

