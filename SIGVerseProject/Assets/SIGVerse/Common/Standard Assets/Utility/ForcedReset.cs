using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets_1_1_2.CrossPlatformInput;

#pragma warning disable 618

[RequireComponent(typeof (GUITexture))]
public class ForcedReset : MonoBehaviour
{
    private void Update()
    {
        // if we have forced a reset ...
        if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
        {
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).path);
        }
    }
}
