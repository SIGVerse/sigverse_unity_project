using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace SIGVerse.Common.Recorder
{
	[RequireComponent(typeof (WorldPlaybackCommon))]
	public class WorldPlaybackRecorder : MonoBehaviour, IPlaybackStringHandler
	{
		public enum TargetType 
		{
			AllObjects,
			ObjectsWithRigidbodyAndItsChildObjects,
			ObjectsWithRigidbody,
		}

		private const string TimeFormat = "#####0[s]";

		[HeaderAttribute("Recording Targets")]
		public TargetType targetType = TargetType.ObjectsWithRigidbodyAndItsChildObjects;

		public List<GameObject> exclusionTargets;

		[TooltipAttribute("Regular Expression")]
		public List<String> exclusionTargetNameRegexs;


		[HeaderAttribute("Recording Parameters")]
		public string fileName = "Motions{_yyyyMMdd_HHmmss}.dat";

		[TooltipAttribute("[ms]")]
		public int recordingInterval = 20;

		[HeaderAttribute("GUI")]
		public Text mainPanelStatusText;


		//-----------------------------------------------------
		protected enum Step
		{
			Waiting,
			Initializing,
			Initialized, 
			Recording,
			Writing,
		}

		protected string filePath;

		protected Step step = Step.Waiting;

		protected float elapsedTime = 0.0f;
		protected float previousRecordedTime = 0.0f;

		protected List<Transform>   targetTransforms;
//		protected List<VideoPlayer> targetVideoPlayers;

		private List<string> dataLines;


		// Update is called once per frame
		protected virtual void Update()
		{
			if (this.step == Step.Recording)
			{
				this.elapsedTime += Time.deltaTime;

				if (1000.0 * (this.elapsedTime - this.previousRecordedTime) < this.recordingInterval) { return; }

				this.SaveData();

				this.previousRecordedTime = this.elapsedTime;
			}
		}

		public bool Initialize(string filePath)
		{
			if(this.step == Step.Waiting)
			{
				this.filePath = Path.GetFullPath(filePath);

				if (!Directory.Exists(Path.GetDirectoryName(this.filePath)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(this.filePath));
				}

				this.StartInitializing();
				return true;
			}

			return false;
		}

		public bool Initialize()
		{
			return this.Initialize(this.GetComponent<WorldPlaybackCommon>().GetFolderPath() + "/" + GetDateTimeStringName(this.fileName));
		}

		private static string GetDateTimeStringName(string path)
		{
			string[] splitByOpen = path.Split(new char[] { '{' }, 2);

			if (splitByOpen.Length < 2) { return path; }

			string[] splitByClose = splitByOpen[1].Split(new char[] { '}' }, 2);

			if (splitByClose.Length < 2) { return path; }

			if (splitByClose[0] == string.Empty) { return path; }

			return splitByOpen[0] + DateTime.Now.ToString(splitByClose[0]) + splitByClose[1];
		}

		public bool Record()
		{
			if (this.step == Step.Initialized)
			{
				this.StartRecording();
				return true;
			}

			return false;
		}

		public bool Stop()
		{
			if (this.step == Step.Recording)
			{
				this.StopRecording();
				return true;
			}

			return false;
		}

		protected virtual void StartInitializing()
		{
			this.step = Step.Initializing;

			SIGVerseLogger.Info("Output Playback file Path=" + this.filePath);

			// Create the recording target list
			this.InitializeTargetTransforms();

			// File open
			StreamWriter streamWriter = new StreamWriter(this.filePath, false);

			List<string> definitionLines = this.GetDefinitionLines();

			foreach(string definitionLine in definitionLines)
			{
				streamWriter.WriteLine(definitionLine);
			}

			streamWriter.Flush();
			streamWriter.Close();

			this.dataLines = new List<string>();

			this.step = Step.Initialized;
		}

		protected virtual void InitializeTargetTransforms()
		{
			// Transform
			this.targetTransforms = new List<Transform>();

			switch (this.targetType)
			{
				case TargetType.AllObjects:
				{
					this.targetTransforms = FindObjectsOfType<Transform>().ToList(); // High Load

					break;
				}
				case TargetType.ObjectsWithRigidbodyAndItsChildObjects:
				{
					HashSet<Transform> targetHashSet = new HashSet<Transform>();

					Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>(); // High Load

					foreach (Rigidbody rb in rigidbodies)
					{
						targetHashSet.UnionWith(rb.gameObject.GetComponentsInChildren<Transform>());
					}

					this.targetTransforms = targetHashSet.ToList();

					break;
				}
				case TargetType.ObjectsWithRigidbody:
				{
					Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>(); // High Load

					this.targetTransforms = rigidbodies.Select(x => x.transform).ToList();

					break;
				}
			}

			// Exclude some objects.
			foreach(GameObject rootObjectToExclude in this.exclusionTargets)
			{
				Transform[] transformsToExclude = rootObjectToExclude.GetComponentsInChildren<Transform>(true);

				foreach(Transform transformToExclude in transformsToExclude)
				{
					this.targetTransforms.Remove(transformToExclude);
				}
			}

			int targetTransformCountBefore = this.targetTransforms.Count;

			foreach(String exclusionTargetNameRegex in this.exclusionTargetNameRegexs)
			{
				this.targetTransforms = this.targetTransforms.Where(x => !Regex.IsMatch(x.name, exclusionTargetNameRegex)).ToList();
			}

			if(this.targetTransforms.Count != targetTransformCountBefore)
			{
				SIGVerseLogger.Info("The number excluded by name is "+(targetTransformCountBefore - this.targetTransforms.Count));
			}
		}

		protected virtual void AddDataLine(string dataLine)
		{
			if (this.step != Step.Recording)
			{
//				SIGVerseLogger.Warn("Illegal timing to add dataLine. data="+dataLine);
				return;
			}
			
			this.dataLines.Add(dataLine);
		}

		protected virtual List<string> GetDefinitionLines()
		{
			List<string> definitionLines = new List<string>();

			// Transform
			definitionLines.Add(PlaybackTransformEventController.GetDefinitionLine(this.targetTransforms.ToList()));

			//// Video Player
			//if(this.isReplayVideoPlayers)
			//{
			//	definitionLines.Add(PlaybackVideoPlayerEventController.GetDefinitionLine(this.targetVideoPlayers));
			//}

			return definitionLines;
		}


		protected virtual void StartRecording()
		{
			SIGVerseLogger.Info("( Start the world playback recording )");

			this.step = Step.Recording;

			// Reset elapsed time
			this.elapsedTime = 0.0f;
			this.previousRecordedTime = 0.0f;
		}

		protected virtual void StopRecording()
		{
			SIGVerseLogger.Info("( Stop the world playback recording )");

			this.step = Step.Writing;

			Thread threadWriteData = new Thread(new ThreadStart(this.WriteDataToFile));
			threadWriteData.Start();
		}
		
		protected virtual void WriteDataToFile()
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(this.filePath, true);

				foreach (string dataLine in this.dataLines)
				{
					streamWriter.WriteLine(dataLine);
				}

				streamWriter.Flush();
				streamWriter.Close();

				this.step = Step.Waiting;
			}
			catch(Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
				Application.Quit();
			}
		}

		protected virtual void SaveData()
		{
			this.SaveTransforms();
//			this.SaveVideoPlayers();
		}

		protected virtual string GetHeaderElapsedTime()
		{
			return Math.Round(this.elapsedTime, 4, MidpointRounding.AwayFromZero).ToString();
		}


		protected virtual void SaveTransforms()
		{
			this.AddDataLine(PlaybackTransformEventController.GetDataLine(this.GetHeaderElapsedTime(), this.targetTransforms));
		}

		//protected virtual void SaveVideoPlayers()
		//{
		//	if (!this.isReplayVideoPlayers) { return; }

		//	this.AddDataLine(PlaybackVideoPlayerEventController.GetDataLine(this.GetHeaderElapsedTime(), this.targetVideoPlayers));
		//}


		public bool IsInitialized()
		{
			return this.step == Step.Initialized;
		}

		public bool IsWaiting()
		{
			return this.step == Step.Waiting;
		}

		public bool IsRecording()
		{
			return this.step == Step.Recording;
		}

		protected virtual void OnGUI()
		{
			if (this.step == Step.Recording)
			{
				this.mainPanelStatusText.text = Math.Floor(this.elapsedTime).ToString(TimeFormat);
			}
		}

		public void OnReceiveString(string stringData)
		{
			this.AddDataLine(PlaybackStringEventController.GetDataLine(this.GetHeaderElapsedTime(), stringData));
		}
	}
}


