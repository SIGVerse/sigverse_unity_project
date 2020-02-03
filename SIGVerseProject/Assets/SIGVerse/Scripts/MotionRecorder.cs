using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SIGVerse.Common
{
	public class MotionRecorder : MonoBehaviour, IDragHandler
	{
		private const string ButtonTextStart = "Start";
		private const string ButtonTextStop = "Stop";

		private const string DataType1Motion = "0";

		private const string DataType2MotionDefinition = "0";
		private const string DataType2MotionValue = "1";

		public List<GameObject> targetObjects;

		[Header("Recorder Properties")]
		public Button recordButton;
		public Text outputFolderPath;

		//	[TooltipAttribute("ms")]
		public Text recordInterval;

		[Header("Player Properties")]
		public Button playButton;
		public Text inputFilePath;


		//-----------------------------------------------------
		private Text recordButtonText;
		private Text playButtonText;

		private bool isRecording = false;
		private bool isPlaying = false;

		private string outputFilePath;
		private StreamWriter swMotionsDataWriter;

		private float elapsedTime = 0.0f;
		private float previousRecordedTime = 0.0f;

		private List<Transform> targetTransformInstances;
		private List<string> savedMotionStrings;


		public class GameObjUpdateData
		{
			private Transform updatingTargetTransform;
			private Vector3 position;
			private Vector3 rotation;
			private Vector3 scale;

			public void SetUpdatingTargetTransform(Transform transform)
			{
				this.updatingTargetTransform = transform;
			}

			public void SetPostion(Vector3 position)
			{
				this.position = position;
			}
			public void SetRotation(Vector3 rotation)
			{
				this.rotation = rotation;
			}
			public void SetScale(Vector3 scale)
			{
				this.scale = scale;
			}

			public void UpdateTransform()
			{
				this.updatingTargetTransform.localPosition = this.position;
				this.updatingTargetTransform.localEulerAngles = this.rotation;
				this.updatingTargetTransform.localScale = this.scale;
			}
		}

		public class TimeSeriesMotionsData
		{
			public float ElapsedTime { get; set; }
			private List<GameObjUpdateData> gameObjUpdateDataList;

			public TimeSeriesMotionsData()
			{
				this.gameObjUpdateDataList = new List<GameObjUpdateData>();
			}

			public void AddGameObjUpdateData(GameObjUpdateData gameObjUpdateData)
			{
				this.gameObjUpdateDataList.Add(gameObjUpdateData);
			}

			public List<GameObjUpdateData> GetGameObjUpdateDataList()
			{
				return this.gameObjUpdateDataList;
			}
		}

		private Queue<TimeSeriesMotionsData> timeSeriesMotionsQue;



		// Use this for initialization
		void Start()
		{
			this.recordButtonText = this.recordButton.GetComponentInChildren<Text>();
			this.playButtonText = this.playButton.GetComponentInChildren<Text>();
		}


		// Update is called once per frame
		void Update()
		{
			this.elapsedTime += Time.deltaTime;

			if (this.isRecording)
			{
				this.SaveMotions();
			}
			else if (this.isPlaying)
			{
				this.PlayMotions();
			}
		}

		void SaveMotions()
		{
			if (1000.0 * (this.elapsedTime - this.previousRecordedTime) < float.Parse(recordInterval.text)) { return; }

			string lineStr = string.Empty;

			lineStr += Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero) + "," + DataType1Motion + "," + DataType2MotionValue;

			foreach (Transform transform in this.targetTransformInstances)
			{
				lineStr += "\t" +
					Math.Round(transform.localPosition.x, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localPosition.y, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localPosition.z, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.x, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.y, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localEulerAngles.z, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.x, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.y, 4, MidpointRounding.AwayFromZero) + "," +
					Math.Round(transform.localScale.z, 4, MidpointRounding.AwayFromZero);
			}

			//foreach (GameObject targetObj in targetObjects)
			//{
			//	foreach (Transform transform in targetObj.transform.GetComponentsInChildren<Transform>())
			//	{
			//		lineStr += "\t" + 
			//			Math.Round(transform.localPosition.x,    5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localPosition.y,    5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localPosition.z,    5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localEulerAngles.x, 5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localEulerAngles.y, 5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localEulerAngles.z, 5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localScale.x,       5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localScale.y,       5, MidpointRounding.AwayFromZero) + "," +
			//			Math.Round(transform.localScale.z,       5, MidpointRounding.AwayFromZero);
			//	}
			//}

			this.savedMotionStrings.Add(lineStr);

			this.previousRecordedTime = this.elapsedTime;
		}

		void PlayMotions()
		{
			if (this.timeSeriesMotionsQue.Count == 0)
			{
				this.StopPlaying();
				return;
			}

			TimeSeriesMotionsData timeSeridsMotionsData = null;

			while (this.elapsedTime >= this.timeSeriesMotionsQue.Peek().ElapsedTime)
			{
				timeSeridsMotionsData = timeSeriesMotionsQue.Dequeue();

				if (this.timeSeriesMotionsQue.Count == 0) { break; }
			}

			if (timeSeridsMotionsData == null) { return; }

			List<GameObjUpdateData> transforms = timeSeridsMotionsData.GetGameObjUpdateDataList();

			foreach (GameObjUpdateData transform in transforms)
			{
				transform.UpdateTransform();
			}
		}


		public void Record()
		{
			if (!this.isRecording)
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
//			DateTime dateTime = DateTime.Now;

			//this.outputFilePath = outputFolderPath.TrimEnd(' ', '\\') + "\\Motions_" + dateTime.ToString("yyyyMMddHHmmsss") + ".dat";
			this.outputFilePath = outputFolderPath.text.TrimEnd(' ', '\\') + "\\Motions.dat";
			SIGVerseLogger.Info("StartRecording outputFilePath=" + this.outputFilePath);

			// File open
			this.swMotionsDataWriter = new StreamWriter(this.outputFilePath, false);

			this.targetTransformInstances = new List<Transform>();

			// Write header line and get transform instances
			string definitionLine = string.Empty;

			definitionLine += "0.0," + DataType1Motion + "," + DataType2MotionDefinition; // Elapsed time is dummy.

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

			this.swMotionsDataWriter.WriteLine(definitionLine);

			this.savedMotionStrings = new List<string>();

			// Change Buttons
			this.recordButtonText.text = ButtonTextStop;
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
				this.swMotionsDataWriter.WriteLine(savedMotionString);
			}

			this.swMotionsDataWriter.Flush();
			this.swMotionsDataWriter.Close();

			// Change Buttons
			this.recordButtonText.text = ButtonTextStart;
			this.playButton.interactable = true;
		}


		public void Play()
		{
			if (!this.isPlaying)
			{
				this.StartPlaying();
			}
			else
			{
				this.StopPlaying();
			}
		}

		private void StartPlaying()
		{
			if (!File.Exists(inputFilePath.text))
			{
				SIGVerseLogger.Info("Input File NOT found.");
				return;
			}

			// File open
			StreamReader srMotionsDataReader = new StreamReader(inputFilePath.text);

			this.timeSeriesMotionsQue = new Queue<TimeSeriesMotionsData>();

			List<Transform> transformOrder = new List<Transform>();

			while (srMotionsDataReader.Peek() >= 0)
			{
				string lineStr = srMotionsDataReader.ReadLine();

				string[] columnArray = lineStr.Split(new char[] { '\t' }, 2);

				if (columnArray.Length < 2) { continue; }

				string headerStr = columnArray[0];
				string dataStr = columnArray[1];

				string[] headerArray = headerStr.Split(',');

				// Motion data
				if (headerArray[1] == DataType1Motion)
				{
					string[] dataArray = dataStr.Split('\t');

					// Definition
					if (headerArray[2] == DataType2MotionDefinition)
					{
						transformOrder.Clear();

						Debug.Log("data num=" + dataArray.Length);

						foreach (string transformPath in dataArray)
						{
							transformOrder.Add(GameObject.Find(transformPath).transform);
						}
					}
					// Value
					else if (headerArray[2] == DataType2MotionValue)
					{
						if (transformOrder.Count == 0) { continue; }

						TimeSeriesMotionsData timeSeriesMotionsData = new TimeSeriesMotionsData();

						timeSeriesMotionsData.ElapsedTime = float.Parse(headerArray[0]);

						for (int i = 0; i < dataArray.Length; i++)
						{
							string[] transformValues = dataArray[i].Split(',');

							GameObjUpdateData transform = new GameObjUpdateData();
							transform.SetUpdatingTargetTransform(transformOrder[i]);

							transform.SetPostion(new Vector3(float.Parse(transformValues[0]), float.Parse(transformValues[1]), float.Parse(transformValues[2])));
							transform.SetRotation(new Vector3(float.Parse(transformValues[3]), float.Parse(transformValues[4]), float.Parse(transformValues[5])));

							if (transformValues.Length == 6)
							{
								transform.SetScale(Vector3.one);
							}
							else if (transformValues.Length == 9)
							{
								transform.SetScale(new Vector3(float.Parse(transformValues[6]), float.Parse(transformValues[7]), float.Parse(transformValues[8])));
							}

							timeSeriesMotionsData.AddGameObjUpdateData(transform);
						}

						this.timeSeriesMotionsQue.Enqueue(timeSeriesMotionsData);
					}
				}
			}

			srMotionsDataReader.Close();

			// Change Buttons
			this.playButtonText.text = ButtonTextStop;
			this.recordButton.interactable = false;

			// Disable Animator
			foreach (GameObject targetObj in targetObjects)
			{
				// Disable only one animator component
				targetObj.transform.GetComponent<Animator>().enabled = false;
			}

			// Reset elapsed time
			this.elapsedTime = 0.0f;
			this.previousRecordedTime = 0.0f;

			this.isPlaying = true;
		}

		private void StopPlaying()
		{
			this.isPlaying = false;

			// Change Buttons
			this.playButtonText.text = ButtonTextStart;
			this.recordButton.interactable = true;

			// Enable Animator
			foreach (GameObject targetObj in targetObjects)
			{
				// Enable only one animator component
				targetObj.transform.GetComponent<Animator>().enabled = true;
			}
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

		public void OnDrag(PointerEventData eventData)
		{
			this.transform.position += (Vector3)eventData.delta;
		}
	}
}