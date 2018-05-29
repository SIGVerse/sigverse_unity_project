#pragma warning disable 0414

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Threading;
using System.Collections;

#if SIGVERSE_MYSQL
	using MySql.Data.MySqlClient;
#endif

namespace SIGVerse.SampleScenes.Playbacker
{
	public class UpdatingTransformList
	{
		public float ElapsedTime { get; set; }
		private List<UpdatingTransformData> updatingTransformList;

		/// <summary>
		/// constructor
		/// </summary>
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

	[RequireComponent(typeof (PlaybackerCommon))]
	public class PlaybackerPlayer : MonoBehaviour
	{
		private const string TimeFormat = "#####0.0";

		private const string TitleTextText     = "Text Player";
		private const string TitleTextDababase = "Database Player";
		private const string StatusTextPlaying = "Playing ...";
		private const string StatusTextReading = "Reading ...";

		private const string MysqlSchemaName = "sigverse";
		private const string MysqlTableName = "playbacker_motion_data";

		[Header("Player Properties")]
		public Text       titleText;
		public Text       statusText;
		public Button     playButton;
		public Text       timeText;
		public GameObject textSettings;
		public GameObject databaseSettings;

		//-----------------------------------------------------
		private ModeType modeType;

		private List<GameObject> targetObjects;
		private Dropdown changeModeDropdown;
		private Button   resetObjectsButton;

		private Text playButtonText;

		private InputField inputFilePathInputField;

		private InputField uniqueIdInputField;
		private InputField mysqlIpInputField;
		private InputField mysqlPortInputField;
		private InputField mysqlUserInputField;
		private InputField mysqlPassInputField;

		private bool isPlaying = false;
		private bool isReading = false;

		private Dictionary<string, Transform> targetObjectsPathMap  = new Dictionary<string, Transform>();
		private Dictionary<Animator,  bool> animatorEnableMap       = new Dictionary<Animator,  bool>();
		private Dictionary<Rigidbody, bool> rigidbodyIsKinematicMap = new Dictionary<Rigidbody, bool>();

		private float elapsedTime = 0.0f;


		private List<UpdatingTransformList> playingTransformList;
		private int playingTransformIndex;


		private void Awake()
		{
			this.targetObjects = this.GetComponent<PlaybackerCommon>().targetObjects;
			this.changeModeDropdown = this.GetComponent<PlaybackerCommon>().changeModeDropdown;
			this.resetObjectsButton = this.GetComponent<PlaybackerCommon>().resetObjectsButton;

			this.playButtonText = this.playButton.GetComponentInChildren<Text>();

			this.inputFilePathInputField = this.textSettings.GetComponentInChildren<InputField>();

			this.uniqueIdInputField  = this.databaseSettings.transform.Find("UniqueIdInputField").GetComponent<InputField>();
			this.mysqlIpInputField   = this.databaseSettings.transform.Find("IpInputField")      .GetComponent<InputField>();
			this.mysqlPortInputField = this.databaseSettings.transform.Find("PortInputField")    .GetComponent<InputField>();
			this.mysqlUserInputField = this.databaseSettings.transform.Find("UserInputField")    .GetComponent<InputField>();
			this.mysqlPassInputField = this.databaseSettings.transform.Find("PassInputField")    .GetComponent<InputField>();

			this.ChangeModeDropdownValueChanged(this.changeModeDropdown);

			foreach (GameObject targetObj in targetObjects)
			{
				Transform[] transforms = targetObj.transform.GetComponentsInChildren<Transform>(true);

				foreach (Transform transform in transforms)
				{
					this.targetObjectsPathMap.Add(SIGVerseUtils.GetHierarchyPath(transform), transform);
				}
			}
		}


		// Use this for initialization
		void Start ()
		{
			this.DisableStatusText();
		}

		private void DisableStatusText()
		{
			this.statusText.color = new Color(this.statusText.color.r, this.statusText.color.g, this.statusText.color.b, 0.0f);
		}


		public void ChangeModeDropdownValueChanged(Dropdown dropdown)
		{
			// Change the main panel layout
			this.modeType = (ModeType)dropdown.value;

			switch ((ModeType)dropdown.value)
			{
				case ModeType.TextPlayer:
				{
					this.titleText.text = TitleTextText;

					this.textSettings    .SetActive(true);
					this.databaseSettings.SetActive(false);

					break;
				}
				case ModeType.DatabasePlayer:
				{
					this.titleText.text = TitleTextDababase;

					this.textSettings    .SetActive(false);
					this.databaseSettings.SetActive(true);

					break;
				}
				default:
				{
					break;
				}
			}
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


		void OnGUI()
		{
			if(this.isPlaying || this.isReading)
			{
				this.statusText.color = new Color(this.statusText.color.r, this.statusText.color.g, this.statusText.color.b, Mathf.Sin(5.0f * this.elapsedTime) * 0.5f + 0.5f);
			}

			if(this.isPlaying)
			{
				this.timeText.text = this.elapsedTime.ToString(TimeFormat); ;
			}
		}

		private void PlayMotions()
		{
			// Stop playing when reached the end of the list
			if (this.playingTransformIndex >= this.playingTransformList.Count)
			{
				this.StopPlaying();
				return;
			}

			UpdatingTransformList updatingTransformList = null;

			// Increase the list index until the elapsed time of the list reaches the actual elapsed time
			while (this.elapsedTime >= this.playingTransformList[this.playingTransformIndex].ElapsedTime)
			{
				updatingTransformList = this.playingTransformList[this.playingTransformIndex];

				this.playingTransformIndex++;

				if (this.playingTransformIndex >= this.playingTransformList.Count) { break; }
			}

			if (updatingTransformList == null) { return; }

			// Play
			foreach (UpdatingTransformData updatingTransformData in updatingTransformList.GetUpdatingTransformList())
			{
				updatingTransformData.UpdateTransform();
			}
		}



		public void OnPlayButtonClick()
		{
			try
			{
#if !SIGVERSE_MYSQL
				if(this.modeType == ModeType.DatabasePlayer)
				{
					throw new Exception("Don't define SIGVERSE_MYSQL");
				}
#endif
				if(!this.isPlaying)
				{
					this.StartCoroutine(this.StartPlaying());
				}
				else
				{
					this.StopPlaying();
				}

				EventSystem.current.SetSelectedGameObject(null);
			}
			catch(Exception ex)
			{
				this.HandleException(ex);
			}
		}

		private IEnumerator StartPlaying()
		{
			SIGVerseLogger.Info("Player : Initialise");

			if (!File.Exists(inputFilePathInputField.text))
			{
				throw new Exception("Input File NOT found. File path="+inputFilePathInputField.text);
			}

			this.DisableSettings();

			this.statusText.text  = StatusTextReading;

			this.isReading = true;

			this.playingTransformList = new List<UpdatingTransformList>();


			if(this.modeType == ModeType.TextPlayer)
			{
				Thread threadWriteMotions = new Thread(new ThreadStart(this.ReadMotionsFromFile));
				threadWriteMotions.Start();
			}
			else
			{
#if SIGVERSE_MYSQL
				Thread threadWriteMotions = new Thread(new ThreadStart(this.SelectMotionsFromMySQL));
				threadWriteMotions.Start();
#endif
			}

			while(this.isReading)
			{
				yield return new WaitForSeconds(0.1f); 
			}

			// Change Buttons
			this.playButtonText.text = PlaybackerCommon.ButtonTextStop;


			this.animatorEnableMap.Clear();
			this.rigidbodyIsKinematicMap.Clear();

			// Disable Animators and Rigidbodies
			foreach (GameObject targetObj in targetObjects)
			{
				// Disable animators
				Animator[] animators = targetObj.transform.GetComponentsInChildren<Animator>(true);

				foreach (Animator animator in animators)
				{
					this.animatorEnableMap.Add(animator, animator.enabled);
					animator.enabled = false;
				}

				// Disable rigidbodies
				Rigidbody[] rigidbodies = targetObj.transform.GetComponentsInChildren<Rigidbody>(true);

				foreach (Rigidbody rigidbody in rigidbodies)
				{
					this.rigidbodyIsKinematicMap.Add(rigidbody, rigidbody.isKinematic);

					rigidbody.isKinematic     = true;
					rigidbody.velocity        = Vector3.zero;
					rigidbody.angularVelocity = Vector3.zero;
				}
			}


			SIGVerseLogger.Info("Player : Playing start.");

			// Reset elapsed time
			this.elapsedTime = 0.0f;

			this.playingTransformIndex = 0;

			this.statusText.text  = StatusTextPlaying;

			this.isPlaying = true;
		}


		private void DisableSettings()
		{
			this.changeModeDropdown.interactable = false;
			this.resetObjectsButton.interactable = false;

			this.inputFilePathInputField.interactable = false;

			this.uniqueIdInputField .interactable = false;
			this.mysqlIpInputField  .interactable = false;
			this.mysqlPortInputField.interactable = false;
			this.mysqlUserInputField.interactable = false;
			this.mysqlPassInputField.interactable = false;
		}

		private void StopPlaying()
		{
			this.isPlaying = false;

			// Change Buttons
			this.playButtonText.text = PlaybackerCommon.ButtonTextStart;

			// Enable Animators 
			foreach(KeyValuePair<Animator, bool> pair in this.animatorEnableMap)
			{
				pair.Key.enabled = pair.Value;
			}

			// Enable Rigidbodies
			foreach(KeyValuePair<Rigidbody, bool> pair in this.rigidbodyIsKinematicMap)
			{
				pair.Key.isKinematic     = pair.Value;
				pair.Key.velocity        = Vector3.zero;
				pair.Key.angularVelocity = Vector3.zero;
			}

			this.EnableSettings();

			this.DisableStatusText();

			SIGVerseLogger.Info("Player : Playing finished.");
		}

		private void EnableSettings()
		{
			this.changeModeDropdown.interactable = true;
			this.resetObjectsButton.interactable = true;

			this.inputFilePathInputField.interactable = true;

			this.uniqueIdInputField .interactable = true;
			this.mysqlIpInputField  .interactable = true;
			this.mysqlPortInputField.interactable = true;
			this.mysqlUserInputField.interactable = true;
			this.mysqlPassInputField.interactable = true;
		}

		private void HandleException(Exception ex)
		{
			this.EnableSettings();

			SIGVerseLogger.Error(ex.Message);
			SIGVerseLogger.Error(ex.StackTrace);

			GameObject warningWindow = (GameObject)Resources.Load(SIGVerseUtils.WarningWindowResourcePath);

			warningWindow.GetComponent<SIGVerseWarningWindow>().message.text = "Please check SIGVerse log.\n" + ex.Message;

			Instantiate(warningWindow);
		}


		private void ReadMotionsFromFile()
		{
			try
			{
				// File open
				StreamReader motionsDataReader = new StreamReader(inputFilePathInputField.text);

				List<string> motionsDataList = new List<string>();

				while (motionsDataReader.Peek() >= 0)
				{
					motionsDataList.Add(motionsDataReader.ReadLine());
				}

				motionsDataReader.Close();

				this.CreatePlayingTransformList(motionsDataList);

				this.isReading = false;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
				Application.Quit();
			}
		}

#if SIGVERSE_MYSQL
		private void SelectMotionsFromMySQL()
		{
			MySqlConnection mysqlConn = null;

			try
			{
				string connString =
					"server  =" + this.mysqlIpInputField.text + ";" +
					"port    =" + this.mysqlPortInputField.text + ";" +
					"database=" + MysqlSchemaName + ";" +
					"userid  =" + this.mysqlUserInputField.text + ";" +
					"password=" + this.mysqlPassInputField.text;

				mysqlConn = new MySqlConnection(connString);
				mysqlConn.Open();

				string selectSql = "SELECT * FROM " + MysqlSchemaName + "." + MysqlTableName + " WHERE recording_id=" + uniqueIdInputField.text;

				MySqlCommand mysqlCommand = new MySqlCommand(selectSql, mysqlConn);

				IAsyncResult iAsync = mysqlCommand.BeginExecuteReader();

				while (!iAsync.IsCompleted)
				{
					Thread.Sleep(100);
				}

				MySqlDataReader mysqlDataReader = mysqlCommand.EndExecuteReader(iAsync);

				List<string> motionsDataList = new List<string>();

				while (mysqlDataReader.Read())
				{
					motionsDataList.Add(mysqlDataReader.GetString("motion_data"));
				}

				mysqlDataReader.Close();
				mysqlCommand.Dispose();
				mysqlConn.Close();

				this.CreatePlayingTransformList(motionsDataList);

				this.isReading = false;
			}
			catch (Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);

				if (mysqlConn != null) { mysqlConn.Close(); }
				Application.Quit();
			}
		}
#endif

		private void CreatePlayingTransformList(List<string> motionsDataList)
		{
			List<Transform> transformOrder = new List<Transform>();

			foreach (string motionsData in motionsDataList)
			{
				string[] columnArray = motionsData.Split(new char[] { '\t' }, 2);

				if (columnArray.Length < 2) { continue; }

				string headerStr = columnArray[0];
				string dataStr = columnArray[1];

				string[] headerArray = headerStr.Split(',');
				string[] dataArray = dataStr.Split('\t');

				// Definition
				if (int.Parse(headerArray[1]) == PlaybackerCommon.TypeDef)
				{
					transformOrder.Clear();

	//				Debug.Log("data num=" + dataArray.Length);

					foreach (string transformPath in dataArray)
					{
						if (!this.targetObjectsPathMap.ContainsKey(transformPath))
						{
							throw new Exception("Couldn't find the object that path is " + transformPath);
						}

						transformOrder.Add(this.targetObjectsPathMap[transformPath]);
					}
				}
				// Value
				else if (int.Parse(headerArray[1]) == PlaybackerCommon.TypeVal)
				{
					if (transformOrder.Count == 0) { continue; }

					UpdatingTransformList timeSeriesMotionsData = new UpdatingTransformList();

					timeSeriesMotionsData.ElapsedTime = float.Parse(headerArray[0]);

					for (int i = 0; i < dataArray.Length; i++)
					{
						string[] transformValues = dataArray[i].Split(',');

						UpdatingTransformData transformPlayer = new UpdatingTransformData();
						transformPlayer.UpdatingTransform = transformOrder[i];

						transformPlayer.LocalPosition = new Vector3(float.Parse(transformValues[0]), float.Parse(transformValues[1]), float.Parse(transformValues[2]));
						transformPlayer.LocalRotation = new Vector3(float.Parse(transformValues[3]), float.Parse(transformValues[4]), float.Parse(transformValues[5]));

						if (transformValues.Length == 9)
						{
							transformPlayer.LocalScale = new Vector3(float.Parse(transformValues[6]), float.Parse(transformValues[7]), float.Parse(transformValues[8]));
						}

						timeSeriesMotionsData.AddUpdatingTransform(transformPlayer);
					}

					this.playingTransformList.Add(timeSeriesMotionsData);
				}
			}
		}
	}
}

