#pragma warning disable 0414

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Collections;
using System.Threading;

#if SIGVERSE_MYSQL
	using MySql.Data.MySqlClient;
#endif

namespace SIGVerse.SampleScenes.Playbacker
{
	[RequireComponent(typeof (PlaybackerCommon))]
	public class PlaybackerRecorder : MonoBehaviour
	{
		private const string TitleTextText       = "Text Recorder";
		private const string TitleTextDababase   = "Database Recorder";
		private const string StatusTextRecording = "Recording ...";
		private const string StatusTextWriting   = "Writing ...";

		private const string MysqlSchemaName = "sigverse";
		private const string MysqlTableName  = "playbacker_motion_data";

		[Header("Recorder Properties")]
		public Text       titleText;
		public Text       statusText;
		public Button     recordButton;
		public GameObject settings;
		public GameObject textSettings;
		public GameObject databaseSettings;

		//-----------------------------------------------------
		private ModeType modeType;

		private List<GameObject> targetObjects;
		private Dropdown changeModeDropdown;
		private Button   resetObjectsButton;
	
		private Text recordButtonText;

		private InputField intervalInputField;
		private Toggle     withScaleToggle;
		private InputField outputFilePathInputField;

		private InputField uniqueIdInputField;
		private InputField mysqlIpInputField;
		private InputField mysqlPortInputField;
		private InputField mysqlUserInputField;
		private InputField mysqlPassInputField;

		private bool isRecording = false;
		private bool isWriting   = false;

		private float elapsedTime = 0.0f;
		private float previousRecordedTime = 0.0f;

		private List<Transform> targetTransformInstances;

		private string       savedHeaderStrings;
		private List<string> savedMotionStrings;


		// Use this for initialization
		void Start ()
		{
			this.targetObjects      = this.GetComponent<PlaybackerCommon>().targetObjects;
			this.changeModeDropdown = this.GetComponent<PlaybackerCommon>().changeModeDropdown;
			this.resetObjectsButton = this.GetComponent<PlaybackerCommon>().resetObjectsButton;

			this.recordButtonText = this.recordButton.GetComponentInChildren<Text>();

			this.intervalInputField = this.settings.GetComponentInChildren<InputField>();
			this.withScaleToggle    = this.settings.GetComponentInChildren<Toggle>();

			this.outputFilePathInputField = this.textSettings.GetComponentInChildren<InputField>();

			this.uniqueIdInputField  = this.databaseSettings.transform.Find("UniqueIdInputField").GetComponent<InputField>();
			this.mysqlIpInputField   = this.databaseSettings.transform.Find("IpInputField")      .GetComponent<InputField>();
			this.mysqlPortInputField = this.databaseSettings.transform.Find("PortInputField")    .GetComponent<InputField>();
			this.mysqlUserInputField = this.databaseSettings.transform.Find("UserInputField")    .GetComponent<InputField>();
			this.mysqlPassInputField = this.databaseSettings.transform.Find("PassInputField")    .GetComponent<InputField>();

			this.ChangeModeDropdownValueChanged(this.changeModeDropdown);

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
				case ModeType.TextRecorder:
				{
					this.titleText.text = TitleTextText;

					this.textSettings    .SetActive(true);
					this.databaseSettings.SetActive(false);

					break;
				}
				case ModeType.DatabaseRecorder:
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

			if (this.isRecording)
			{
				this.SaveMotions();
			}
		}

		void OnGUI()
		{
			if(this.isRecording || this.isWriting)
			{
				this.statusText.color = new Color(this.statusText.color.r, this.statusText.color.g, this.statusText.color.b, Mathf.Sin(5.0f * this.elapsedTime) * 0.5f + 0.5f);
			}
		}


		private void SaveMotions()
		{
			if (1000.0*(this.elapsedTime-this.previousRecordedTime) < float.Parse(intervalInputField.text)) { return; }

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

				if(this.withScaleToggle.isOn)
				{
					lineStr += "," +
						Math.Round(transform.localScale.x,   4, MidpointRounding.AwayFromZero) + "," +
						Math.Round(transform.localScale.y,   4, MidpointRounding.AwayFromZero) + "," +
						Math.Round(transform.localScale.z,   4, MidpointRounding.AwayFromZero);
				}
			}

			this.savedMotionStrings.Add(lineStr);

			this.previousRecordedTime = this.elapsedTime;
		}


		public void OnRecordButtonClick()
		{
			try
			{
#if !SIGVERSE_MYSQL
				if(this.modeType == ModeType.DatabaseRecorder)
				{
					throw new Exception("Don't define SIGVERSE_MYSQL");
				}
#endif
				if (!this.isRecording)
				{
					this.StartRecording();
				}
				else
				{
					this.StartCoroutine(StopRecording());
				}

				EventSystem.current.SetSelectedGameObject(null);
			}
			catch(Exception ex)
			{
				this.HandleException(ex);
			}
		}


		private void StartRecording()
		{
				SIGVerseLogger.Info("Recorder : Initialise");

				this.DisableSettings();
		
				DateTime dateTime = DateTime.Now;

				this.targetTransformInstances = new List<Transform>();

				List<string> linkPathList = new List<string>(); // It is for only duplication check

				// Make header line
				this.savedHeaderStrings = string.Empty;

				this.savedHeaderStrings += "0.0," + PlaybackerCommon.TypeDef; // Elapsed time is dummy.

				// Create targets transform list
				foreach (GameObject targetObj in targetObjects)
				{
					Transform[] transforms = targetObj.transform.GetComponentsInChildren<Transform>();

					foreach (Transform transform in transforms)
					{
						// Save Transform instance list
						this.targetTransformInstances.Add(transform);

						string linkPath = SIGVerseUtils.GetHierarchyPath(transform);

						// Make a header line
						this.savedHeaderStrings += "\t" + linkPath;

						// Duplication check
						if (linkPathList.Contains(linkPath))
						{
							SIGVerseLogger.Error("Objects in the same path exist. path=" + linkPath);
							throw new Exception("Objects in the same path exist.");
						}

						linkPathList.Add(linkPath);
					}
				}

				if(this.modeType == ModeType.TextRecorder)
				{
					this.InitializeText();
				}
				else
				{
#if SIGVERSE_MYSQL
					this.InitializeDatabase();
#endif
				}

				this.savedMotionStrings = new List<string>();

				// Change Buttons
				this.recordButtonText.text = PlaybackerCommon.ButtonTextStop;

				// Reset elapsed time
				this.elapsedTime = 0.0f;
				this.previousRecordedTime = 0.0f;


				SIGVerseLogger.Info("Recorder : Recording start.");

				this.statusText.text  = StatusTextRecording;

				this.isRecording = true;
		}

		private void DisableSettings()
		{
			this.changeModeDropdown.interactable = false;
			this.resetObjectsButton.interactable = false;

			this.intervalInputField.interactable = false;
			this.withScaleToggle   .interactable = false;

			this.outputFilePathInputField.interactable = false;

			this.uniqueIdInputField .interactable = false;
			this.mysqlIpInputField  .interactable = false;
			this.mysqlPortInputField.interactable = false;
			this.mysqlUserInputField.interactable = false;
			this.mysqlPassInputField.interactable = false;
		}


		private IEnumerator StopRecording()
		{
			this.isRecording = false;

			this.statusText.text  = StatusTextWriting;

			this.isWriting = true;

			if(this.modeType == ModeType.TextRecorder)
			{
				Thread threadWriteMotions = new Thread(new ThreadStart(this.WriteMotionsToFile));
				threadWriteMotions.Start();
			}
				else
			{
#if SIGVERSE_MYSQL
				Thread threadWriteMotions = new Thread(new ThreadStart(this.InsertMotionsToMySQL));
				threadWriteMotions.Start();
#endif
			}

			while(this.isWriting)
			{
				yield return new WaitForSeconds(0.1f); 
			}

			// Change Buttons
			this.recordButtonText.text = PlaybackerCommon.ButtonTextStart;

			this.EnableSettings();

			this.DisableStatusText();

			SIGVerseLogger.Info("Recorder : Recording finished.");
		}

		private void EnableSettings()
		{
			this.changeModeDropdown.interactable = true;
			this.resetObjectsButton.interactable = true;

			this.intervalInputField.interactable = true;
			this.withScaleToggle   .interactable = true;

			this.outputFilePathInputField.interactable = true;

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


		private void InitializeText()
		{
			SIGVerseLogger.Info("outputFilePath=" + this.outputFilePathInputField.text);

			string directoryName = Path.GetDirectoryName(this.outputFilePathInputField.text);

			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			// File open
			StreamWriter motionsDataWriter = new StreamWriter(this.outputFilePathInputField.text, false);

			motionsDataWriter.Flush();
			motionsDataWriter.Close();
		}

		private void WriteMotionsToFile()
		{
			try
			{
				StreamWriter motionsDataWriter = new StreamWriter(this.outputFilePathInputField.text, true);

				// Write header
				motionsDataWriter.WriteLine(this.savedHeaderStrings);

				// Write motion data
				foreach (string savedMotionString in this.savedMotionStrings)
				{
					motionsDataWriter.WriteLine(savedMotionString);
				}

				motionsDataWriter.Flush();
				motionsDataWriter.Close();

				this.isWriting = false;
			}
			catch(Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
				Application.Quit();
			}
		}

#if SIGVERSE_MYSQL
		private void InitializeDatabase()
		{
			// Delete records with the same recording_id
			SIGVerseLogger.Info("MySQL = " + this.mysqlIpInputField.text + ":" + this.mysqlPortInputField.text + ", Unique ID="+this.uniqueIdInputField.text);

			string connString = 
				"server  =" + this.mysqlIpInputField.text+";"+
				"port    =" + this.mysqlPortInputField.text+";"+
				"database=" + MysqlSchemaName+";"+
				"userid  =" + this.mysqlUserInputField.text+";"+
				"password=" + this.mysqlPassInputField.text;

			MySqlConnection mysqlConn = new MySqlConnection(connString);
			mysqlConn.Open();

			string deleteSql = "DELETE FROM "+MysqlSchemaName+"."+MysqlTableName+" WHERE recording_id="+uniqueIdInputField.text;

			MySqlCommand mysqlCommand = new MySqlCommand(deleteSql, mysqlConn);

			int deletedNum = mysqlCommand.ExecuteNonQuery();

			if(deletedNum > 0)
			{
				SIGVerseLogger.Info("Deleted " + deletedNum + "records.");
			}

			mysqlCommand.Dispose();
			mysqlConn.Close();
		}


		private void InsertMotionsToMySQL()
		{
			MySqlConnection mysqlConn  = null;
			MySqlCommand mysqlCommand = null;

			try
			{
				string connString = 
					"server  =" + this.mysqlIpInputField.text+";"+
					"port    =" + this.mysqlPortInputField.text+";"+
					"database=" + MysqlSchemaName+";"+
					"userid  =" + this.mysqlUserInputField.text+";"+
					"password=" + this.mysqlPassInputField.text;

				mysqlConn = new MySqlConnection(connString);
				mysqlConn.Open();


				// Write header
				string insertSql = string.Empty;

				this.CreateInsertQuery(ref insertSql, this.savedHeaderStrings);

				mysqlCommand = new MySqlCommand(insertSql, mysqlConn);
				mysqlCommand.ExecuteNonQuery();

				SIGVerseLogger.Info("Inserted " + "1" + " records.");

				// Write motion data
				int cnt = 0;
				insertSql = string.Empty;

				foreach (string savedMotionString in this.savedMotionStrings)
				{
					this.CreateInsertQuery(ref insertSql, savedMotionString);
					cnt++;

					// Execute the insert query every 100 times.
					if (cnt == 100)
					{
						mysqlCommand = new MySqlCommand(insertSql, mysqlConn);
						mysqlCommand.ExecuteNonQuery();

						cnt = 0;
						insertSql = string.Empty;
					}
				}

				if (cnt!=0)
				{
					mysqlCommand = new MySqlCommand(insertSql, mysqlConn);
					mysqlCommand.ExecuteNonQuery();
				}

				SIGVerseLogger.Info("Inserted " + savedMotionStrings.Count + " records.");
				
				mysqlCommand.Dispose();
				mysqlConn.Close();

				this.isWriting = false;
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

		private void CreateInsertQuery(ref string insertSql, string motionDataString)
		{
			string[] motionStringArray = motionDataString.Split("\t".ToCharArray(), 2);
			string[] headerArray = motionStringArray[0].Split(",".ToCharArray());

			int elapsedTime = (int)(float.Parse(headerArray[0]) * 1000);
			int dataType    = (int)(int  .Parse(headerArray[1]));

			string valueString =
				"(" +
					this.uniqueIdInputField.text + "," +
					elapsedTime + "," +
					dataType + "," +
					"'" + motionDataString + "'" +
				")";

			if (insertSql==string.Empty)
			{
				// Create the insert query if it is empty
				insertSql =
					"INSERT INTO " + MysqlSchemaName + "." + MysqlTableName + " " +
					"(recording_id, elapsed_time, data_type, motion_data) " +
					"VALUES " +
					valueString;
			}
			else
			{
				// Add values if the insert query is not empty
				insertSql += "," + valueString;
			}
		}
	}
}

