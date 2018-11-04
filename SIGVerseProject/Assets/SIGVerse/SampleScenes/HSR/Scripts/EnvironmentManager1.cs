//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.SceneManagement;
//using SIGVerse.Common;
//using SIGVerse.ToyotaHSR;
//using UnityEngine.UI;

//namespace SIGVerse.SampleScenes.Hsr
//{
//	public class EnvironmentManager1 : MonoBehaviour, IRosMsgReceiveHandler
//	{
//		private const int SendingAreYouReadyInterval = 1000;

//		private const string MsgTellMe  = "Please tell me";
//		private const string MsgPointIt = "Please point it";

//		//-----------------------------

//		public List<GameObject> environments;

//		public AudioSource objectCollisionAudioSource;

//		//-----------------------------

//		private EnvironmentManagerUtils1 tool;

//		private GameObject mainMenu;

//		private string taskMessage;

//		private Dictionary<string, bool> receivedMessageMap;


//		void Awake()
//		{
//			try
//			{
//				this.tool = new EnvironmentManagerUtils(this);

//				this.mainMenu = GameObject.FindGameObjectWithTag("MainMenu");
//			}
//			catch (Exception exception)
//			{
//				Debug.LogError(exception);
//				SIGVerseLogger.Error(exception.Message);
//				SIGVerseLogger.Error(exception.StackTrace);
//				this.ApplicationQuitAfter1sec();
//			}
//		}


//		// Use this for initialization
//		void Start()
//		{
//			List<GameObject> graspables = this.tool.GetGraspables();

//			for (int i=0; i<graspables.Count; i++)
//			{
//				Rigidbody rigidbody = graspables[i].GetComponent<Rigidbody>();

//				rigidbody.constraints
//					= RigidbodyConstraints.FreezeRotation |
//					  RigidbodyConstraints.FreezePositionX |
//					  RigidbodyConstraints.FreezePositionZ;

//				rigidbody.maxDepenetrationVelocity = 0.5f;

//				StartCoroutine(this.tool.LoosenRigidbodyConstraints(rigidbody));
//			}

//			this.PreProcess();
//		}


//		private void PreProcess()
//		{
//			this.taskMessage          = this.tool.GetTaskMessage();

//			SIGVerseLogger.Info(this.taskMessage);

//			this.receivedMessageMap = new Dictionary<string, bool>();
//			this.receivedMessageMap.Add(MsgTellMe,   false);
//			this.receivedMessageMap.Add(MsgPointIt,  false);

//			SIGVerseLogger.Info("Task start!");
//		}


//		// Update is called once per frame
//		void Update ()
//		{
//			try
//			{
//				if (this.receivedMessageMap[MsgTellMe])
//				{
//					this.SendRosMessage(this.taskMessage);
//					this.tool.AddSpeechQueModerator(this.taskMessage, true);

//					this.receivedMessageMap[MsgTellMe] = false;
//				}

//				if (this.tool.IsPlacementCheckFinished())
//				{
//					bool isSucceeded = this.tool.IsPlacementSucceeded();

//					if (isSucceeded)
//					{
//						this.SendPanelNotice("Task Completed");

//						this.tool.AddSpeechQueModerator("Excellent!");
//					}
//				}
//			}
//			catch (Exception exception)
//			{
//				Debug.LogError(exception);
//				SIGVerseLogger.Error(exception.Message);
//				SIGVerseLogger.Error(exception.StackTrace);
//				this.ApplicationQuitAfter1sec();
//			}
//		}

//		private void ApplicationQuitAfter1sec()
//		{
//			Thread.Sleep(1000);
//			Application.Quit();
//		}



//		private void SendRosMessage(string message)
//		{
//			ExecuteEvents.Execute<IRosMsgSendHandler>
//			(
//				target: this.gameObject, 
//				eventData: null, 
//				functor: (reciever, eventData) => reciever.OnSendRosMessage(message)
//			);
//		}

//		private void SendPanelNotice(string message)
//		{
//			//PanelNoticeStatus noticeStatus = new PanelNoticeStatus(message, fontSize, color, 2.0f);

//			//// For changing the notice of a panel
//			//ExecuteEvents.Execute<IPanelNoticeHandler>
//			//(
//			//	target: this.mainMenu, 
//			//	eventData: null, 
//			//	functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
//			//);
//		}


//		public void OnReceiveRosMessage(SIGVerse.RosBridge.std_msgs.String rosMsg)
//		{
//			if(this.receivedMessageMap.ContainsKey(rosMsg.data))
//			{
//				this.receivedMessageMap[rosMsg.data] = true;
//			}
//			else
//			{
//				SIGVerseLogger.Warn("Received Illegal message : " + rosMsg);
//			}
//		}
//	}
//}

