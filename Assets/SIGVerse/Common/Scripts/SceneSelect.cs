using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

namespace SIGVerse.Common
{
	public class SceneSelect : MonoBehaviour, IDragHandler
	{
		[HeaderAttribute("Input Fields")]
		public GameObject sceneVal;
		public GameObject rosIPVal;
		public GameObject rosPortVal;

		// Use this for initialization
		void Start()
		{
			// Update Scene dropdown
			Dropdown sceneDropdown = this.sceneVal.GetComponent<Dropdown>();

			sceneDropdown.options.Clear();

			foreach (string sceneName in EditorConstantsManager.sceneNameArray)
			{
				if (sceneName == EditorConstantsManager.sceneNameArray[0]) { continue; } // Except the title scene

				Dropdown.OptionData optionData = new Dropdown.OptionData();

				optionData.text = sceneName;

				sceneDropdown.options.Add(optionData);
			}


			sceneDropdown.value = 0;
			sceneDropdown.captionText.text = "";

			if (sceneDropdown.options.Count > 0)
			{
				sceneDropdown.captionText.text = sceneDropdown.options[sceneDropdown.value].text;
			}

			// Update ROS Bridge IP
			this.rosIPVal.GetComponentInChildren<InputField>().text = ConfigManager.Instance.configInfo.rosbridgeIP;

			// Update ROS Bridge Port 
			this.rosPortVal.GetComponentInChildren<InputField>().text = ConfigManager.Instance.configInfo.rosbridgePort.ToString();
		}

		//// Update is called once per frame
		//void Update ()
		//{
		//}

		private string GetSceneName(string scenePath)
		{
			string[] pathArray = scenePath.Split('/');

			string fileName = pathArray[pathArray.Length - 1];

			return fileName.Substring(0, fileName.Length - 6); // Trim the extension
		}


		public void UpdateConfigRosIP(Text rosIPText)
		{
			ConfigManager.Instance.configInfo.rosbridgeIP = rosIPText.text;
		}

		public void UpdateConfigRosPort(Text rosPortText)
		{
			ConfigManager.Instance.configInfo.rosbridgePort = int.Parse(rosPortText.text);
		}

		public void LoadScene(Text sceneNameText)
		{
			ConfigManager.SaveConfig();

			SceneManager.LoadScene(sceneNameText.text);
		}


		public void OnDrag(PointerEventData eventData)
		{
			this.transform.position += (Vector3)eventData.delta;
		}
	}
}

