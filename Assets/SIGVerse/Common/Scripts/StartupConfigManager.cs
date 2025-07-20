using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SIGVerse.Common
{
	public class StartupConfigManager : MonoBehaviour
	{
		// Use this for initialization
		void Start()
		{
			// Update Scene Select values
			GameObject valuesObj = GameObject.Find("SceneSelect/Canvas/Panel/InputField/Value/");

			GameObject rosIPObj = valuesObj.transform.Find("ROSBridgeIPInputField").gameObject;
			rosIPObj.GetComponentInChildren<InputField>().text = ConfigManager.Instance.configInfo.rosbridgeIP;

			GameObject rosPortObj = valuesObj.transform.Find("ROSBridgePortInputField").gameObject;
			rosPortObj.GetComponentInChildren<InputField>().text = ConfigManager.Instance.configInfo.rosbridgePort.ToString();
		}
	}
}
