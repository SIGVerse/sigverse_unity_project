using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.XR.Management;
using Unity.Netcode.Transports.UTP;
using TMPro;

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public class LoginManager : MonoBehaviour
	{
		[HeaderAttribute("NetCode Info")]
		public NetworkManager networkManager;
		public string ngoAddress = "127.0.0.1";
		public ushort ngoPort = 7777;

		[HeaderAttribute("Spawn Info")]
		public NetworkPrefabsList networkPrefabsList;

		public string humanPrefabName;
		public string robotPrefabName;

		public string humanName;
		public string robotName;

		public Transform robotSpawnPosition;
		public Transform humanSpawnPosition;

		[HeaderAttribute("UI MainMenu")]
		public GameObject mainMenu;

		// -----------------------
		private GameObject mainPanel;

		private TMP_Dropdown hostDropdown;
		private TMP_Text errorMessageText;

		private XRLoader activeLoader;

		void Awake()
		{
			this.mainPanel  = this.mainMenu.transform.Find("Canvas/MainPanel").gameObject;

			this.hostDropdown     = this.mainPanel.transform.Find("HostDropdown").GetComponent<TMP_Dropdown>();
			this.errorMessageText = this.mainPanel.transform.Find("ErrorMessageText").GetComponent<TMP_Text>();

			UnityTransport unityTransport = networkManager.GetComponent<UnityTransport>();
			unityTransport.ConnectionData.Address = this.ngoAddress;
			unityTransport.ConnectionData.Port    = this.ngoPort;
		}


		void Start()
		{
			this.humanSpawnPosition.gameObject.SetActive(false);
			this.robotSpawnPosition.gameObject.SetActive(false);

			NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
			NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
		}

		private void OnClientConnectedCallback(ulong clientId)
		{
			Debug.Log("Client Connected. ClientId="+clientId);
		}

		private void OnClientDisconnectCallback(ulong clientId)
		{
			Debug.Log("Client Disconnected. ClientId="+clientId);
		}

		private void OnTransportFailure()
		{
			Debug.LogError("NetworkTransport fails.");
			this.mainPanel.SetActive(true);
			this.errorMessageText.gameObject.SetActive(true);
			this.errorMessageText.text = "NetworkTransport fails.";
		}

		public void Login(bool isHuman)
		{
			bool isHost = IsHost(isHuman);

			Transform spawnPosition = isHuman ? this.humanSpawnPosition : this.robotSpawnPosition;

			GameObject avatarPrefab = this.networkPrefabsList.PrefabList.Where(x => x.Prefab.name == (isHuman ? this.humanPrefabName : this.robotPrefabName)).First().Prefab;

			if (isHuman)
			{
				StartCoroutine(this.InstantiateHuman(isHost, avatarPrefab, spawnPosition));
			}
			else
			{
				NetworkManager.Singleton.NetworkConfig.PlayerPrefab.GetComponent<PlayerSpawner>()
					.SetPlayerInfo(avatarPrefab.name, spawnPosition.position, GetAvatarRotation(avatarPrefab, spawnPosition), this.robotName);

				if (isHost){ NetworkManager.Singleton.StartHost(); }
				else       { NetworkManager.Singleton.StartClient(); }
			}

			this.mainPanel.SetActive(false);
		}

		private bool IsHost(bool isHuman)
		{
			if(isHuman)
			{
				return this.hostDropdown.options[this.hostDropdown.value].text.Contains("Human");
			}
			else
			{
				return this.hostDropdown.options[this.hostDropdown.value].text.Contains("Robot");
			}
		}

		public IEnumerator InstantiateHuman(bool isHost, GameObject avatarPrefab, Transform spawnPosition)
		{
			if (!this.humanPrefabName.StartsWith("Dummy"))
			{
				// Initialize XR System
				XRManagerSettings xrManagerSettings = XRGeneralSettings.Instance.Manager;

				if (xrManagerSettings == null) { SIGVerseLogger.Error("xrManagerSettings == null"); yield break; }

				if (xrManagerSettings.activeLoader == null)
				{
					yield return xrManagerSettings.InitializeLoader();
				}

				this.activeLoader = xrManagerSettings.activeLoader;

				if (this.activeLoader == null)
				{
					Debug.LogError("Initializing XR Failed.");
					yield break;
				}

				xrManagerSettings.activeLoader.Start();
			}

			NetworkManager.Singleton.NetworkConfig.PlayerPrefab.GetComponent<PlayerSpawner>()
				.SetPlayerInfo(avatarPrefab.name, spawnPosition.position, GetAvatarRotation(avatarPrefab, spawnPosition), this.humanName);

			if (isHost){ NetworkManager.Singleton.StartHost(); }
			else       { NetworkManager.Singleton.StartClient(); }
		}

		private static Quaternion GetAvatarRotation(GameObject avatarPrefab, Transform spawnPosition)
		{
			Vector3 avatarEuler = avatarPrefab.transform.localEulerAngles;
			return Quaternion.Euler(avatarEuler.x, avatarEuler.y+spawnPosition.localEulerAngles.y, avatarEuler.z);
		}


		void OnDestroy()
		{
			// It is mandatory to perform this termination process.
			if(this.activeLoader != null)
			{
				this.activeLoader.Stop();
				XRGeneralSettings.Instance.Manager.DeinitializeLoader();
			}
			if (NetworkManager.Singleton != null)
			{
				NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnectedCallback;
				NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
			}
		}
	}
}
