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
	public interface ISubviewHandler : IEventSystemHandler
	{
		/// <summary>
		/// Update a camera information of a subview
		/// </summary>
		/// <param name="subviewNumber">subview number (this number is greater than 0)</param>
		/// <param name="camera"></param>
		void OnUpdateSubviewCamera(int subviewNumber, Camera camera);

		/// <summary>
		/// Update a camera information of a subview
		/// </summary>
		/// <param name="subviewNumber">subview number (this number is greater than 0)</param>
		/// <param name="camera"></param>
		/// <param name="isShowing"></param>
		void OnUpdateSubviewCamera(int subviewNumber, Camera camera, bool isShowing);
	}


	public class SubviewManager : MonoBehaviour, ISubviewHandler
	{
		public const string ButtonTextOff = "OFF";
		public const string ButtonTextOn  = "ON";

		private const int SubviewNum = 4;

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
		private Texture2D subviewTextureDefault;

		void Awake()
		{
			this.subviewCameras        = new Camera       [SubviewNum];
			this.subviewButtons        = new Button       [SubviewNum];
			this.subviewDropdowns      = new Dropdown     [SubviewNum];
			this.subviewRectTransforms = new RectTransform[SubviewNum];
			this.subviewRectSizes      = new Vector2      [SubviewNum];
			this.subviewRenderTextures = new RenderTexture[SubviewNum];
			this.subviewMaterials      = new Material     [SubviewNum];
			this.subviewImages         = new Image        [SubviewNum];

			this.unlitTexturShader = Shader.Find("Unlit/Texture");

			this.subviewTextureDefault = new Texture2D(1, 1);
			this.subviewTextureDefault.SetPixel(0, 0, Color.gray);
			this.subviewTextureDefault.Apply();


			for (int i = 0; i < SubviewNum; i++)
			{
				this.subviewButtons[i]     = this.subviewObjects[i].GetComponentInChildren<Button>();
				this.subviewDropdowns[i]   = this.subviewObjects[i].GetComponentInChildren<Dropdown>();

				Image[] subviewPanelImages = this.subviewPanels[i].GetComponentsInChildren<Image>().Where(image => this.subviewPanels[i] != image.gameObject).ToArray();

				this.subviewRectTransforms[i] = subviewPanelImages[0].GetComponent<RectTransform>();

				this.subviewImages[i] = subviewPanelImages[0].GetComponent<Image>();

				this.UpdateRenderTexture(i);
			}

			this.OnActiveSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());

			this.UpdateButtonStates();

			SceneManager.activeSceneChanged += OnActiveSceneChanged;
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

			// Have to reset the material when creating the render texture
			this.subviewMaterials[index] = new Material(this.unlitTexturShader);

			if(this.subviewCameras[index]!=null)
			{
				this.subviewMaterials[index].mainTexture = this.subviewRenderTextures[index];
			}
			else
			{
				this.subviewMaterials[index].mainTexture = this.subviewTextureDefault;
			}

			this.subviewImages[index].material = this.subviewMaterials[index];
		}



		private void OnActiveSceneChanged( Scene i_preChangedScene, Scene i_postChangedScene )
		{
			this.UpdateCameraList();
			
		}

		public void UpdateCameraList()
		{
			Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

			this.dropdownAllCameras = new Camera[cameras.Length + 1];
			this.dropdownAllCameras[0] = null;
			cameras.CopyTo(this.dropdownAllCameras, 1);

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

			for(int i=0; i<SubviewNum; i++)
			{
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
	
		void Update ()
		{
		
		}


		void LateUpdate()
		{
			for (int i = 0; i < SubviewNum; i++)
			{
				if (this.subviewCameras[i] != null)
				{
					RenderTexture renderTextureTmp = this.subviewCameras[i].targetTexture;

					// Resize
					if(this.subviewRectTransforms[i].rect.width!=this.subviewRectSizes[i].x || this.subviewRectTransforms[i].rect.height!=this.subviewRectSizes[i].y)
					{
						this.UpdateRenderTexture(i);
					}

					// Render
					this.subviewCameras[i].targetTexture = this.subviewRenderTextures[i];

					this.subviewCameras[i].Render();

					this.subviewCameras[i].targetTexture = renderTextureTmp;
				}
			}
		}

		public void OnUpdateCameraListButtonClick()
		{
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

		public void OnSubviewDropdownValueChanged(Dropdown dropdown)
		{
			for(int i=0; i<SubviewNum; i++)
			{
				if(this.subviewDropdowns[i] == dropdown)
				{
					this.subviewCameras[i] = this.dropdownAllCameras[dropdown.value];

					this.UpdateRenderTexture(i);
					break;
				}
			}
		}


		private void UpdateButtonState(GameObject subviewPanel, Button button)
		{
			Text  buttonText  = button.GetComponentInChildren<Text>();

			if(subviewPanel.activeSelf)
			{
				button.GetComponentInChildren<Image>().color = new Color(200/255f, 255/255f, 175/255f, 255/255f); // Light green
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


		public void OnUpdateSubviewCamera(int subviewNumber, Camera camera)
		{
			this.OnUpdateSubviewCamera(subviewNumber, camera, true);
		}


		public void OnUpdateSubviewCamera(int subviewNumber, Camera camera, bool isShowing)
		{
			if (subviewNumber <= 0)
			{
				throw new ArgumentException("Subview number is not greater than 0", "subviewNumber");
			}

			int index = subviewNumber - 1;

			// Update button state
			if(isShowing)
			{
				this.subviewPanels[index].SetActive(true);
			}
			else
			{
				this.subviewPanels[index].SetActive(false);
			}

			this.UpdateButtonState(this.subviewPanels[index], this.subviewButtons[index]);

			// Update dropdown
			for(int i=0; i<this.dropdownAllCameras.Length; i++)
			{
				if(camera == this.dropdownAllCameras[i])
				{
					this.subviewDropdowns[index].value = i;
					break;
				}
			}

			// Update camera information
			this.subviewCameras[index] = camera;

			// Update render texture
			this.UpdateRenderTexture(index);
		}
	}
}

