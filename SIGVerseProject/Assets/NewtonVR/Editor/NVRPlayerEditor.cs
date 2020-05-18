using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace NewtonVR
{
    [CustomEditor(typeof(NVRPlayer))]
    public class NVRPlayerEditor : Editor
    {
        private const string OculusDefine = "NVR_Oculus";

        private static bool hasReloaded = false;
        private static bool waitingForReload = false;
        private static DateTime startedWaitingForReload;

        //private static bool hasOculusSDK = false;
        private static bool hasOculusSDKDefine = false;

        private static string progressBarMessage = null;

        private static string CheckForUpdatesKey = "NewtonVRCheckForUpdates";

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            hasReloaded = true;

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            hasOculusSDKDefine = scriptingDefines.Contains(OculusDefine);

            waitingForReload = false;
            ClearProgressBar();
        }

        private void RemoveDefine(string define)
        {
            DisplayProgressBar("Removing support for " + define);
            waitingForReload = true;
            startedWaitingForReload = DateTime.Now;

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Remove(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }

        private void AddDefine(string define)
        {
            DisplayProgressBar("Setting up support for " + define);
            waitingForReload = true;
            startedWaitingForReload = DateTime.Now;

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Add(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);

            if (PlayerSettings.virtualRealitySupported == false)
            {
                PlayerSettings.virtualRealitySupported = true;
            }
        }

        private static void DisplayProgressBar(string newMessage = null)
        {
            if (newMessage != null)
            {
                progressBarMessage = newMessage;
            }

            EditorUtility.DisplayProgressBar("NewtonVR", progressBarMessage, UnityEngine.Random.value); // :D
        }

        private static void ClearProgressBar()
        {
            progressBarMessage = null;
            EditorUtility.ClearProgressBar();
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private static void HasWaitedLongEnough()
        {
            TimeSpan waitedTime = DateTime.Now - startedWaitingForReload;
            if (waitedTime.TotalSeconds > 15)
            {
                DidReloadScripts();
            }
        }

        public override void OnInspectorGUI()
        {
            NVRPlayer player = (NVRPlayer)target;
            if (PlayerPrefs.HasKey(CheckForUpdatesKey) == false || PlayerPrefs.GetInt(CheckForUpdatesKey) != System.Convert.ToInt32(player.NotifyOnVersionUpdate))
            {
                PlayerPrefs.SetInt("NewtonVRCheckForUpdates", System.Convert.ToInt32(player.NotifyOnVersionUpdate));
            }

            if (hasReloaded == false)
                DidReloadScripts();

            if (waitingForReload)
                HasWaitedLongEnough();

            player.OculusSDKEnabled = hasOculusSDKDefine;

            //bool installOculusSDK = false;
            bool enableOculusSDK = player.OculusSDKEnabled;

//            Debug.Log("hasOculusSDK="+hasOculusSDK);

            EditorGUILayout.BeginHorizontal();
            enableOculusSDK = EditorGUILayout.Toggle("Enable Oculus SDK", player.OculusSDKEnabled);
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(10);

            GUILayout.Label("Model override for all SDKs");
            bool modelOverrideAll = EditorGUILayout.Toggle("Override hand models for all SDKs", player.OverrideAll);
            EditorGUILayout.BeginFadeGroup(1);
            using (new EditorGUI.DisabledScope(modelOverrideAll == false))
            {
                player.OverrideAllLeftHand = (GameObject)EditorGUILayout.ObjectField("Left Hand", player.OverrideAllLeftHand, typeof(GameObject), false);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                player.OverrideAllLeftHandPhysicalColliders = (GameObject)EditorGUILayout.ObjectField("Left Hand Physical Colliders", player.OverrideAllLeftHandPhysicalColliders, typeof(GameObject), false);
                GUILayout.EndHorizontal();
                player.OverrideAllRightHand = (GameObject)EditorGUILayout.ObjectField("Right Hand", player.OverrideAllRightHand, typeof(GameObject), false);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                player.OverrideAllRightHandPhysicalColliders = (GameObject)EditorGUILayout.ObjectField("Right Hand Physical Colliders", player.OverrideAllRightHandPhysicalColliders, typeof(GameObject), false);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
            if (modelOverrideAll == true)
            {
                player.OverrideOculus = false;
            }
            if (player.OverrideAll != modelOverrideAll)
            {
                EditorUtility.SetDirty(target);
                player.OverrideAll = modelOverrideAll;
            }

            GUILayout.Space(10);

            //if (player.OculusSDKEnabled == true)
            //{
            //    GUILayout.Label("Model override for Oculus SDK");
            //    using (new EditorGUI.DisabledScope(hasOculusSDK == false))
            //    {
            //        bool modelOverrideOculus = EditorGUILayout.Toggle("Override hand models for Oculus SDK", player.OverrideOculus);
            //        EditorGUILayout.BeginFadeGroup(Convert.ToSingle(modelOverrideOculus));
            //        using (new EditorGUI.DisabledScope(modelOverrideOculus == false))
            //        {
            //            player.OverrideOculusLeftHand = (GameObject)EditorGUILayout.ObjectField("Left Hand", player.OverrideOculusLeftHand, typeof(GameObject), false);
            //            GUILayout.BeginHorizontal();
            //            GUILayout.Space(20);
            //            player.OverrideOculusLeftHandPhysicalColliders = (GameObject)EditorGUILayout.ObjectField("Left Hand Physical Colliders", player.OverrideOculusLeftHandPhysicalColliders, typeof(GameObject), false);
            //            GUILayout.EndHorizontal();
            //            player.OverrideOculusRightHand = (GameObject)EditorGUILayout.ObjectField("Right Hand", player.OverrideOculusRightHand, typeof(GameObject), false);
            //            GUILayout.BeginHorizontal();
            //            GUILayout.Space(20);
            //            player.OverrideOculusRightHandPhysicalColliders = (GameObject)EditorGUILayout.ObjectField("Right Hand Physical Colliders", player.OverrideOculusRightHandPhysicalColliders, typeof(GameObject), false);
            //            GUILayout.EndHorizontal();
            //        }
            //        EditorGUILayout.EndFadeGroup();

            //        if (modelOverrideOculus == true)
            //        {
            //            player.OverrideAll = false;
            //        }
            //        if (player.OverrideOculus != modelOverrideOculus)
            //        {
            //            EditorUtility.SetDirty(target);
            //            player.OverrideOculus = modelOverrideOculus;
            //        }
            //    }
            //}

            GUILayout.Space(10);

            if (enableOculusSDK == false && player.OculusSDKEnabled == true)
            {
                RemoveDefine(OculusDefine);
            }
            else if (enableOculusSDK == true && player.OculusSDKEnabled == false)
            {
                AddDefine(OculusDefine);
            }

            DrawDefaultInspector();

            if (waitingForReload == true || string.IsNullOrEmpty(progressBarMessage) == false)
            {
                DisplayProgressBar();
            }
            if (GUI.changed)
            {
                if (Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }
    }
}
