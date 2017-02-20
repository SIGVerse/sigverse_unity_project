using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.Common;

namespace SIGVerse.Common
{
	public class SIGVerseMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private const string timeFormat = "#####0.0";
		private const float defaultTimeScale = 1.0f;

		[HeaderAttribute("Panels")]
		public GameObject mainPanel;
		public GameObject messagePanel;
		public GameObject infoPanel;

		[HeaderAttribute("Time")]
		public GameObject timeValObj;

		[HeaderAttribute("Informations")]
		public GameObject sceneNameObj;
		public GameObject rosIPObj;
		public GameObject rosPortObj;
		//---------------------------------------------------

		private GameObject canvas;

		private Text timeValueText;

		private GameObject draggingPanel;

		private Image mainPanelImage;
		private GameObject targetsOfHiding;

		private bool canSeeMainPanel;
		private bool canSeeMessagePanel;
		private bool canSeeInfoPanel;

		private bool isRunning = false;

		private float elapsedTime = 0.0f;


		void Awake()
		{
			Time.timeScale = 0.0f;

			DontDestroyOnLoad(this);

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
		{
			this.sceneNameObj.GetComponent<Text>().text = SceneManager.GetActiveScene().name;

			if (loadedScene.buildIndex != 0)
			{
				this.canvas.SetActive(true);
			}
		}


		// Use this for initialization
		void Start()
		{
			this.rosIPObj.GetComponent<Text>().text = ConfigManager.Instance.configInfo.rosIP;
			this.rosPortObj.GetComponent<Text>().text = ConfigManager.Instance.configInfo.rosPort;

			this.canvas = this.transform.GetComponentInChildren<Canvas>().gameObject;

			this.timeValueText = this.timeValObj.GetComponent<Text>();

			this.mainPanelImage = this.mainPanel.GetComponent<Image>();

			this.targetsOfHiding = this.mainPanel.transform.FindChild("TargetsOfHiding").gameObject;

			this.canvas.SetActive(false);
			this.mainPanel.SetActive(true);
			this.messagePanel.SetActive(false);
			this.infoPanel.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{
			this.elapsedTime += Time.deltaTime;

			this.timeValueText.text = this.elapsedTime.ToString(timeFormat);
		}

		public void ClickHiddingButton()
		{
			if (this.mainPanelImage.enabled)
			{
				this.canSeeMainPanel = this.mainPanelImage.enabled;
				this.canSeeMessagePanel = this.messagePanel.activeSelf;
				this.canSeeInfoPanel = this.infoPanel.activeSelf;

				this.mainPanelImage.enabled = false;
				this.targetsOfHiding.SetActive(false);
				this.messagePanel.SetActive(false);
				this.infoPanel.SetActive(false);
			}
			else
			{
				if (this.canSeeMainPanel)
				{
					this.mainPanelImage.enabled = true;
					this.targetsOfHiding.SetActive(true);
				}
				if (this.canSeeMessagePanel)
				{
					this.messagePanel.SetActive(true);
				}
				if (this.canSeeInfoPanel)
				{
					this.infoPanel.SetActive(true);
				}
			}
		}

		public void ClickStartButton(Text buttonText)
		{
			if (this.isRunning)
			{
				Time.timeScale = 0.0f;
				buttonText.text = "Start";
				this.isRunning = false;
			}
			else
			{
				Time.timeScale = defaultTimeScale;
				buttonText.text = "Stop";
				this.isRunning = true;
			}
		}

		public void ClickInfoButton()
		{
			if (this.infoPanel.activeSelf)
			{
				this.infoPanel.SetActive(false);
			}
			else
			{
				this.infoPanel.SetActive(true);
			}
		}

		public void ClickMessageButton()
		{
			if (this.messagePanel.activeSelf)
			{
				this.messagePanel.SetActive(false);
			}
			else
			{
				this.messagePanel.SetActive(true);
			}
		}

		public void ClickQuitButton()
		{
			//TODO Want to reboot
			Debug.Log("Quit!!!");
			Application.Quit();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.pointerEnter == null) { return; }

			Transform selectedObj = eventData.pointerEnter.transform;

			do
			{
				if (selectedObj.gameObject.GetInstanceID() == this.mainPanel.GetInstanceID() ||
					selectedObj.gameObject.GetInstanceID() == this.infoPanel.GetInstanceID() ||
					selectedObj.gameObject.GetInstanceID() == this.messagePanel.GetInstanceID())
				{
					this.draggingPanel = selectedObj.gameObject;
					break;
				}

				selectedObj = selectedObj.transform.parent;

			} while (selectedObj.transform.parent != null);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (this.draggingPanel == null) { return; }

			this.draggingPanel.transform.position += (Vector3)eventData.delta;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this.draggingPanel = null;
		}
	}
}
