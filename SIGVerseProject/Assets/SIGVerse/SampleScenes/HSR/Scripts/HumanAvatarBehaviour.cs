using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using UnityEngine.UI;
using SIGVerse.RosBridge;

namespace SIGVerse.SampleScenes.Hsr
{
	public class HumanAvatarBehaviour : MonoBehaviour, IRosReceivingStringMsgHandler
	{
		public GameObject mainMenu;
		public GameObject rosbridgeScripts;

		//-----------------------------

		public bool go = false;

		private const string MsgTellMe  = "Please tell me";
		private const string MsgPointIt = "Please point it";

		private const string TagRobot                 = "Robot";
		private const string TagGraspables            = "Graspable";
		private const string TagDestinationCandidates = "DestinationCandidate";

		private const string JudgeTriggersForOnName = "JudgeTriggerForOn";
		private const string JudgeTriggersForInName = "JudgeTriggerForIn";

		private Dictionary<string, bool> receivedMessageMap;

		private GameObject robot;
		private List<GameObject> graspables;
		private List<GameObject> destinationCandidates;

		private GameObject graspingTarget;
		private GameObject destination;

		private Rigidbody graspingTargetRigidbody;

		private CleanupAvatarVRHandController avatarLeftHandController;
		private CleanupAvatarVRHandController avatarRightHandController;
		
		private string taskMessage;

		private PlacementChecker placementChecker;

//		private GameObject mainMenu;


		void Awake()
		{
			// Get the robot
			this.robot = GameObject.FindGameObjectWithTag(TagRobot);


			// Get the graspables
			this.graspables = GameObject.FindGameObjectsWithTag(TagGraspables).ToList<GameObject>();

			if (graspables.Count == 0){ throw new Exception("Count of Graspables is zero."); }

			// Check the name conflict of graspables.
			if (this.graspables.Count != (from graspable in this.graspables select graspable.name).Distinct().Count())
			{
				throw new Exception("There is the name conflict of graspable objects.");
			}

			SIGVerseLogger.Info("Count of Graspables = " + this.graspables.Count);


			// Get the destination candidates
			this.destinationCandidates = GameObject.FindGameObjectsWithTag(TagDestinationCandidates).ToList<GameObject>();

			if (this.destinationCandidates.Count == 0){ throw new Exception("Count of DestinationCandidates is zero."); }

			// Check the name conflict of destination candidates.
			if (this.destinationCandidates.Count != (from destinations in this.destinationCandidates select destinations.name).Distinct().Count())
			{
				throw new Exception("There is the name conflict of destination candidates objects.");
			}

			SIGVerseLogger.Info("Count of DestinationCandidates = " + this.destinationCandidates.Count);


			this.graspingTarget = this.graspables           [UnityEngine.Random.Range(0, this.graspables.Count)];
			this.destination    = this.destinationCandidates[UnityEngine.Random.Range(0, this.destinationCandidates.Count)];

			SIGVerseLogger.Info("Grasping target is " + graspingTarget.name);
			SIGVerseLogger.Info("Destination is "     + destination.name);

			this.graspingTargetRigidbody = this.graspingTarget.GetComponentInChildren<Rigidbody>();

			this.avatarLeftHandController  = this.GetComponentsInChildren<CleanupAvatarVRHandController>().Where(item=>item.handType==CleanupAvatarVRHandController.HandType.LeftHand) .First();
			this.avatarRightHandController = this.GetComponentsInChildren<CleanupAvatarVRHandController>().Where(item=>item.handType==CleanupAvatarVRHandController.HandType.RightHand).First();
		}


		// Use this for initialization
		void Start()
		{
			this.receivedMessageMap = new Dictionary<string, bool>();
			this.receivedMessageMap.Add(MsgTellMe, false);
			this.receivedMessageMap.Add(MsgPointIt, false);

			// Add Placement checker to triggers
			Transform judgeTriggerForOn = this.destination.transform.Find(JudgeTriggersForOnName);
			Transform judgeTriggerForIn = this.destination.transform.Find(JudgeTriggersForInName);

			if (judgeTriggerForOn == null && judgeTriggerForIn == null) { throw new Exception("No JudgeTrigger. name=" + this.destination.name); }
			if (judgeTriggerForOn != null && judgeTriggerForIn != null) { throw new Exception("Too many JudgeTrigger. name=" + this.destination.name); }

			if (judgeTriggerForOn != null)
			{
				this.placementChecker = judgeTriggerForOn.gameObject.AddComponent<PlacementChecker>();
				this.placementChecker.Initialize(PlacementChecker.JudgeType.On);

				this.taskMessage = this.CreateTaskMessage("on");
			}
			if (judgeTriggerForIn != null)
			{
				this.placementChecker = judgeTriggerForIn.gameObject.AddComponent<PlacementChecker>();
				this.placementChecker.Initialize(PlacementChecker.JudgeType.In);

				this.taskMessage = this.CreateTaskMessage("in");
			}

			SIGVerseLogger.Info("Task Message:" + this.taskMessage);

			StartCoroutine(this.JudgePlacement());
		}

		private string CreateTaskMessage(string preposition)
		{
			return "Grasp the " + this.graspingTarget.name.Split('#')[0] + ", and put it "+preposition+" the " + this.destination.name.Split('#')[0] + ".";
		}


		// Update is called once per frame
		void Update()
		{
			if (this.receivedMessageMap[MsgTellMe])
			{
				this.SendRosMessage(this.taskMessage);

				this.receivedMessageMap[MsgTellMe] = false;
			}

			if (this.receivedMessageMap[MsgPointIt] || this.go)
			{
				if(UnityEngine.Random.Range(0, 2) == 0)
				{
					// Left Hand
					this.avatarLeftHandController.PointTargetObject(this.graspingTarget);
				}
				else
				{
					// Right Hand
					this.avatarRightHandController.PointTargetObject(this.graspingTarget);
				}

				this.receivedMessageMap[MsgPointIt] = false;
				this.go = false;
			}
		}


		private void SendRosMessage(string message)
		{
			ExecuteEvents.Execute<IRosSendingStringMsgHandler>
			(
				target: this.rosbridgeScripts,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnSendRosStringMsg(message)
			);
		}

		private void SendPanelNotice(string message)
		{
			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(message, 150, PanelNoticeStatus.Green, 2.0f);

			// For changing the notice of the panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);
		}

		private IEnumerator JudgePlacement()
		{
			while(true)
			{
				yield return new WaitForSeconds(1.0f);

				if(this.IsPlaced())
				{
					StartCoroutine(this.SendMessage("Good Job!", 1.0f));
					StartCoroutine(this.SendMessage("Task Finished!", 3.0f));
					break;
				}
			}
		}

		private IEnumerator SendMessage(string message, float waitingTime)
		{
			yield return new WaitForSeconds(waitingTime);

			this.SendRosMessage(message);
			this.SendPanelNotice(message);
		}

		private bool IsPlaced()
		{
			if(Time.time <= 0){ return false; }

			if (this.graspingTarget.transform.root == this.robot.transform.root)
			{
				return false;
			}
			else
			{
				return this.placementChecker.IsPlacedNow(this.graspingTargetRigidbody);
			}
		}

		public void OnReceiveRosStringMsg(SIGVerse.RosBridge.std_msgs.String rosMsg)
		{
			if (this.receivedMessageMap.ContainsKey(rosMsg.data))
			{
				this.receivedMessageMap[rosMsg.data] = true;
			}
			else
			{
				SIGVerseLogger.Warn("Received Illegal message : " + rosMsg.data);
			}
		}
	}
}

