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

		[HeaderAttribute("Objects")]
		public GameObject humanSource;
		public GameObject robotSource;

		public GameObject[] rootsOfSyncTarget;

		[HeaderAttribute("UI")]
		public GameObject mainPanel;
		public GameObject noticePanel;
		public Text roomNameText;
		public Text roomNameDefaultText;

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

			this.roomName = (roomNameText.text!=string.Empty) ? roomNameText.text : roomNameDefaultText.text;

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
//			Debug.Log("OnConnectedToMaster");

			PhotonNetwork.JoinOrCreateRoom(this.roomName, new RoomOptions(), TypedLobby.Default);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
//			Debug.Log("OnDisconnected");
		}

		public override void OnJoinedRoom()
		{
			if(this.isHuman)
			{
				PhotonNetwork.NickName = HumanNamePrefix + "#" + PhotonNetwork.LocalPlayer.ActorNumber;

				PhotonNetwork.Instantiate(this.humanSource.name, this.humanSource.transform.position, this.humanSource.transform.rotation);
			}
			else
			{
				PhotonNetwork.NickName = RobotNamePrefix + "#" + PhotonNetwork.LocalPlayer.ActorNumber;

				XRSettings.enabled = false;

				PhotonNetwork.Instantiate(this.robotSource.name, this.robotSource.transform.position, this.robotSource.transform.rotation);
			}

			this.mainPanel.SetActive(false);
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

#else
	public class PunLauncher : MonoBehaviour
	{
		void Start()
		{
			throw new Exception("SIGVERSE_PUN is NOT defined.");
		}
	}
#endif

#if SIGVERSE_PUN && UNITY_EDITOR
	[CustomEditor(typeof(PunLauncher))]
	public class PunLauncherEditor : Editor
	{
		void OnEnable()
		{
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Update Photon View", GUILayout.Width(200), GUILayout.Height(40)))
				{
					Undo.RecordObject(target, "Update Photon View");

					PunLauncher punLauncher = (PunLauncher)target;

					// Remove photon scripts
					RemoveScripts<PhotonTransformView>();
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
						photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
						photonView.ObservedComponents = new List<Component>();

						PhotonTransformView photonTransformView = Undo.AddComponent<PhotonTransformView>(roomObject);
//						PhotonRigidbodyView photonRigidbodyView = Undo.AddComponent<PhotonRigidbodyView>(roomObject);

						photonView.ObservedComponents.Add(photonTransformView);
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
