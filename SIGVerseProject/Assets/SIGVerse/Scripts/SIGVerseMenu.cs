using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SIGVerse.Common
{
	public class SIGVerseMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public enum PanelOperationType
		{
			Move, 
			ExpandXY, 
			ExpandX,
			ExpandY,
		}

		public const string SIGVerseMenuName = "SIGVerseMenu";

		private const string TimeFormat = "#####0.0";
		private const float DefaultTimeScale = 1.0f;

		private const float DraggingThreshold = 15;
		private const float MinimumPanelSize  = 100;

		private const int SubviewNum = 4;


		[HeaderAttribute("Major Panels")]
		public Image      backgroundImage;
		public GameObject mainPanel;
		public GameObject subviewPanel;
		public GameObject infoPanel;
		public GameObject messagePanel;

		[HeaderAttribute("Main Panel")]
		public GameObject targetsOfHiding;
		public GameObject startButton;

		[HeaderAttribute("Subview Panels")]
		public List<GameObject> subviewPanels;

		[HeaderAttribute("Time")]
		public GameObject timeValObj;

		[HeaderAttribute("Informations")]
		public GameObject sceneNameObj;
		public GameObject rosIPObj;
		public GameObject rosPortObj;
		//---------------------------------------------------

		private GameObject canvas;

		private Color backgroundDarkColor;
		private Color backgroundBrightColor;

		private Text timeValueText;
		private Text startButtonText;

		private PanelOperationType panelOperationType;
		private GameObject draggingPanel;
		private RectTransform   expandingRectTransform;

		private Image mainPanelImage;

		private bool canSeeMainPanel;
		private bool canSeeSubviewPanel;
		private bool canSeeInfoPanel;
		private bool canSeeMessagePanel;

//		private bool[] canSeeSubviewPanels;

		private bool isRunning = false;

		private float elapsedTime = 0.0f;


		[RuntimeInitializeOnLoadMethod()]
		private static void Init()
		{
			if(GameObject.FindObjectOfType<SIGVerseMenu>()!=null) { return; };

			if(!ConfigManager.Instance.configInfo.useSigverseMenu) { return; }

			GameObject sigverseMenuObjPrefab = (GameObject)Resources.Load(SIGVerseUtils.SIGVerseMenuResourcePath);

			GameObject sigverseMenuObj = Instantiate(sigverseMenuObjPrefab);
			sigverseMenuObj.name = SIGVerseMenuName;

			SIGVerseLogger.Info("SIGVerseMenu Start");
		}

		void Awake()
		{
			this.backgroundDarkColor   = this.backgroundImage.color;
			this.backgroundBrightColor = this.backgroundImage.color;
			this.backgroundBrightColor.a = 0.0f;

			Time.timeScale = 0.0f;

			DontDestroyOnLoad(this);

			this.rosIPObj  .GetComponent<Text>().text = ConfigManager.Instance.configInfo.rosbridgeIP;
			this.rosPortObj.GetComponent<Text>().text = ConfigManager.Instance.configInfo.rosbridgePort.ToString();

			this.canvas = this.transform.GetComponentInChildren<Canvas>().gameObject;

			this.timeValueText = this.timeValObj.GetComponent<Text>();

			this.mainPanelImage = this.mainPanel.GetComponent<Image>();

			this.startButtonText = this.startButton.GetComponentInChildren<Text>();

//			this.canSeeSubviewPanels = new bool[SubviewNum];

			this.canvas      .SetActive(true);
			this.mainPanel   .SetActive(true);
			this.subviewPanel.SetActive(false);
			this.infoPanel   .SetActive(false);
			this.messagePanel.SetActive(false);

			this.sceneNameObj.GetComponent<Text>().text = SceneManager.GetActiveScene().name;

			CreateEventSystem();

			if(ConfigManager.Instance.configInfo.isAutoStartWithMenu)
			{
				this.OnHiddingButtonClick();
				this.OnStartButtonClick();
			}

			SceneManager.sceneLoaded += OnSceneLoaded;
		}


		void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
		{
//			Debug.Log("scene name="+SceneManager.GetActiveScene().name);

			this.sceneNameObj.GetComponent<Text>().text = SceneManager.GetActiveScene().name;

			CreateEventSystem();
		}


		private static void CreateEventSystem()
		{
			EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem> ();

			if(eventSystem==null)
			{
				GameObject sigverseEventSystemObjPrefab = (GameObject)Resources.Load(SIGVerseUtils.EventSystemResourcePath);

				Instantiate(sigverseEventSystemObjPrefab);
			}
		}


		// Use this for initialization
		//void Start()
		//{
		//}


		// Update is called once per frame
		void Update()
		{
			this.elapsedTime += Time.deltaTime;

			this.timeValueText.text = this.elapsedTime.ToString(TimeFormat);
		}

		public void OnHiddingButtonClick()
		{
			if (this.mainPanelImage.enabled)
			{
				this.canSeeMainPanel    = this.mainPanelImage.enabled;
				this.canSeeSubviewPanel = this.subviewPanel  .activeSelf;
				this.canSeeInfoPanel    = this.infoPanel     .activeSelf;
				this.canSeeMessagePanel = this.messagePanel  .activeSelf;

				//for(int i=0; i<SubviewNum; i++)
				//{
				//	this.canSeeSubviewPanels[i] = this.subviewPanels[i].activeSelf;
				//}

				this.mainPanelImage .enabled = false;
				this.targetsOfHiding.SetActive(false);
				this.subviewPanel   .SetActive(false);
				this.infoPanel      .SetActive(false);
				this.messagePanel   .SetActive(false);

				//for(int i=0; i<SubviewNum; i++)
				//{
				//	this.subviewPanels[i].SetActive(false);
				//}
			}
			else
			{
				if (this.canSeeMainPanel)
				{
					this.mainPanelImage.enabled = true;
					this.targetsOfHiding.SetActive(true);
				}
				if (this.canSeeSubviewPanel)
				{
					this.subviewPanel.SetActive(true);
				}
				if (this.canSeeInfoPanel)
				{
					this.infoPanel.SetActive(true);
				}
				if (this.canSeeMessagePanel)
				{
					this.messagePanel.SetActive(true);
				}
				//for(int i=0; i<SubviewNum; i++)
				//{
				//	if (this.canSeeSubviewPanels[i])
				//	{
				//		this.subviewPanels[i].SetActive(true);
				//	}
				//}
			}
		}

		public void OnStartButtonClick()
		{
			if (this.isRunning)
			{
				Time.timeScale = 0.0f;
				this.startButtonText.text = "Start";
				this.isRunning = false;
			}
			else
			{
				Time.timeScale = DefaultTimeScale;
				this.startButtonText.text = "Stop";
				this.isRunning = true;
			}

			EventSystem.current.SetSelectedGameObject(null);
		}

		public void OnSubviewButtonClick()
		{
			if (this.subviewPanel.activeSelf)
			{
				this.subviewPanel.SetActive(false);
			}
			else
			{
				this.subviewPanel.SetActive(true);
			}
		}

		public void OnInfoButtonClick()
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

		public void OnMessageButtonClick()
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

		public void OnQuitButtonClick()
		{
			Debug.Log("Quit!!!");
			Application.Quit();
		}


		public void OnGUI ()
		{
			if (this.isRunning)
			{
				this.backgroundImage.color = this.backgroundBrightColor;
			}
			else
			{
				this.backgroundImage.color = this.backgroundDarkColor;
			}
		}


		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.pointerPressRaycast.gameObject == null) { return; }

			Transform selectedObj = eventData.pointerPressRaycast.gameObject.transform;

			do
			{
				bool isMajorPanel = 
					selectedObj.gameObject == this.mainPanel    ||
					selectedObj.gameObject == this.infoPanel    ||
					selectedObj.gameObject == this.messagePanel;

				bool isSubviewPanels = 
					selectedObj.gameObject == this.subviewPanel ||
					this.subviewPanels.Select(panel => panel).Any(panel => panel == selectedObj.gameObject);

				if (isMajorPanel)
				{
					this.draggingPanel = selectedObj.gameObject;

					this.panelOperationType = PanelOperationType.Move;

					break;
				}
				else if(isSubviewPanels)
				{
					this.draggingPanel = selectedObj.gameObject;

					this.expandingRectTransform  = this.draggingPanel.GetComponent<RectTransform>();

					float rightEnd = this.draggingPanel.transform.position.x + expandingRectTransform.rect.width;
					float lowerEnd = this.draggingPanel.transform.position.y - expandingRectTransform.rect.height;

					bool isExpandingX = eventData.pressPosition.x >= rightEnd - DraggingThreshold && eventData.pressPosition.x <= rightEnd;
					bool isExpandingY = eventData.pressPosition.y <= lowerEnd + DraggingThreshold && eventData.pressPosition.y >= lowerEnd;

					if(selectedObj.gameObject.GetInstanceID() == this.subviewPanel.GetInstanceID())
					{
						isExpandingY = false;
					}

					if(isExpandingX && isExpandingY)
					{
						this.panelOperationType = PanelOperationType.ExpandXY;
					}
					else if(isExpandingX)
					{
						this.panelOperationType = PanelOperationType.ExpandX;
					}
					else if(isExpandingY)
					{
						this.panelOperationType = PanelOperationType.ExpandY;
					}
					else
					{
						this.panelOperationType = PanelOperationType.Move;
					}

					break;
				}

				selectedObj = selectedObj.transform.parent;

			} while (selectedObj.transform.parent != null);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (this.draggingPanel == null) { return; }

			switch(this.panelOperationType)
			{
				case PanelOperationType.Move:
				{
					this.draggingPanel.transform.position += (Vector3)eventData.delta;
					break;
				}
				case PanelOperationType.ExpandXY:
				{
					this.ResizePanelWidth(eventData);
					this.ResizePanelHeight(eventData);
					break;
				}
				case PanelOperationType.ExpandX:
				{
					this.ResizePanelWidth(eventData);
					break;
				}
				case PanelOperationType.ExpandY:
				{
					this.ResizePanelHeight(eventData);
					break;
				}
			}
		}

		private void ResizePanelWidth(PointerEventData eventData)
		{
			float panelWidth = eventData.position.x - this.expandingRectTransform.position.x;

			if(panelWidth >= MinimumPanelSize)
			{
				float deltaWidth = panelWidth - this.expandingRectTransform.rect.width;

				this.expandingRectTransform.sizeDelta = new Vector2(this.expandingRectTransform.sizeDelta.x + deltaWidth, this.expandingRectTransform.sizeDelta.y);
			}
		}

		private void ResizePanelHeight(PointerEventData eventData)
		{
			float panelHeight = this.expandingRectTransform.position.y - eventData.position.y;

			if(panelHeight >= MinimumPanelSize)
			{
				float deltaHeight = panelHeight - this.expandingRectTransform.rect.height;

				this.expandingRectTransform.sizeDelta = new Vector2(this.expandingRectTransform.sizeDelta.x, this.expandingRectTransform.sizeDelta.y + deltaHeight);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this.draggingPanel = null;
		}
	}
}
