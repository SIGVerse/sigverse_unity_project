//using System;
//using System.IO;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using SIGVerse.Common;
//using SIGVerse.ToyotaHSR;
//using System.Collections;
//using SIGVerse.RosBridge;
//using SIGVerse.SIGVerseRosBridge;

//namespace SIGVerse.SampleScenes.Hsr
//{
//	[Serializable]
//	public class RelocatableObjectInfo
//	{
//		public string name;
//		public Vector3 position;
//		public Vector3 eulerAngles;
//	}

//	[Serializable]
//	public class EnvironmentInfo
//	{
//		public string taskMessage;
//		public string correctedTaskMessage;
//		public string environmentName;
//		public bool   isEnvironmentNameSent;
//		public string graspingTargetName;
//		public string destinationName;
//		public List<RelocatableObjectInfo> graspablesPositions;
//		public List<RelocatableObjectInfo> destinationsPositions; 
//	}

//	public class SpeechInfo
//	{
//		public string message;
//		public string gender;
//		public bool   canCancel;

//		public SpeechInfo(string message, string gender, bool canCancel)
//		{
//			this.message   = message;
//			this.gender    = gender;
//			this.canCancel = canCancel;
//		}
//	}

//	public class EnvironmentManagerUtils1
//	{
//		private const string TagRobot                      = "Robot";
//		private const string TagModerator                  = "Moderator";
//		private const string TagGraspingCandidates         = "GraspingCandidates";
//		private const string TagGraspingCandidatesPosition = "GraspingCandidatesPosition";
//		private const string TagDestinationCandidates      = "DestinationCandidates";

//		private const string JudgeTriggersName    = "JudgeTriggers";
//		private const string DeliveryPositionName = "DeliveryPosition";

//		private const float  DeliveryThreshold = 0.3f;

//		public const string SpeechExePath  = "../TTS/ConsoleSimpleTTS.exe";
//		public const string SpeechLanguage = "409";
//		public const string SpeechGenderModerator = "Male";
//		public const string SpeechGenderHsr       = "Female";


//		private IRosConnection[] rosConnections;

//		private string environmentName;
//		private string taskMessage;
//		private bool   isEnvironmentNameSent;

//		private GameObject graspingTarget;
//		private List<GameObject> graspables;
//		private List<GameObject> graspingCandidates;

//		private List<GameObject> graspingCandidatesPositions;

//		private GameObject destination;
//		private List<GameObject> destinationCandidates;

//		private GameObject robot;


//		private bool? isPlacementSucceeded;


//		private System.Diagnostics.Process speechProcess;

//		private Queue<SpeechInfo> speechInfoQue;
//		private SpeechInfo latestSpeechInfo;

//		private bool isSpeechUsed;


//		public EnvironmentManagerUtils(EnvironmentManager moderator)
//		{
//			EnvironmentInfo environmentInfo = this.EnableEnvironment(moderator.environments);

//			this.GetGameObjects(environmentInfo);

//			this.Initialize(moderator.objectCollisionAudioSource);
//		}

//		private EnvironmentInfo EnableEnvironment(List<GameObject> environments)
//		{
//			if(environments.Count != (from environment in environments select environment.name).Distinct().Count())
//			{
//				throw new Exception("There is the name conflict of environments.");
//			}


//			EnvironmentInfo environmentInfo = null;

//			GameObject activeEnvironment = (from environment in environments where environment.activeSelf==true select environment).FirstOrDefault();

//			if(activeEnvironment!=null)
//			{
//				this.environmentName = activeEnvironment.name;

//				SIGVerseLogger.Warn("Selected an active environment. name=" + activeEnvironment.name);
//			}
//			else
//			{
//				this.environmentName = environments[UnityEngine.Random.Range(0, environments.Count)].name;
//			}

//			foreach (GameObject environment in environments)
//			{
//				if(environment.name==this.environmentName)
//				{
//					environment.SetActive(true);
//				}
//				else
//				{
//					environment.SetActive(false);
//				}
//			}

//			return environmentInfo;
//		}


//		private void GetGameObjects(EnvironmentInfo environmentInfo)
//		{
//			this.robot = GameObject.FindGameObjectWithTag(TagRobot);

//			// Get grasping candidates
//			this.graspingCandidates = this.ExtractGraspingCandidates(environmentInfo);

//			this.graspables = new List<GameObject>();

//			this.graspables.AddRange(this.graspingCandidates);

//			// Check the name conflict of graspables.
//			if(this.graspables.Count != (from graspable in this.graspables select graspable.name).Distinct().Count())
//			{
//				throw new Exception("There is the name conflict of graspable objects.");
//			}

//			SIGVerseLogger.Info("Count of Graspables = " + this.graspables.Count);


//			// Get grasping candidates positions
//			this.graspingCandidatesPositions = GameObject.FindGameObjectsWithTag(TagGraspingCandidatesPosition).ToList<GameObject>();

//			if (this.graspables.Count > this.graspingCandidatesPositions.Count)
//			{
//				throw new Exception("graspables.Count > graspingCandidatesPositions.Count.");
//			}
//			else
//			{
//				SIGVerseLogger.Info("Count of GraspingCandidatesPosition = " + this.graspingCandidatesPositions.Count);
//			}


//			this.destinationCandidates = GameObject.FindGameObjectsWithTag(TagDestinationCandidates).ToList<GameObject>();

//			if(this.destinationCandidates.Count == 0)
//			{
//				throw new Exception("Count of DestinationCandidates is zero.");
//			}

//			// Check the name conflict of destination candidates.
//			if(this.destinationCandidates.Count != (from destinations in this.destinationCandidates select destinations.name).Distinct().Count())
//			{
//				throw new Exception("There is the name conflict of destination candidates objects.");
//			}

//			SIGVerseLogger.Info("Count of Destinations = " + this.destinationCandidates.Count);
//		}

//		public List<GameObject> ExtractGraspingCandidates(EnvironmentInfo environmentInfo)
//		{
//			// Get grasping candidates
//			List<GameObject> graspingCandidates = GameObject.FindGameObjectsWithTag(TagGraspingCandidates).ToList<GameObject>();

//			if (graspingCandidates.Count == 0)
//			{
//				throw new Exception("Count of GraspingCandidates is zero.");
//			}

//			return graspingCandidates;
//		}

//		private void Initialize(AudioSource objectCollisionAudioSource)
//		{
//			Dictionary<RelocatableObjectInfo, GameObject> graspablesPositionMap    = null; //key:GraspablePositionInfo,   value:Graspables
//			Dictionary<RelocatableObjectInfo, GameObject> destinationsPositionsMap = null; //key:DestinationPositionInfo, value:DestinationCandidate

//			this.graspingTarget        = this.DecideGraspingTarget();
//			this.destination           = this.DecideDestination();

//			graspablesPositionMap    = this.CreateGraspablesPositionMap();
//			destinationsPositionsMap = this.CreateDestinationsPositionsMap();


//			if(this.destination.tag!=TagModerator)
//			{ 
//				// Add Placement checker to triggers
//				Transform judgeTriggersTransform = this.destination.transform.Find(JudgeTriggersName);

//				if (judgeTriggersTransform==null) { throw new Exception("No Judge Triggers object"); }

//				judgeTriggersTransform.gameObject.AddComponent<PlacementChecker>();
//			}

			
//			foreach (KeyValuePair<RelocatableObjectInfo, GameObject> pair in graspablesPositionMap)
//			{
//				pair.Value.transform.position    = pair.Key.position;
//				pair.Value.transform.eulerAngles = pair.Key.eulerAngles;

////				Debug.Log(pair.Key.name + " : " + pair.Value.name);
//			}

//			foreach (KeyValuePair<RelocatableObjectInfo, GameObject> pair in destinationsPositionsMap)
//			{
//				pair.Value.transform.position    = pair.Key.position;
//				pair.Value.transform.eulerAngles = pair.Key.eulerAngles;

////				Debug.Log(pair.Key.name + " : " + pair.Value.name);
//			}

//			this.taskMessage           = this.CreateTaskMessage();
//			this.isEnvironmentNameSent = true;


//			this.rosConnections = SIGVerseUtils.FindObjectsOfInterface<IRosConnection>();

//			SIGVerseLogger.Info("ROS connection : count=" + this.rosConnections.Length);


//			// Set up the voice (Using External executable file)
//			this.speechProcess = new System.Diagnostics.Process();
//			this.speechProcess.StartInfo.FileName = Application.dataPath + "/" + SpeechExePath;
//			this.speechProcess.StartInfo.CreateNoWindow = true;
//			this.speechProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

//			this.isSpeechUsed = System.IO.File.Exists(this.speechProcess.StartInfo.FileName);

//			this.speechInfoQue = new Queue<SpeechInfo>();

//			SIGVerseLogger.Info("Text-To-Speech: " + Application.dataPath + "/" + SpeechExePath);


//			this.isPlacementSucceeded   = null;
//		}


//		public List<GameObject> GetGraspables()
//		{
//			return this.graspables;
//		}


//		public IEnumerator LoosenRigidbodyConstraints(Rigidbody rigidbody)
//		{
//			while(!rigidbody.IsSleeping())
//			{
//				yield return null;
//			}

//			rigidbody.constraints = RigidbodyConstraints.None;
//		}


//		public GameObject DecideGraspingTarget()
//		{
//			// Decide the grasping target
//			GameObject graspingTarget = this.graspingCandidates[UnityEngine.Random.Range(0, this.graspingCandidates.Count)];

//			SIGVerseLogger.Info("Grasping target is " + graspingTarget.name);

//			return graspingTarget;
//		}


//		public GameObject DecideDestination()
//		{
//			// Decide the destination
//			GameObject destination = this.destinationCandidates[UnityEngine.Random.Range(0, this.destinationCandidates.Count)];

//			SIGVerseLogger.Info("Destination is " + destination.name);

//			return destination;
//		}


//		public void DeactivateGraspingCandidatesPositions()
//		{
//			foreach (GameObject graspingCandidatesPosition in this.graspingCandidatesPositions)
//			{
//				graspingCandidatesPosition.SetActive(false);
//			}
//		}

//		public Dictionary<RelocatableObjectInfo, GameObject> CreateGraspablesPositionMap()
//		{
//			// Shuffle lists
//			this.graspables                  = this.graspables                 .OrderBy(i => Guid.NewGuid()).ToList();
//			this.graspingCandidatesPositions = this.graspingCandidatesPositions.OrderBy(i => Guid.NewGuid()).ToList();

//			Dictionary<RelocatableObjectInfo, GameObject> graspingCandidatesMap = new Dictionary<RelocatableObjectInfo, GameObject>();

//			for (int i=0; i<this.graspables.Count; i++)
//			{
//				RelocatableObjectInfo graspablePositionInfo = new RelocatableObjectInfo();

//				graspablePositionInfo.name        = this.graspables[i].name;
//				graspablePositionInfo.position    = this.graspingCandidatesPositions[i].transform.position - new Vector3(0, this.graspingCandidatesPositions[i].transform.localScale.y * 0.49f, 0);
//				graspablePositionInfo.eulerAngles = this.graspingCandidatesPositions[i].transform.eulerAngles;

//				graspingCandidatesMap.Add(graspablePositionInfo, this.graspables[i]);
//			}

//			return graspingCandidatesMap;
//		}


//		public Dictionary<RelocatableObjectInfo, GameObject> CreateDestinationsPositionsMap()
//		{
//			Dictionary<RelocatableObjectInfo, GameObject> destinationsPositionsMap = new Dictionary<RelocatableObjectInfo, GameObject>();

//			for (int i=0; i<this.destinationCandidates.Count; i++)
//			{
//				RelocatableObjectInfo destinationPositionInfo = new RelocatableObjectInfo();

//				destinationPositionInfo.name        = this.destinationCandidates[i].name;
//				destinationPositionInfo.position    = this.destinationCandidates[i].transform.position;
//				destinationPositionInfo.eulerAngles = this.destinationCandidates[i].transform.eulerAngles;

//				destinationsPositionsMap.Add(destinationPositionInfo, this.destinationCandidates[i]);
//			}

//			return destinationsPositionsMap;
//		}


//		public string GetEnvironmentName()
//		{
//			if(this.isEnvironmentNameSent)
//			{
//				return this.environmentName;
//			}
//			else
//			{
//				return string.Empty;
//			}
//		}

//		private string CreateTaskMessage()
//		{
//			return "Grasp the " + this.graspingTarget.name.Split('#')[0] + " and send it to the " + this.destination.name.Split('#')[0] + ".";
//		}

//		public string GetTaskMessage()
//		{
//			return this.taskMessage;
//		}


//		public void ControlSpeech(bool isTaskFinished)
//		{
//			if(!this.isSpeechUsed){ return; }

//			// Cancel current speech that can be canceled when task finished
//			try
//			{
//				if (isTaskFinished && this.latestSpeechInfo!=null && this.latestSpeechInfo.canCancel && !this.speechProcess.HasExited)
//				{
//					this.speechProcess.Kill();
//				}
//			}
//			catch (Exception)
//			{
//				SIGVerseLogger.Warn("Do nothing even if an error occurs");
//				// Do nothing even if an error occurs
//			}


//			if (this.speechInfoQue.Count <= 0){ return; }

//			// Return if the current speech is not over
//			if (this.latestSpeechInfo!=null && !this.speechProcess.HasExited){ return; }


//			SpeechInfo speechInfo = this.speechInfoQue.Dequeue();

//			if(isTaskFinished && speechInfo.canCancel){ return; }

//			this.latestSpeechInfo = speechInfo;

//			string message = this.latestSpeechInfo.message.Replace("_", " "); // Remove "_"

//			this.speechProcess.StartInfo.Arguments = "\"" + message + "\" \"Language=" + SpeechLanguage + "; Gender=" + this.latestSpeechInfo.gender + "\"";

//			try
//			{
//				this.speechProcess.Start();

//				SIGVerseLogger.Info("Spoke :" + message);
//			}
//			catch (Exception)
//			{
//				SIGVerseLogger.Warn("Could not speak :" + message);
//			}
//		}


//		private void AddSpeechQue(string message, string gender, bool canCancel = false)
//		{
//			if(!this.isSpeechUsed){ return; }

//			this.speechInfoQue.Enqueue(new SpeechInfo(message, gender, canCancel));
//		}

//		public void AddSpeechQueModerator(string message, bool canCancel = false)
//		{
//			this.AddSpeechQue(message, SpeechGenderModerator, canCancel);
//		}

//		public void AddSpeechQueHsr(string message, bool canCancel = false)
//		{
//			this.AddSpeechQue(message, SpeechGenderHsr, canCancel);
//		}

//		public bool IsSpeaking()
//		{
//			return this.speechInfoQue.Count != 0 || (this.latestSpeechInfo!=null && !this.speechProcess.HasExited);
//		}



//		public bool IsPlacementCheckFinished()
//		{
//			return isPlacementSucceeded != null;
//		}

//		public bool IsPlacementSucceeded()
//		{
//			return (bool)isPlacementSucceeded;
//		}


//		public IEnumerator UpdatePlacementStatus(MonoBehaviour moderator)
//		{
//			if(this.graspingTarget.transform.root == this.robot.transform.root)
//			{
//				this.isPlacementSucceeded = false;

//				SIGVerseLogger.Info("Target placement failed: HSR has the grasping target.");
//			}
//			else
//			{
//				PlacementChecker placementChecker = this.destination.GetComponentInChildren<PlacementChecker>();

//				IEnumerator<bool?> isPlaced = placementChecker.IsPlaced(this.graspingTarget);

//				yield return moderator.StartCoroutine(isPlaced);

//				this.isPlacementSucceeded = (bool)isPlaced.Current;
//			}
//		}


//		public bool IsConnectedToRos()
//		{
//			foreach(IRosConnection rosConnection in this.rosConnections)
//			{
//				if(!rosConnection.IsConnected())
//				{
//					return false;
//				}
//			}
//			return true;
//		}

//		public IEnumerator ClearRosConnections()
//		{
//			yield return new WaitForSecondsRealtime (1.5f);

//			foreach(IRosConnection rosConnection in this.rosConnections)
//			{
//				rosConnection.Clear();
//			}

//			SIGVerseLogger.Info("Clear ROS connections");
//		}

//		public IEnumerator CloseRosConnections()
//		{
//			yield return new WaitForSecondsRealtime (1.5f);

//			foreach(IRosConnection rosConnection in this.rosConnections)
//			{
//				rosConnection.Close();
//			}

//			SIGVerseLogger.Info("Close ROS connections");
//		}
//	}
//}

