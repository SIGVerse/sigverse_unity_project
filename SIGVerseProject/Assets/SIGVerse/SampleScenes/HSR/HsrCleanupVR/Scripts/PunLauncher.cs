using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Linq;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if SIGVERSE_PUN
using Photon.Pun;
using Photon.Realtime;
#endif

namespace SIGVerse.SampleScenes.Hsr.HsrCleanupVR
{
#if SIGVERSE_PUN

	public class PunLauncher : MonoBehaviourPunCallbacks
	{
//		public const string GameVersion = "0.1";
		public const string HumanNamePrefix = "Human";
		public const string RobotNamePrefix = "HSR";
		private const string SubViewControllerStr = "SubviewController";

		[HeaderAttribute("Spawn Info")]
		public int humanMaxNumber;
		public Vector3[] humanPosisions;
		public Vector3[] humanEulerAngles;

		public int robotMaxNumber;
		public Vector3[] robotPosisions;
		public Vector3[] robotEulerAngles;


		[HeaderAttribute("Objects")]
		public GameObject humanSource;
		public GameObject robotSource;
		public GameObject[] rootsOfSyncTarget;

		[HeaderAttribute("Scripts")]
		public ChatManager chatManager;

		[HeaderAttribute("UI")]
		public Button humanLoginButton;
		public Button robotLoginButton;
		public GameObject mainPanel;
		public GameObject noticePanel;
		public Text roomNameText;
		public Text roomNameDefaultText;
		public Text errorMessageText;

//		[HideInInspector]
		public List<GameObject> roomObjects;

		// -----------------------
		private string roomName;

		private bool isHuman;

		void Awake()
		{
			XRSettings.enabled = true;
		}

		void Start()
		{
			PhotonNetwork.AutomaticallySyncScene = true;

			// Check for duplication
			List<string> duplicateNames = roomObjects.GroupBy(obj => obj.name).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

			if (duplicateNames.Count > 0)
			{
				throw new Exception("There are multiple objects with the same name. e.g. " + duplicateNames[0]);
			}

			// Manage the synchronized room objects using singleton
			RoomObjectManager.Instance.roomObjects = roomObjects;
		}

		//void Update()
		//{
		//}

		public void Connect(bool isHuman)
		{
			this.isHuman = isHuman;

			this.roomName = (roomNameText.text != string.Empty) ? roomNameText.text : roomNameDefaultText.text;

			PhotonNetwork.GameVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;

			PhotonNetwork.ConnectUsingSettings();
		}

		public void GetOwnership()
		{
			PhotonView photonView = this.GetComponent<PhotonView>();

			photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}

		public override void OnConnectedToMaster()
		{
//			Debug.Log("OnConnectedToMaster RoomName=" + this.roomName);

			PhotonNetwork.JoinOrCreateRoom(this.roomName, new RoomOptions(), TypedLobby.Default);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
//			Debug.Log("OnDisconnected");

			this.chatManager.ClearChatUserList();
		}

		public override void OnJoinedRoom()
		{
			if (this.ShouldDisconnect(out int numberOfLogins))
			{
				this.StartCoroutine(this.Disconnect());
				return;
			}

			if (this.isHuman)
			{
				PhotonNetwork.NickName = HumanNamePrefix + "#" + PhotonNetwork.LocalPlayer.ActorNumber;

				PhotonNetwork.Instantiate(this.humanSource.name, this.humanPosisions[numberOfLogins], Quaternion.Euler(this.humanEulerAngles[numberOfLogins]));
			}
			else
			{
				PhotonNetwork.NickName = RobotNamePrefix + "#" + PhotonNetwork.LocalPlayer.ActorNumber;

				XRSettings.enabled = false;

				PhotonNetwork.Instantiate(this.robotSource.name, this.robotPosisions[numberOfLogins], Quaternion.Euler(this.robotEulerAngles[numberOfLogins]));
			}

			this.mainPanel.SetActive(false);
		}

		private bool ShouldDisconnect(out int numberOfLogins)
		{
			Player[] players = PhotonNetwork.PlayerListOthers;

			numberOfLogins = this.isHuman? players.Count(p => p.NickName.StartsWith(HumanNamePrefix)) : players.Count(p => p.NickName.StartsWith(RobotNamePrefix));

			if ((this.isHuman && numberOfLogins>=this.humanMaxNumber) || (!this.isHuman && numberOfLogins >= this.robotMaxNumber))
			{
				string errorMessage = "Over capacity - logs you out.";

				SIGVerseLogger.Warn(errorMessage);

				this.errorMessageText.text = errorMessage;
				this.errorMessageText.gameObject.SetActive(true);

				this.humanLoginButton.interactable = false;
				this.robotLoginButton.interactable = false;

				return true;
			}

			return false;
		}

		private IEnumerator Disconnect()
		{
			yield return new WaitForSeconds(3.0f);

			PhotonNetwork.LeaveRoom();
			PhotonNetwork.Disconnect();

			this.errorMessageText.gameObject.SetActive(false);

			this.humanLoginButton.interactable = true;
			this.robotLoginButton.interactable = true;
		}


		public void SetRoomObjects(List<GameObject> roomObjects)
		{
			this.roomObjects = roomObjects;
		}

		public static void AddPhotonTransformView(PhotonView photonView, GameObject synchronizedTarget, bool syncPos = false, bool syncRot = true)
		{
			PhotonTransformView photonTransformView = synchronizedTarget.AddComponent<PhotonTransformView>();

			photonTransformView.m_SynchronizePosition = syncPos;
			photonTransformView.m_SynchronizeRotation = syncRot;
			photonTransformView.m_SynchronizeScale    = false;

			photonView.ObservedComponents.Add(photonTransformView);
		}

		public static void EnableSubview(GameObject operationTarget)
		{
			operationTarget.transform.root.Find(SubViewControllerStr).gameObject.SetActive(true);

			// Update the camera list before enable SubviewOptionController
			GameObject.FindObjectOfType<SubviewManager>().UpdateCameraList();

			SubviewOptionController[] subviewOptionControllers = operationTarget.GetComponentsInChildren<SubviewOptionController>();

			foreach (SubviewOptionController subviewOptionController in subviewOptionControllers)
			{
				subviewOptionController.enabled = true;
			}
		}

		//public override void OnLeftRoom()
		//{
		//	SceneManager.LoadScene(0);
		//}

		//public void LeaveRoom()
		//{
		//	PhotonNetwork.LeaveRoom();
		//}
	}

#endif

#if SIGVERSE_PUN && UNITY_EDITOR
	[CustomEditor(typeof(PunLauncher))]
	public class PunLauncherEditor : Editor
	{
		//void OnEnable()
		//{
		//}

		public override void OnInspectorGUI()
		{
			PunLauncher punLauncher = (PunLauncher)target;

			if(punLauncher.humanMaxNumber != punLauncher.humanPosisions.Length || punLauncher.humanMaxNumber != punLauncher.humanEulerAngles.Length)
			{
				Undo.RecordObject(target, "Update Human Spawn Info");
				Array.Resize(ref punLauncher.humanPosisions,   punLauncher.humanMaxNumber);
				Array.Resize(ref punLauncher.humanEulerAngles, punLauncher.humanMaxNumber);
			}

			if (punLauncher.robotMaxNumber != punLauncher.robotPosisions.Length || punLauncher.robotMaxNumber != punLauncher.robotEulerAngles.Length)
			{
				Undo.RecordObject(target, "Update Robot Spawn Info");
				Array.Resize(ref punLauncher.robotPosisions,   punLauncher.robotMaxNumber);
				Array.Resize(ref punLauncher.robotEulerAngles, punLauncher.robotMaxNumber);
			}

			base.OnInspectorGUI();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Update Photon View", GUILayout.Width(200), GUILayout.Height(40)))
				{
					Undo.RecordObject(target, "Update Photon View");

					// Remove photon scripts
					RemoveScripts<PhotonTransformView>();
					RemoveScripts<LocalTransformView>();
					RemoveScripts<PhotonRigidbodyView>();
					RemoveScripts<PhotonView>();
					RemoveScripts<PunOwnerChangerForObject>();

					// Add photon scripts
					List<GameObject> roomObjects = new List<GameObject>();

					foreach (GameObject sourceOfSyncTarget in punLauncher.rootsOfSyncTarget)
					{
						Rigidbody[] syncTargetRigidbodies = sourceOfSyncTarget.GetComponentsInChildren<Rigidbody>();

						foreach (Rigidbody syncTargetRigidbody in syncTargetRigidbodies)
						{
							roomObjects.Add(syncTargetRigidbody.gameObject);
						}
					}

					punLauncher.SetRoomObjects(roomObjects);

					foreach (GameObject roomObject in roomObjects)
					{
						PhotonView photonView = Undo.AddComponent<PhotonView>(roomObject);
						photonView.OwnershipTransfer = OwnershipOption.Takeover;
						photonView.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
						photonView.ObservedComponents = new List<Component>();

//						PhotonTransformView photonTransformView = Undo.AddComponent<PhotonTransformView>(roomObject);
						LocalTransformView  localTransformView  = Undo.AddComponent<LocalTransformView>(roomObject);
//						PhotonRigidbodyView photonRigidbodyView = Undo.AddComponent<PhotonRigidbodyView>(roomObject);

//						photonView.ObservedComponents.Add(photonTransformView);
						photonView.ObservedComponents.Add(localTransformView);
//						photonView.ObservedComponents.Add(photonRigidbodyView);

						Undo.AddComponent<PunOwnerChangerForObject>(roomObject);
					}
				}

				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
		}

		private void RemoveScripts<T>() where T : Component
		{
			PunLauncher punLauncher = (PunLauncher)target;

			foreach (GameObject sourceOfSyncTarget in punLauncher.rootsOfSyncTarget)
			{
				T[] photonScripts = sourceOfSyncTarget.GetComponentsInChildren<T>();

				foreach (T photonScript in photonScripts)
				{
					Undo.DestroyObjectImmediate(photonScript);
				}
			}
		}
	}
#endif
}
