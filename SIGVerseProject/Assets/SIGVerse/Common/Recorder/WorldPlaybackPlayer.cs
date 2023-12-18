using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.Common.Recorder
{
	[RequireComponent(typeof(WorldPlaybackCommon))]
	public class WorldPlaybackPlayer : MonoBehaviour
	{
		public enum Step
		{
			Waiting,
			Initializing,
			Playing,
		}

		private const string ElapsedTimeFormat = "###0.0";
		private const string TotalTimeFormat = "###0";

		public List<GameObject> avatarSources;

		[TooltipAttribute("GameObjects from the scene")]
		public List<GameObject> targetsToDisableOnPlaybackMode;

		[TooltipAttribute("Scripts from Assets")]
#if UNITY_EDITOR
		public List<MonoScript> scriptsToDisableOnPlaybackMode;
#endif
		[HideInInspector]
		public List<string> scriptsAssemblyQualifiedName;

		[HeaderAttribute("Playback Parameters")]
		[TooltipAttribute("[s]")]
		public float repeatInterval = 5.0f;

		public GameObject[] stringDataDestinations;

		public bool enablePlayerCamera = false;
		public bool useObservationCamera = true;

		public GameObject observationCamera;

		[HeaderAttribute("GUI")]
		public GameObject playbackPanel;

		public Sprite playSprite;
		public Sprite pauseSprite;

		public MonoBehaviour behav;


		//--------------------------------------------------------------------------------------------

		protected Step step = Step.Waiting;
		protected Step preStep = Step.Waiting;

		protected bool isInitialized = false;
		protected bool canInstantiateRootObjects = false;
		protected HashSet<string> rootObjectNames;


		protected string errorMsg = string.Empty;

		protected float elapsedTime = 0.0f;
		protected float deltaTime = 0.0f;

		protected float startTime = 0.0f;
		protected float endTime = 0.0f;

		protected float playingSpeed = 1.0f;
		protected bool isRepeating = false;

		protected PlaybackTransformEventController transformController;  // Transform
		protected PlaybackStringEventController stringController;     // String Data

		protected string filePath;

		protected int totalTimeInt = 0;

		protected bool isStepChanged = true;
		protected bool isFileRead = false;


		//--- GUI ---
		protected Text statusText;
		protected Button readFileButton;
		protected Text fileNameText;
		protected Text elapsedTimeText;
		protected Text totalTimeText;
		protected Slider timeSlider;
		protected Button playButton;
		protected Dropdown speedDropdown;
		protected Toggle repeatToggle;
		protected InputField startTimeInputField;
		protected InputField endTimeInputField;


		protected virtual void Awake()
		{
			this.statusText          = this.playbackPanel.transform.Find("StatusText").GetComponent<Text>();
			this.readFileButton      = this.playbackPanel.transform.Find("ReadFile/ReadFileButton").GetComponent<Button>();
			this.fileNameText        = this.playbackPanel.transform.Find("ReadFile/FileNameText").GetComponent<Text>();
			this.elapsedTimeText     = this.playbackPanel.transform.Find("ElapsedTime/ElapsedTimeText").GetComponent<Text>();
			this.totalTimeText       = this.playbackPanel.transform.Find("TotalTime/TotalTimeText").GetComponent<Text>();
			this.timeSlider          = this.playbackPanel.transform.Find("TimeSlider").GetComponent<Slider>();
			this.playButton          = this.playbackPanel.transform.Find("PlayButton").GetComponent<Button>();
			this.speedDropdown       = this.playbackPanel.transform.Find("Speed/SpeedDropdown").GetComponent<Dropdown>();
			this.repeatToggle        = this.playbackPanel.transform.Find("Repeat/RepeatToggle").GetComponent<Toggle>();
			this.startTimeInputField = this.playbackPanel.transform.Find("StartTimeInputField").GetComponent<InputField>();
			this.endTimeInputField   = this.playbackPanel.transform.Find("EndTimeInputField").GetComponent<InputField>();
		}

		protected virtual void Start()
		{
			StartCoroutine(this.DisableObjectsAtStartOfPlaybackMode());
			StartCoroutine(this.EnableObservationCamera());
		}

		protected virtual IEnumerator DisableObjectsAtStartOfPlaybackMode()
		{
			while (!IsPlaybackMode())
			{
				yield return null;
			}

			this.targetsToDisableOnPlaybackMode.ForEach(target => target.SetActive(false));
		}

		protected virtual IEnumerator EnableObservationCamera()
		{
			while (!IsPlaybackMode())
			{
				yield return null;
			}
			this.observationCamera.SetActive(true);
		}

		// Update is called once per frame
		protected virtual void LateUpdate()
		{
			this.isStepChanged = this.step != this.preStep;

			this.isFileRead = this.step == Step.Waiting && this.preStep == Step.Initializing;

			if (this.step == Step.Playing)
			{
				this.UpdateData(Time.deltaTime * this.playingSpeed);
			}
			else if (this.step == Step.Waiting && this.isInitialized)
			{
				this.UpdateData(this.deltaTime);

				this.deltaTime = 0.0f;
			}

			this.preStep = this.step;
		}


		public virtual bool Initialize(string filePath = "")
		{
			if (this.step == Step.Waiting)
			{
				this.step = Step.Initializing;

				if (filePath != "")
				{
					this.filePath = filePath;
				}

				this.isInitialized = false;
				this.canInstantiateRootObjects = false;
				this.rootObjectNames = new HashSet<string>();
				this.errorMsg = string.Empty;

				StartCoroutine(this.StartInitializing());

				return true;
			}

			return false;
		}


		public virtual bool Play(float startTime = 0.0f)
		{
			if (this.step == Step.Waiting && this.isInitialized)
			{
				this.StartPlaying(startTime);
				return true;
			}

			return false;
		}

		public virtual bool Stop()
		{
			if (this.step == Step.Playing)
			{
				this.StopPlaying();
				return true;
			}

			return false;
		}


		protected virtual IEnumerator StartInitializing()
		{
			Thread threadGetRootObjects = new Thread(new ParameterizedThreadStart(this.PrepareRootObjectNames));
			threadGetRootObjects.Start(this.filePath);

			while (!this.canInstantiateRootObjects)
			{
				yield return null;
			}

			this.InstantiateAdditionalRootObjects();

			this.DisableComponent();

			this.StartInitializingEvents();

			Thread threadReadData = new Thread(new ThreadStart(this.ReadDataFromFile));
			threadReadData.Start();
		}


		protected virtual void PrepareRootObjectNames(object filePathObj)
		{
			try
			{
				string filePath = (string)filePathObj;

				if (!File.Exists(filePath))
				{
					throw new Exception("Playback file NOT found. Path=" + filePath);
				}

				// File open
				StreamReader streamReader = new StreamReader(filePath);

				while (streamReader.Peek() >= 0)
				{
					string lineStr = streamReader.ReadLine();

					string[] columnArray = lineStr.Split(new char[] { '\t' }, 2);

					if (columnArray.Length < 2) { continue; }

					string headerStr = columnArray[0];
					string dataStr = columnArray[1];

					string[] headerArray = headerStr.Split(',');

					// Transform data
					if (headerArray[1] == WorldPlaybackCommon.DataType1Transform)
					{
						// Definition
						if (headerArray[2] == WorldPlaybackCommon.DataType2TransformDef)
						{
							string[] dataArray = dataStr.Split('\t');

							foreach (string transformPath in dataArray)
							{
								this.rootObjectNames.Add(transformPath.Split(new char[] { '/' }, 2)[0]);
							}
						}
					}
				}

				streamReader.Close();

				this.canInstantiateRootObjects = true;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
			}
		}


		protected virtual void StartInitializingEvents()
		{
			this.transformController = new PlaybackTransformEventController(this.filePath);  // Transform
			this.stringController = new PlaybackStringEventController(this.stringDataDestinations);  // String Data

			this.transformController.StartInitializingEvents();
			this.stringController.StartInitializingEvents();
		}

		protected virtual void InstantiateAdditionalRootObjects()
		{
			List<string> existingObjects = new List<string>();

			foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
			{
				if (!rootObj.activeInHierarchy) { continue; }

				existingObjects.Add(rootObj.name);
			}

			foreach (string rootObjectName in this.rootObjectNames)
			{
				if (!existingObjects.Contains(rootObjectName))
				{
					string resourceName = rootObjectName.Split('#')[1];

					SIGVerseLogger.Info("Add object. name=" + resourceName);

					UnityEngine.Object resourceObj = Resources.Load(resourceName);
					GameObject resourceGameObject = null;

					if (resourceObj == null)
					{
						foreach (GameObject avatarSource in this.avatarSources)
						{
							if (avatarSource.name == resourceName)
							{
								resourceGameObject = avatarSource;
								break;
							}
						}

						if(resourceGameObject == null)
						{
							SIGVerseLogger.Error("Couldn't find a object. name=" + rootObjectName);
							continue;
						}
					}
					else
					{
						resourceGameObject = (GameObject)resourceObj;
					}
					
					GameObject instance = MonoBehaviour.Instantiate(resourceGameObject);
					instance.name = rootObjectName;

					List<Component> allComponents = instance.GetComponentsInChildren<Component>().ToList();

					allComponents.ForEach(component => this.DisablePlayerComponent(component));
				}
			}
		}

		protected virtual void DisableComponent()
		{
			List<Component> allComponents = GameObject.FindObjectsOfType<Component>().ToList(); // High Load

			allComponents.ForEach(component => this.DisableComponent(component));
		}

		protected virtual void DisableComponent(Component component)
		{
			Type type = component.GetType();

			if (type.IsSubclassOf(typeof(Collider)))
			{
				((Collider)component).enabled = false;
			}
			else if (type == typeof(Rigidbody))
			{
				((Rigidbody)component).isKinematic = true;
				((Rigidbody)component).velocity = Vector3.zero;
				((Rigidbody)component).angularVelocity = Vector3.zero;
			}
			else if(type == typeof(Animator))
			{
				((Animator)component).enabled = false;
			}
			else
			{
				foreach(string scriptAssemblyQualifiedName in this.scriptsAssemblyQualifiedName)
				{
					if(type.AssemblyQualifiedName == scriptAssemblyQualifiedName)
					{
						((MonoBehaviour)component).enabled = false; // scriptAssemblyQualifiedName is subclass of MonoBehaviour.
					}
				}
			}
		}

		protected virtual void DisablePlayerComponent(Component component)
		{
			Type type = component.GetType();

			if (type.IsSubclassOf(typeof(Behaviour)))
			{
				if (type == typeof(Camera) && this.enablePlayerCamera)
				{
					((Camera)component).stereoTargetEye = StereoTargetEyeMask.None;
				}
				else
				{
					((Behaviour)component).enabled = false;
				}
			}
		}

		protected virtual void ReadDataFromFile()
		{
			try
			{
				if (!File.Exists(this.filePath))
				{
					throw new Exception("Playback file NOT found. Path=" + this.filePath);
				}

				// File open
				StreamReader streamReader = new StreamReader(this.filePath);

				while (streamReader.Peek() >= 0)
				{
					string lineStr = streamReader.ReadLine();

					string[] columnArray = lineStr.Split(new char[] { '\t' }, 2);

					if (columnArray.Length < 2) { continue; }

					string headerStr = columnArray[0];
					string dataStr = columnArray[1];

					string[] headerArray = headerStr.Split(',');

					this.ReadData(headerArray, dataStr);
				}

				streamReader.Close();

				SIGVerseLogger.Info("Playback player : File reading finished.");

				this.endTime = this.GetTotalTime();

				SIGVerseLogger.Info("Playback player : Total time=" + this.endTime);

				this.isInitialized = true;

				this.step = Step.Waiting;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message + "\n\n" + ex.StackTrace);

				this.errorMsg = "Cannot read the file !";
				this.step = Step.Waiting;
			}
		}


		protected virtual void ReadData(string[] headerArray, string dataStr)
		{
			this.transformController.ReadEvents(headerArray, dataStr); // Transform
			this.stringController.ReadEvents(headerArray, dataStr); // String Data
		}


		protected virtual void StartPlaying(float startTime)
		{
			SIGVerseLogger.Info("( Start the world playback playing from " + startTime + "[s] )");

			this.step = Step.Playing;

			this.UpdateIndexAndElapsedTime(startTime);
		}


		protected virtual void UpdateIndexAndElapsedTime(float elapsedTime)
		{
			this.elapsedTime = elapsedTime;

			this.deltaTime = 0.0f;

			this.transformController.UpdateIndex(elapsedTime); // Transform
			this.stringController.UpdateIndex(elapsedTime); // String Data
		}


		protected virtual void StopPlaying()
		{
			SIGVerseLogger.Info("( Stop the world playback playing )");

			this.step = Step.Waiting;
		}


		protected virtual void UpdateData(float deltaTime)
		{
			this.elapsedTime += deltaTime;

			if (this.elapsedTime > this.endTime)
			{
				if (this.isRepeating)
				{
					// Wait until the next start
					if (this.elapsedTime > this.endTime + repeatInterval)
					{
						this.UpdateDataByLatest(this.startTime);
					}
				}
				else
				{
					this.Stop();
				}
				return;
			}

			this.transformController.ExecutePassedLatestEvents(this.elapsedTime, deltaTime); // Transform
			this.stringController.ExecutePassedAllEvents(this.elapsedTime, deltaTime); // String Data
		}


		protected virtual void UpdateDataByLatest(float elapsedTime)
		{
			this.UpdateIndexAndElapsedTime(elapsedTime);

			this.transformController.ExecuteLatestEvents(); // Transforms
		}

		protected virtual float GetTotalTime()
		{
			return Mathf.Max(this.transformController.GetTotalTime(), this.stringController.GetTotalTime());
		}


		//----------------------------   GUI related codes are below   ---------------------------------------------

		protected virtual void OnGUI()
		{
			// Update a text of status
			if (this.errorMsg != string.Empty)
			{
				this.statusText.text = this.errorMsg;
				this.SetTextColorAlpha(this.statusText, 1.0f);

				return;
			}

			switch (this.step)
			{
				case Step.Waiting:
				{
					if (this.isStepChanged)
					{
						Debug.Log("Waiting");

						if (this.isInitialized)
						{
							this.readFileButton.interactable = true;
							this.timeSlider.interactable = true;
							this.playButton.interactable = true;
							this.speedDropdown.interactable = true;
							this.repeatToggle.interactable = true;
							this.startTimeInputField.interactable = true;
							this.endTimeInputField.interactable = true;
						}

						if (this.isFileRead)
						{
							this.SetTextColorAlpha(this.statusText, 0.0f);

							this.fileNameText.text = Path.GetFileName(this.filePath);
							this.totalTimeText.text = Math.Ceiling(this.GetTotalTime()).ToString(TotalTimeFormat);
							this.totalTimeInt = int.Parse(this.totalTimeText.text);

							this.ResetTimeSlider();
							this.SetStartTime(0);
							this.SetEndTime(this.totalTimeInt);

							this.UpdateDataByLatest(0);

							this.isFileRead = false;
						}

						this.SetTextColorAlpha(this.statusText, 0.0f);

						this.playButton.image.sprite = this.playSprite;

						this.isStepChanged = false;
					}

					this.UpdateTimeDisplay();

					break;
				}
				case Step.Initializing:
				{
					if (this.isStepChanged)
					{
						SIGVerseLogger.Info("Initializing");

						this.statusText.text = "Reading...";

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);
					break;
				}
				case Step.Playing:
				{
					if (this.isStepChanged)
					{
						SIGVerseLogger.Info("Playing");

						this.statusText.text = "Playing...";

						this.playButton.image.sprite = this.pauseSprite;

						this.readFileButton.interactable = false;
						this.timeSlider.interactable = false;
						this.speedDropdown.interactable = false;
						this.repeatToggle.interactable = false;
						this.startTimeInputField.interactable = false;
						this.endTimeInputField.interactable = false;

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);

					this.UpdateTimeDisplay();

					this.timeSlider.value = Mathf.Clamp((this.elapsedTime - this.startTime) / (this.endTime - this.startTime), 0.0f, 1.0f);

					break;
				}
			}
		}

		private float GetElapsedTimeUsingSlider()
		{
			return this.startTime + (this.endTime - this.startTime) * this.timeSlider.value;
		}

		private void SetTextColorAlpha(Text text, float alpha)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
		}

		private void ResetTimeSlider()
		{
			this.timeSlider.value = 0.0f;
		}

		private void UpdateTimeDisplay()
		{
			float time = (this.elapsedTime < this.endTime) ? this.elapsedTime : this.endTime;

			this.elapsedTimeText.text = time.ToString(ElapsedTimeFormat);
		}

		private void SetStartTime(int startTime)
		{
			this.startTime = startTime;
			this.startTimeInputField.text = startTime.ToString();
		}

		private void SetEndTime(int endTime)
		{
			this.endTime = endTime;
			this.endTimeInputField.text = endTime.ToString();
		}


		public bool IsPlaybackMode()
		{
			return this.playbackPanel.activeSelf;
		}


		public void OnPlayButtonClick()
		{
			if (this.step == Step.Waiting && this.isInitialized)
			{
				switch (this.speedDropdown.value)
				{
					case 0: { this.playingSpeed = 1.0f; break; }
					case 1: { this.playingSpeed = 2.0f; break; }
					case 2: { this.playingSpeed = 4.0f; break; }
					case 3: { this.playingSpeed = 8.0f; break; }
				}

				this.isRepeating = this.repeatToggle.isOn;

				if (!this.Play(this.GetElapsedTimeUsingSlider())) { SIGVerseLogger.Warn("Cannot start the world playing"); }
			}
			else if (this.step == Step.Playing)
			{
				if (!this.Stop()) { SIGVerseLogger.Warn("Cannot stop the world playing"); }
			}
		}

		public void OnStartTimeEndEdit()
		{
			if (this.startTimeInputField.text != string.Empty)
			{
				int startTimeInt = int.Parse(this.startTimeInputField.text);

				if (startTimeInt < 0)
				{
					this.SetStartTime(0);
				}
				else if (startTimeInt >= this.endTime)
				{
					this.SetStartTime((int)Math.Floor(this.endTime) - 1);
				}
				else
				{
					this.SetStartTime(startTimeInt);
				}
			}
			else
			{
				this.SetStartTime(0);
			}

			this.ResetTimeSlider();
			this.UpdateDataByLatest(this.startTime);
			this.UpdateTimeDisplay();
		}

		public void OnEndTimeEndEdit()
		{
			if (this.endTimeInputField.text != string.Empty)
			{
				int endTimeInt = int.Parse(this.endTimeInputField.text);

				if (endTimeInt <= this.startTime)
				{
					this.SetEndTime((int)Math.Ceiling(this.startTime) + 1);
				}
				else if (endTimeInt > this.totalTimeInt)
				{
					this.SetEndTime(this.totalTimeInt);
				}
				else
				{
					this.SetEndTime(endTimeInt);
				}
			}
			else
			{
				this.SetEndTime(this.totalTimeInt);
			}

			this.ResetTimeSlider();
			this.UpdateDataByLatest(this.startTime);
			this.UpdateTimeDisplay();
		}

		public void OnSliderChanged()
		{
			if (!this.timeSlider.interactable) { return; }

			if (this.step == Step.Waiting && this.isInitialized)
			{
				this.deltaTime = this.GetElapsedTimeUsingSlider() - this.elapsedTime;
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			this.scriptsAssemblyQualifiedName = this.scriptsToDisableOnPlaybackMode
				.Where(x => x!=null && x.GetClass()!=null && x.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
				.Select(x => x.GetClass().AssemblyQualifiedName).ToList();
		}

		[CustomEditor(typeof(WorldPlaybackPlayer))]
		public class WorldPlaybackPlayerEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				WorldPlaybackPlayer worldPlaybackPlayer = base.target as WorldPlaybackPlayer;

				GUILayout.Space(30);
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
				EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("scriptsAssemblyQualifiedName", EditorStyles.boldLabel);

				EditorGUI.indentLevel++;
				foreach(string scriptAssemblyQualifiedName in worldPlaybackPlayer.scriptsAssemblyQualifiedName)
				{
					EditorGUILayout.LabelField(scriptAssemblyQualifiedName);
				}
				EditorGUI.indentLevel--;
			}
		}
#endif
	}
}

