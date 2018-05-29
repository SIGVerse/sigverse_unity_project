using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace SIGVerse.Common
{
	public enum SubviewType
	{
		Subview1 = 1,
		Subview2,
		Subview3,
		Subview4,
	}

	public enum SubviewPositionType
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight, 
	}

	public interface ISubviewHandler : IEventSystemHandler
	{
		/// <summary>
		/// Update a camera information of a subview
		/// </summary>
		/// <param name="subviewType">subview type</param>
		/// <param name="camera"></param>
		void OnSetSubviewCamera(SubviewType subviewType, Camera camera);

		/// <summary>
		/// Update a camera information of a subview
		/// </summary>
		/// <param name="subviewType">subview type</param>
		/// <param name="camera"></param>
		/// <param name="isShowing"></param>
		void OnSetSubviewCamera(SubviewType subviewType, Camera camera, bool isShowing);

		/// <summary>
		/// Update position of a subview
		/// </summary>
		/// <param name="subviewType">subview type</param>
		/// <param name="subviewPositionNumber">0: buttom right, 1: buttom left</param>
		void OnSetSubviewPosition(SubviewType subviewType, SubviewPositionType subviewPositionType, float offsetX, float offsetY);
	}


	public class SubviewManager : MonoBehaviour, ISubviewHandler
	{
		public const string ButtonTextOff = "OFF";
		public const string ButtonTextOn  = "ON";

		private const float DefaultPositionOffset = 15f;

		private const int MaxCameraListUpdateInterval = 500;


		private const int SubviewNum = 4;

		private readonly Color ColorLightGreen = new Color(200/255f, 255/255f, 175/255f, 255/255f);


		//----------------------------------------

		[HeaderAttribute("Subview Panel")]
		public GameObject[] subviewObjects;

		[HeaderAttribute("Subview Panels")]
		public GameObject[] subviewPanels;

		//----------------------------------------

		private Camera[] dropdownAllCameras;

		private Camera[]        subviewCameras;
		private Button[]        subviewButtons;
		private Dropdown[]      subviewDropdowns;
		private RectTransform[] subviewRectTransforms;
		private Vector2[]       subviewRectSizes;
		private RenderTexture[] subviewRenderTextures;
		private Material[]      subviewMaterials;
		private Image[]         subviewImages;

		private Shader unlitTexturShader;

		private DateTime   cameraListLastUpdateTime;
		private DateTime[] subviewLastUpdateTime;


		void Awake()
		{
			try
			{
				this.subviewCameras        = new Camera       [SubviewNum];
				this.subviewButtons        = new Button       [SubviewNum];
				this.subviewDropdowns      = new Dropdown     [SubviewNum];
				this.subviewRectTransforms = new RectTransform[SubviewNum];
				this.subviewRectSizes      = new Vector2      [SubviewNum];
				this.subviewRenderTextures = new RenderTexture[SubviewNum];
				this.subviewMaterials      = new Material     [SubviewNum];
				this.subviewImages         = new Image        [SubviewNum];
				this.subviewLastUpdateTime = new DateTime     [SubviewNum];

				this.unlitTexturShader = (Shader)Resources.Load(SIGVerseUtils.UnlitShaderResourcePath);

				// Initialize Subviews
				for(int i=0; i<this.subviewPanels.Length; i++)
				{
					this.subviewPanels[i].SetActive(false);

					// Set subviews position (All panels are the same size)
					RectTransform rectTransform = this.subviewPanels[i].GetComponent<RectTransform>();

					float posX = Screen.width  - DefaultPositionOffset - rectTransform.rect.width;
					float posY = Screen.height - DefaultPositionOffset - i * rectTransform.rect.height;

					if(posY-rectTransform.rect.height < 0) { posY = rectTransform.rect.height; }
				
					rectTransform.position = new Vector3(posX, posY, 0.0f);
				}

				this.cameraListLastUpdateTime = DateTime.MinValue;

				for (int i = 0; i < SubviewNum; i++)
				{
					this.subviewCameras[i] = this.subviewPanels[i].GetComponentInChildren<Camera>();

					this.subviewButtons[i]     = this.subviewObjects[i].GetComponentInChildren<Button>();
					this.subviewDropdowns[i]   = this.subviewObjects[i].GetComponentInChildren<Dropdown>();

					Image[] subviewPanelImages = this.subviewPanels[i].GetComponentsInChildren<Image>().Where(image => this.subviewPanels[i] != image.gameObject).ToArray();

					this.subviewRectTransforms[i] = subviewPanelImages[0].GetComponent<RectTransform>();

					this.subviewImages[i] = subviewPanelImages[0].GetComponent<Image>();

					this.subviewDropdowns[i].value = 0;

					this.UpdateRenderTexture(i);

					this.subviewImages[i].material = null;

					this.subviewLastUpdateTime[i] = DateTime.MinValue;
				}

				this.OnActiveSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());

				this.UpdateButtonStates();

				SceneManager.activeSceneChanged += OnActiveSceneChanged;
			}
			catch(Exception ex)
			{
				SIGVerseLogger.Error(ex.Message);
				SIGVerseLogger.Error(ex.StackTrace);
			}
		}


		private void UpdateRenderTexture(int index)
		{
			this.subviewRectSizes[index] = new Vector2(this.subviewRectTransforms[index].rect.width, this.subviewRectTransforms[index].rect.height);

			// Recreate the render texture
			if(this.subviewRenderTextures[index] != null)
			{
				this.subviewRenderTextures[index].Release();
			}
			this.subviewRenderTextures[index] = new RenderTexture((int)this.subviewRectSizes[index].x, (int)this.subviewRectSizes[index].y, 16, RenderTextureFormat.ARGB32);
			this.subviewRenderTextures[index].Create();

			this.subviewMaterials[index] = new Material(this.unlitTexturShader);
			this.subviewMaterials[index].mainTexture = this.subviewRenderTextures[index];

			this.subviewImages[index].material = this.subviewMaterials[index];

			this.subviewCameras[index].targetTexture = this.subviewRenderTextures[index];
		}



		private void OnActiveSceneChanged( Scene i_preChangedScene, Scene i_postChangedScene )
		{
			this.UpdateCameraList();
		}

		public void UpdateCameraList()
		{
			Camera[] savedCameras = new Camera[SubviewNum];

			// Save cameras info
			if(this.dropdownAllCameras!=null)
			{
				for(int i=0; i<SubviewNum; i++)
				{
					savedCameras[i] = this.dropdownAllCameras[this.subviewDropdowns[i].value];
				}
			}


			// Recreate the camera list
			Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();

			// Excluding subview cameras
			Camera[] cameras = allCameras.Except(this.subviewCameras).ToArray();

			// Recreate dropdownAllCameras
			this.dropdownAllCameras = new Camera[cameras.Length + 1];
			this.dropdownAllCameras[0] = null;
			cameras.CopyTo(this.dropdownAllCameras, 1);


			// Recreate the dropdown option list
			List<Dropdown.OptionData> dropdownOptions = new List<Dropdown.OptionData>();

			foreach(Camera camera in this.dropdownAllCameras)
			{
				Dropdown.OptionData optionData = new Dropdown.OptionData();

				if(camera == null)
				{
					optionData.text = "  -  ";
				}
				else
				{
					optionData.text = camera.name + "   (" + GetDropdownName(camera) + ")";
				}

				dropdownOptions.Add(optionData);
			}

			// Update Subviews info
			for(int i=0; i<SubviewNum; i++)
			{
				// Update a linking of dropdown
				for(int j=0; j<this.dropdownAllCameras.Length; j++)
				{
					if(savedCameras[i] == this.dropdownAllCameras[j])
					{
						this.subviewDropdowns[i].value = j;
					}
				}

				// Update a options of dropdown
				this.subviewDropdowns[i].options = dropdownOptions;
			}
		}


		private static string GetDropdownName(Camera camera)
		{
			Transform obj = camera.transform;

			string dropdownName = "/" + obj.name;

			while(obj.parent != null)
			{
				obj = obj.transform.parent;

				dropdownName = "/" + obj.name + dropdownName;
			}

			return dropdownName;
		}


		void Start ()
		{
		}

		void Update()
		{
		}


		void LateUpdate()
		{
			for (int i = 0; i < SubviewNum; i++)
			{
				if (this.subviewDropdowns[i].value != 0)
				{
					// Resize panels
					if (this.subviewRectTransforms[i].rect.width != this.subviewRectSizes[i].x || this.subviewRectTransforms[i].rect.height != this.subviewRectSizes[i].y)
					{
						this.UpdateRenderTexture(i);
					}

					// Move panels
					if (this.dropdownAllCameras[this.subviewDropdowns[i].value] != null)
					{
						this.subviewCameras[i].CopyFrom(this.dropdownAllCameras[this.subviewDropdowns[i].value]);
						this.subviewCameras[i].targetTexture = this.subviewRenderTextures[i];
					}
				}
			}
		}

		public void OnUpdateCameraListButtonClick()
		{
			if((DateTime.Now - this.cameraListLastUpdateTime).TotalMilliseconds < MaxCameraListUpdateInterval)
			{
				SIGVerseLogger.Warn("The update time interval of the camera list is too short. (<" + MaxCameraListUpdateInterval + "[ms])");
				return;
			}

			this.cameraListLastUpdateTime = DateTime.Now;

			this.UpdateCameraList();
		}


		public void OnSubviewButtonClick(Dropdown dropdown)
		{
			for(int i=0; i<SubviewNum; i++)
			{
				if(this.subviewDropdowns[i] == dropdown)
				{
					if(this.subviewPanels[i].activeSelf)
					{
						this.subviewPanels[i].SetActive(false);
					}
					else
					{
						this.subviewPanels[i].SetActive(true);
					}
					this.UpdateButtonState(this.subviewPanels[i], this.subviewButtons[i]);
					break;
				}
			}
		}

		private void OnSubviewDropdownValueChanged(Dropdown dropdown)
		{
			for(int i=0; i<SubviewNum; i++)
			{
				if(this.subviewDropdowns[i] == dropdown)
				{
					this.ChangeCamera(i, this.dropdownAllCameras[dropdown.value]);
					break;
				}
			}
		}

		private void ChangeCamera(int index, Camera camera)
		{
			// Update a linking of dropdown
			for (int i = 0; i < this.dropdownAllCameras.Length; i++)
			{
				if (camera == this.dropdownAllCameras[i])
				{
					this.subviewDropdowns[index].value = i;
					break;
				}
			}

			// Update a material
			if (this.subviewDropdowns[index].value != 0)
			{
				this.subviewImages[index].material = this.subviewMaterials[index];
			}
			else
			{
				this.subviewImages[index].material = null;
			}
		}


		private void UpdateButtonState(GameObject subviewPanel, Button button)
		{
			Text  buttonText  = button.GetComponentInChildren<Text>();

			if(subviewPanel.activeSelf)
			{
				button.GetComponentInChildren<Image>().color = ColorLightGreen;
				buttonText.text = ButtonTextOn;
			}
			else
			{
				button.GetComponentInChildren<Image>().color = Color.white;
				buttonText.text = ButtonTextOff;
			}
		}

		private void UpdateButtonStates()
		{
			for(int i=0; i<SubviewNum; i++)
			{
				this.UpdateButtonState(this.subviewPanels[i], this.subviewButtons[i]);
			}
		}


		public static void SetSubviewCamera(SubviewType subviewType, Camera camera)
		{
			SIGVerseMenu sigverseMenu = GameObject.FindObjectOfType<SIGVerseMenu>();

			if(sigverseMenu == null)
			{
				SIGVerseLogger.Warn("SIGVerseMenu is not exists.");
				return;
			}

			ExecuteEvents.Execute<ISubviewHandler>
			(
				target: sigverseMenu.gameObject,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnSetSubviewCamera(subviewType, camera)
			);
		}

		public void OnSetSubviewCamera(SubviewType subviewType, Camera camera)
		{
			this.OnSetSubviewCamera(subviewType, camera, true);
		}


		public void OnSetSubviewCamera(SubviewType subviewType, Camera camera, bool isShowing)
		{
			int index = (int)subviewType - 1;

			if((DateTime.Now - this.subviewLastUpdateTime[index]).TotalMilliseconds < MaxCameraListUpdateInterval)
			{
				SIGVerseLogger.Warn("The update time interval of Subview is too short. (<" + MaxCameraListUpdateInterval + "[ms])");
				return;
			}

			this.subviewLastUpdateTime[index] = DateTime.Now;

			// Update button state
			if (isShowing)
			{
				this.subviewPanels[index].SetActive(true);
			}
			else
			{
				this.subviewPanels[index].SetActive(false);
			}

			this.UpdateButtonState(this.subviewPanels[index], this.subviewButtons[index]);

			// Update a camera info
			this.ChangeCamera(index, camera);
		}

		public void OnSetSubviewPosition(SubviewType subviewType, SubviewPositionType subviewPositionType, float offsetX, float offsetY)
		{
			int index = (int)subviewType - 1;

			this.subviewLastUpdateTime[index] = DateTime.Now;

			// Change subviews position
			RectTransform rectTransform = this.subviewPanels[index].GetComponent<RectTransform>();

			switch (subviewPositionType)
			{
				case SubviewPositionType.TopLeft:
				{
					rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
					rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
					rectTransform.pivot     = new Vector2(0.0f, 1.0f);

					float posX = + offsetX;
					float posY = - offsetY + Screen.height;

					rectTransform.position = new Vector3(posX, posY, 0.0f);

					break;
				}
				case SubviewPositionType.TopRight:
				{
					rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
					rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
					rectTransform.pivot     = new Vector2(1.0f, 1.0f);

					float posX = - offsetX + Screen.width;
					float posY = - offsetY + Screen.height;

					rectTransform.position = new Vector3(posX, posY, 0.0f);

					break;
				}
				case SubviewPositionType.BottomLeft:
				{
					rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
					rectTransform.anchorMax = new Vector2(0.0f, 0.0f);
					rectTransform.pivot     = new Vector2(0.0f, 0.0f);

					float posX = + offsetX;
					float posY = + offsetY;

					rectTransform.position = new Vector3(posX, posY, 0.0f);

					break;
				}
				case SubviewPositionType.BottomRight:
				{
					rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
					rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
					rectTransform.pivot     = new Vector2(1.0f, 0.0f);

					float posX = - offsetX + Screen.width;
					float posY = + offsetY;

					rectTransform.position = new Vector3(posX, posY, 0.0f);

					break;
				}
			}
		}

		public static void SetSubviewPosition(SubviewType subviewType, SubviewPositionType subviewPositionType, float offsetX, float offsetY)
		{
			SIGVerseMenu sigverseMenu = GameObject.FindObjectOfType<SIGVerseMenu>();

			if(sigverseMenu == null)
			{
				SIGVerseLogger.Warn("SIGVerseMenu is not exists.");
				return;
			}

			ExecuteEvents.Execute<ISubviewHandler>
			(
				target: sigverseMenu.gameObject,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnSetSubviewPosition(subviewType, subviewPositionType, offsetX, offsetY)
			);
		}

		public static void SetSubviewPosition(SubviewType subviewType, SubviewPositionType subviewPositionType)
		{
			SetSubviewPosition(subviewType, subviewPositionType, DefaultPositionOffset, DefaultPositionOffset);
		}
	}
}



