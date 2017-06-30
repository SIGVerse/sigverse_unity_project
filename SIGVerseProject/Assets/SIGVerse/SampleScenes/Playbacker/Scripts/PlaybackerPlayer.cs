using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace SIGVerse.SampleScenes.Playbacker
{
	public class PlaybackerPlayer : MonoBehaviour
	{
		public List<GameObject> targetObjects;

		[Header("Player Properties")]
		public Button     playButton;
		public InputField inputFilePath;
//		public SensoManager sensoManager;

		[Header("Recorder Properties")]
		public Button recordButton;


		//-----------------------------------------------------
		private Text playButtonText;

		private bool isPlaying = false;

		private float elapsedTime = 0.0f;

		private class UpdatingTransformData
		{
			public Transform UpdatingTransform { get; set; }
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }
	//		public Vector3 Scale    { get; set; }

			public void UpdateTransform()
			{
				this.UpdatingTransform.localPosition    = this.Position;
				this.UpdatingTransform.localEulerAngles = this.Rotation;
	//			this.UpdatingTransform.localScale  = this.Scale;
			}
		}

		private class UpdatingTransformList
		{
			public float ElapsedTime { get; set; }
			private List<UpdatingTransformData> updatingTransformList;

			public UpdatingTransformList()
			{
				this.updatingTransformList = new List<UpdatingTransformData>();
			}

			public void AddUpdatingTransform(UpdatingTransformData updatingTransformData)
			{
				this.updatingTransformList.Add(updatingTransformData);
			}

			public List<UpdatingTransformData> GetUpdatingTransformList()
			{
				return this.updatingTransformList;
			}
		}

		private Queue<UpdatingTransformList> playingTransformQue;



		// Use this for initialization
		void Start ()
		{
			this.playButtonText   = this.playButton.GetComponentInChildren<Text>();
		}


		// Update is called once per frame
		void Update ()
		{
			this.elapsedTime += Time.deltaTime;

			if(this.isPlaying)
			{
				this.PlayMotions();
			}
		}


		void PlayMotions()
		{
			if (this.playingTransformQue.Count == 0)
			{
				this.StopPlaying();
				return;
			}

			UpdatingTransformList updatingTransformList = null;

			while (this.elapsedTime >= this.playingTransformQue.Peek().ElapsedTime)
			{
				updatingTransformList = this.playingTransformQue.Dequeue();

				if (this.playingTransformQue.Count == 0) { break; }
			}

			if (updatingTransformList == null) { return; }

			foreach (UpdatingTransformData updatingTransformData in updatingTransformList.GetUpdatingTransformList())
			{
				updatingTransformData.UpdateTransform();
			}
		}



		public void Play()
		{
			if(!this.isPlaying)
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
				Debug.Log("Input File NOT found.");
				return;
			}

			// File open
			StreamReader motionsDataReader = new StreamReader(inputFilePath.text, Encoding.UTF8);

			this.playingTransformQue = new Queue<UpdatingTransformList>();

			List<Transform> transformOrder = new List<Transform>();

			while (motionsDataReader.Peek() >= 0)
			{
				string lineStr = motionsDataReader.ReadLine();

				string[] columnArray = lineStr.Split(new char[] { '\t' }, 2);

				if (columnArray.Length < 2) { continue; }

				string headerStr = columnArray[0];
				string dataStr   = columnArray[1];

				string[] headerArray = headerStr.Split(',');
				string[] dataArray   = dataStr.Split('\t');

				// Definition
				if (headerArray[1] == PlaybackerCommon.TypeDef)
				{
					transformOrder.Clear();

	//				Debug.Log("data num=" + dataArray.Length);

					foreach (string transformPath in dataArray)
					{
						transformOrder.Add(GameObject.Find(transformPath).transform);
					}
				}
				// Value
				else if (headerArray[1] == PlaybackerCommon.TypeVal)
				{
					if (transformOrder.Count == 0) { continue; }

					UpdatingTransformList timeSeriesMotionsData = new UpdatingTransformList();

					timeSeriesMotionsData.ElapsedTime = float.Parse(headerArray[0]);

					for (int i = 0; i < dataArray.Length; i++)
					{
						string[] transformValues = dataArray[i].Split(',');

						UpdatingTransformData transformPlayer = new UpdatingTransformData();
						transformPlayer.UpdatingTransform = transformOrder[i];

						transformPlayer.Position = new Vector3(float.Parse(transformValues[0]), float.Parse(transformValues[1]), float.Parse(transformValues[2]));
						transformPlayer.Rotation = new Vector3(float.Parse(transformValues[3]), float.Parse(transformValues[4]), float.Parse(transformValues[5]));

						//if (transformValues.Length == 6)
						//{
						//	transformPlayer.Scale = Vector3.one;
						//}

						timeSeriesMotionsData.AddUpdatingTransform(transformPlayer);
					}

					this.playingTransformQue.Enqueue(timeSeriesMotionsData);
				}
			}

			motionsDataReader.Close();

			// Change Buttons
			this.playButtonText.text = PlaybackerCommon.ButtonTextStop;
			this.recordButton.interactable = false;

//			this.sensoManager.enabled = false;

			// Disable Animator
			foreach (GameObject targetObj in targetObjects)
			{
				// Disable only one animator component
				targetObj.transform.GetComponent<Animator>().enabled = false;
			}

			Debug.Log("Playback player : File reading finished.");

			// Reset elapsed time
			this.elapsedTime = 0.0f;

			this.isPlaying = true;
		}

		private void StopPlaying()
		{
			this.isPlaying = false;

			// Change Buttons
			this.playButtonText.text = PlaybackerCommon.ButtonTextStart;
			this.recordButton.interactable = true;

//			this.sensoManager.enabled = true;

			// Enable Animator
			foreach (GameObject targetObj in targetObjects)
			{
				// Enable only one animator component
				targetObj.transform.GetComponent<Animator>().enabled = true;
			}

			Debug.Log("Playback player : Playing finished.");
		}
	}
}

